using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class CommandBuffer_DrawProceduralIndirect_URP : ScriptableRendererFeature
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;
    public Vector3 anchor;

    [Header("Content")]
    public Material material;

    [Header("Settings")]
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

    private GraphicsBuffer positionBuffer;
    private GraphicsBuffer argsBuffer;
    
    private CommandBuffer_DrawProceduralIndirect_URPPass pass;

	public CommandBuffer_DrawProceduralIndirect_URP()
	{
	}

	public override void Create()
    {
        CleanUp();
        if (positionBuffer == null) positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, 16);
        if (argsBuffer == null) argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, 5 * sizeof(uint));
        
        pass = new CommandBuffer_DrawProceduralIndirect_URPPass(Event,count,spacing,anchor,material,positionBuffer,argsBuffer);
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

        if (argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null;
        }
    }

    //========================================================================================================
    internal class CommandBuffer_DrawProceduralIndirect_URPPass : ScriptableRenderPass
    {
        private int count;
        private float spacing;
        private Vector3 anchor;

        private Material material;
        
        private uint[] args;
        private Vector4[] positions;
        private Quaternion[] rotations;

        private GraphicsBuffer positionBuffer;
        private GraphicsBuffer argsBuffer;
        
        private string passName = "CommandBuffer_DrawProceduralIndirect_URPPass";

        public CommandBuffer_DrawProceduralIndirect_URPPass(RenderPassEvent renderPassEvent, 
        int count, float spacing, Vector3 anchor, Material material, GraphicsBuffer posB, GraphicsBuffer argB)
        {
            this.renderPassEvent = renderPassEvent;

            this.count = count;
            this.spacing = spacing;
            this.anchor = anchor;
            this.material = material;
            this.positionBuffer = posB;
            this.argsBuffer = argB;
            
            Setup();
        }
        
        private void Setup()
        {
            if(rotations==null) rotations = ObjectTransforms.GenerateObjRot(count);
            if(positions==null) positions = ObjectTransforms.GenerateObjPosV4(count,anchor,spacing);

            args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)3; // vertex count per instance
            args[1] = (uint)count;
            args[2] = (uint)0; // start vertex location
            args[3] = (uint)0; // start instance location
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            material.SetBuffer("positionBuffer", positionBuffer);
            
            CommandBuffer cmd = CommandBufferPool.Get(passName);
            cmd.SetBufferData(positionBuffer,positions);
            cmd.SetBufferData(argsBuffer,args);
            cmd.DrawProceduralIndirect(Matrix4x4.identity,material, 0, MeshTopology.Triangles, argsBuffer, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        private class PassBufferData
        {
            internal GraphicsBuffer positionBuffer;
            internal Vector4[] positions;
            internal GraphicsBuffer argsBuffer;
            internal uint[] args;
        }
        
        private class PassData
        {
            internal Material material;
            internal GraphicsBuffer argsBuffer;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            
            //shouldn't blit from the backbuffer
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            //Set data to buffer
            material.SetBuffer("positionBuffer", positionBuffer);
            using (var builder = renderGraph.AddUnsafePass<PassBufferData>(passName+"_SetBuffer", out var passData))
            {
                //The compute will be culled because attachment dimensions is 0x0x0, so here we make sure it is not culled
                builder.AllowPassCulling(false);
                
                //Setup passData
                passData.positionBuffer = positionBuffer;
                passData.positions = positions;
                passData.argsBuffer = argsBuffer;
                passData.args = args;

                //Render function
                builder.SetRenderFunc((PassBufferData data, UnsafeGraphContext rgContext) =>
                {
                    CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd).SetBufferData(data.positionBuffer,data.positions);
                    CommandBufferHelpers.GetNativeCommandBuffer(rgContext.cmd).SetBufferData(data.argsBuffer,data.args);
                });
            }
            
            //Destination
            TextureHandle dest = resourceData.cameraColor;
            
            //To avoid error from material preview in the scene
            if(!dest.IsValid())
                return;
            
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                //Setup passData
                passData.material = material;
                passData.argsBuffer = argsBuffer;
                
                //setup builder
                builder.SetRenderAttachment(dest,0);
                builder.AllowPassCulling(false);
                
                //Render function
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    rgContext.cmd.DrawProceduralIndirect(Matrix4x4.identity,data.material, 0, MeshTopology.Triangles, data.argsBuffer, 0);
                });
            }
        }
    }
}