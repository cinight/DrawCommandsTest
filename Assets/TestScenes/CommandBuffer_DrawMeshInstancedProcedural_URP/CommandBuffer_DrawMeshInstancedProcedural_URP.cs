using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class CommandBuffer_DrawMeshInstancedProcedural_URP : ScriptableRendererFeature
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;
    public Vector3 anchor;

    [Header("Content")]
    public Mesh mesh;
    public Material material;

    [Header("Settings")]
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    private GraphicsBuffer positionBuffer;
    private CommandBuffer_DrawMeshInstancedProcedural_URPPass pass;

	public CommandBuffer_DrawMeshInstancedProcedural_URP()
	{
	}

	public override void Create()
    {
        CleanUp();
        positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, 12);
        
        pass = new CommandBuffer_DrawMeshInstancedProcedural_URPPass(Event,count,spacing,anchor,mesh,material,positionBuffer);
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        CleanUp();
    }

    private void CleanUp()
    {
        if (positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
    }

    //========================================================================================================
    internal class CommandBuffer_DrawMeshInstancedProcedural_URPPass : ScriptableRenderPass
    {
        private int count;
        private float spacing;
        private Vector3 anchor;

        private Mesh mesh;
        private Material material;

        private Vector3[] positions;

        private GraphicsBuffer positionBuffer;
        
        private string passName = "CommandBuffer_DrawMeshInstancedIndirect_URP";

        public CommandBuffer_DrawMeshInstancedProcedural_URPPass(RenderPassEvent renderPassEvent, 
        int count, float spacing, Vector3 anchor, Mesh mesh, Material material, GraphicsBuffer posB)
        {
            this.renderPassEvent = renderPassEvent;

            this.count = count;
            this.spacing = spacing;
            this.anchor = anchor;
            this.mesh = mesh;
            this.material = material;
            this.positionBuffer = posB;

            Setup();
        }

        private void Setup()
        {
            if(positions==null) positions = ObjectTransforms.GenerateObjPos(count,anchor,spacing);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            material.SetBuffer("positionBuffer", positionBuffer);
            
            CommandBuffer cmd = CommandBufferPool.Get(passName);
            cmd.SetBufferData(positionBuffer,positions);
            cmd.DrawMeshInstancedProcedural(mesh,0,material,0,count);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        private class PassBufferData
        {
            internal GraphicsBuffer positionBuffer;
            internal Vector3[] positions;
        }
        
        private class PassData
        {
            internal Mesh mesh;
            internal Material material;
            internal int count;
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            material.SetBuffer("positionBuffer", positionBuffer);
            
            //Set data to buffer
            using (var builder = renderGraph.AddLowLevelPass<PassBufferData>(passName+"_SetBuffer", out var passData))
            {
                //The compute will be culled because attachment dimensions is 0x0x0, so here we make sure it is not culled
                builder.AllowPassCulling(false);
                
                //Setup passData
                passData.positionBuffer = positionBuffer;
                passData.positions = positions;

                //Render function
                builder.SetRenderFunc((PassBufferData data, LowLevelGraphContext rgContext) =>
                {
                    rgContext.legacyCmd.SetBufferData(data.positionBuffer,data.positions);
                });
            }
            
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                //Make sure the pass will not be culled
                builder.AllowPassCulling(false);

                //Setup passData
                passData.mesh = mesh;
                passData.material = material;
                passData.count = count;
                
                //Render function
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    rgContext.cmd.DrawMeshInstancedProcedural(data.mesh,0,data.material,0,data.count, null);
                });
            }
        }
    }
}