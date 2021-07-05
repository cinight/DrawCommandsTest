using System;
using System.Collections.Generic;
using UnityEngine;
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

    private ComputeBuffer positionBuffer;

	public CommandBuffer_DrawMeshInstancedProcedural_URP()
	{
	}

	public override void Create()
    {
        CleanUp();
        positionBuffer = new ComputeBuffer(count, 12);
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var pass = new CommandBuffer_DrawMeshInstancedProcedural_URPPass(Event,count,spacing,anchor,mesh,material,positionBuffer);
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

        private ComputeBuffer positionBuffer;

        public CommandBuffer_DrawMeshInstancedProcedural_URPPass(RenderPassEvent renderPassEvent, 
        int count, float spacing, Vector3 anchor, Mesh mesh, Material material, ComputeBuffer posB)
        {
            this.renderPassEvent = renderPassEvent;

            this.count = count;
            this.spacing = spacing;
            this.anchor = anchor;
            this.mesh = mesh;
            this.material = material;
            this.positionBuffer = posB;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if(positions==null) positions = ObjectTransforms.GenerateObjPos(count,anchor,spacing);
            cmd.SetComputeBufferData(positionBuffer,positions);
            material.SetBuffer("positionBuffer", positionBuffer);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("CommandBuffer_DrawMeshInstancedIndirect_URP");

            cmd.DrawMeshInstancedProcedural(mesh,0,material,0,count);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}