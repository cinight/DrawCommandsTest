using System;
using System.Collections.Generic;
using UnityEngine;
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

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;

	public CommandBuffer_DrawProceduralIndirect_URP()
	{
	}

	public override void Create()
    {
        CleanUp();
        if (positionBuffer == null) positionBuffer = new ComputeBuffer(count, 16);
        if (argsBuffer == null) argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
	}


    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var pass = new CommandBuffer_DrawProceduralIndirect_URPPass(Event,count,spacing,anchor,material,positionBuffer,argsBuffer);
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

        private Vector4[] positions;
        private Quaternion[] rotations;

        private ComputeBuffer positionBuffer;
        private ComputeBuffer argsBuffer;

        public CommandBuffer_DrawProceduralIndirect_URPPass(RenderPassEvent renderPassEvent, 
        int count, float spacing, Vector3 anchor, Material material, ComputeBuffer posB, ComputeBuffer argB)
        {
            this.renderPassEvent = renderPassEvent;

            this.count = count;
            this.spacing = spacing;
            this.anchor = anchor;
            this.material = material;
            this.positionBuffer = posB;
            this.argsBuffer = argB;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if(rotations==null) rotations = ObjectTransforms.GenerateObjRot(count);
            if(positions==null) positions = ObjectTransforms.GenerateObjPosV4(count,anchor,spacing);

            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)3; // vertex count per instance
            args[1] = (uint)count;
            args[2] = (uint)0; // start vertex location
            args[3] = (uint)0; // start instance location

            cmd.SetBufferData(positionBuffer,positions);
            cmd.SetBufferData(argsBuffer,args);
            material.SetBuffer("positionBuffer", positionBuffer);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("CommandBuffer_DrawProceduralIndirect_URPPass");

            cmd.DrawProceduralIndirect(Matrix4x4.identity,material, 0, MeshTopology.Triangles, argsBuffer, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}