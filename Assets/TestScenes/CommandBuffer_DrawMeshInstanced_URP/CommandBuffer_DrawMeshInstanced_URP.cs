using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class CommandBuffer_DrawMeshInstanced_URP : ScriptableRendererFeature
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;
    public Vector3 anchor;

    [Header("Content")]
    public Mesh mesh;
    public Material material;
    public int shaderPass;

    [Header("Settings")]
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;
    
    private CommandBuffer_DrawMeshInstanced_URPPass pass;

	public CommandBuffer_DrawMeshInstanced_URP()
	{
	}

	public override void Create()
    {
        pass = new CommandBuffer_DrawMeshInstanced_URPPass(Event,count,spacing,anchor,mesh,material,shaderPass);
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
    }

    //========================================================================================================
    internal class CommandBuffer_DrawMeshInstanced_URPPass : ScriptableRenderPass
    {
        private int count;
        private float spacing;
        private Vector3 anchor;

        private Mesh mesh;
        private Material material;
        private int shaderPass;

        private Vector3[] positions;
        private Quaternion[] rotations;
        private Matrix4x4[] matrix;
        
        private string passName = "CommandBuffer_DrawMeshInstanced_URP";

        public CommandBuffer_DrawMeshInstanced_URPPass(RenderPassEvent renderPassEvent, 
        int count, float spacing, Vector3 anchor, Mesh mesh, Material material, int shaderPass)
        {
            this.renderPassEvent = renderPassEvent;

            this.count = count;
            this.spacing = spacing;
            this.anchor = anchor;
            this.mesh = mesh;
            this.material = material;
            this.shaderPass = shaderPass;

            SetUp();
        }

        private void SetUp()
        {
            if(rotations==null) rotations = ObjectTransforms.GenerateObjRot(count);
            if(positions==null) positions = ObjectTransforms.GenerateObjPos(count,anchor,spacing);
            if(matrix==null) matrix = new Matrix4x4[count];
            for(int i=0; i<count; i++)
            {
                matrix[i] = Matrix4x4.TRS( positions[i] , rotations[i] ,Vector3.one);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(passName);

            cmd.DrawMeshInstanced(mesh,0,material,shaderPass,matrix,count);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        private class PassData
        {
            internal Mesh mesh;
            internal Material material;
            internal int shaderPass;
            internal Matrix4x4[] matrix;
            internal int count;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            
            //shouldn't blit from the backbuffer
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            //Destination
            TextureHandle dest = resourceData.cameraColor;
            
            //To avoid error from material preview in the scene
            if(!dest.IsValid())
                return;
            
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                //Setup passData
                passData.mesh = mesh;
                passData.material = material;
                passData.shaderPass = shaderPass;
                passData.matrix = matrix;
                passData.count = count;
                
                //setup builder
                builder.SetRenderAttachment(dest,0);
                builder.AllowPassCulling(false);
                
                //Render function
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    rgContext.cmd.DrawMeshInstanced(data.mesh,0, data.material, data.shaderPass, data.matrix, data.count);

                });
            }
        }
    }
}