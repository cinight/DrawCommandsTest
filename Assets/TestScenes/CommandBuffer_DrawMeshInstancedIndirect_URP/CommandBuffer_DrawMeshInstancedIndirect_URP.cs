using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class CommandBuffer_DrawMeshInstancedIndirect_URP : ScriptableRendererFeature
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
    private ComputeBuffer argsBuffer;

	public CommandBuffer_DrawMeshInstancedIndirect_URP()
	{
	}

	public override void Create()
    {
        CleanUp();
        positionBuffer = new ComputeBuffer(count, 12);
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var pass = new CommandBuffer_DrawMeshInstancedIndirect_URPPass(Event,count,spacing,anchor,mesh,material,positionBuffer,argsBuffer);
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
    internal class CommandBuffer_DrawMeshInstancedIndirect_URPPass : ScriptableRenderPass
    {
        private int count;
        private float spacing;
        private Vector3 anchor;

        private Mesh mesh;
        private Material material;

        private Vector3[] positions;

        private ComputeBuffer positionBuffer;
        private ComputeBuffer argsBuffer;

        public CommandBuffer_DrawMeshInstancedIndirect_URPPass(RenderPassEvent renderPassEvent, 
        int count, float spacing, Vector3 anchor, Mesh mesh, Material material, ComputeBuffer posB, ComputeBuffer argB)
        {
            this.renderPassEvent = renderPassEvent;

            this.count = count;
            this.spacing = spacing;
            this.anchor = anchor;
            this.mesh = mesh;
            this.material = material;
            this.positionBuffer = posB;
            this.argsBuffer = argB;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if(positions==null) positions = ObjectTransforms.GenerateObjPos(count,anchor,spacing);
            
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)count;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);

            cmd.SetBufferData(positionBuffer,positions);
            cmd.SetBufferData(argsBuffer,args);
            material.SetBuffer("positionBuffer", positionBuffer);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("CommandBuffer_DrawMeshInstancedIndirect_URP");

            cmd.DrawMeshInstancedIndirect(mesh,0,material,0,argsBuffer,0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}