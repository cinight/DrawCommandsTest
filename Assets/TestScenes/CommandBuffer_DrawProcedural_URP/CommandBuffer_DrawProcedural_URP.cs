using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class CommandBuffer_DrawProcedural_URP : ScriptableRendererFeature
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;
    public Vector3 anchor;

    [Header("Content")]
    public Material material;

    [Header("Settings")]
    public RenderPassEvent Event = RenderPassEvent.AfterRenderingPostProcessing;

	public CommandBuffer_DrawProcedural_URP()
	{
	}

	public override void Create()
    {
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var pass = new CommandBuffer_DrawProcedural_URPPass(Event,count,spacing,anchor,material);
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
    }

    //========================================================================================================
    internal class CommandBuffer_DrawProcedural_URPPass : ScriptableRenderPass
    {
        private int count;
        private float spacing;
        private Vector3 anchor;

        private Material material;

        private Vector3[] positions;
        private Quaternion[] rotations;
        private Matrix4x4[] matrix;

        public CommandBuffer_DrawProcedural_URPPass(RenderPassEvent renderPassEvent, 
        int count, float spacing, Vector3 anchor, Material material)
        {
            this.renderPassEvent = renderPassEvent;

            this.count = count;
            this.spacing = spacing;
            this.anchor = anchor;
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
            CommandBuffer cmd = CommandBufferPool.Get("CommandBuffer_DrawMesh_URP");
            for(int i=0; i<count; i++)
            {
                cmd.DrawProcedural(matrix[i],material, 0, MeshTopology.Triangles, 3, 1);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}