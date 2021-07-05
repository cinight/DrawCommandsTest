using System;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("Settings")]
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

	public CommandBuffer_DrawMeshInstanced_URP()
	{
	}

	public override void Create()
    {
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var pass = new CommandBuffer_DrawMeshInstanced_URPPass(Event,count,spacing,anchor,mesh,material);
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

        private Vector3[] positions;
        private Quaternion[] rotations;
        private Matrix4x4[] matrix;

        public CommandBuffer_DrawMeshInstanced_URPPass(RenderPassEvent renderPassEvent, 
        int count, float spacing, Vector3 anchor, Mesh mesh, Material material)
        {
            this.renderPassEvent = renderPassEvent;

            this.count = count;
            this.spacing = spacing;
            this.anchor = anchor;
            this.mesh = mesh;
            this.material = material;

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
            CommandBuffer cmd = CommandBufferPool.Get("CommandBuffer_DrawMeshInstanced_URP");

            cmd.DrawMeshInstanced(mesh,0,material,0,matrix,count);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}