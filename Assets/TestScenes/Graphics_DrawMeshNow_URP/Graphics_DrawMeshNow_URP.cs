using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Graphics_DrawMeshNow_URP : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Mesh mesh;
    public Material material;

    private Vector3[] positions;
    private Quaternion[] rotations;

    void Start()
    {
        SetUp();
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        material.SetPass(0);
        for(int i=0; i<count; i++)
        {
            Graphics.DrawMeshNow(mesh,positions[i],rotations[i]);
        }
    }

    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    private void SetUp()
    {
        rotations = ObjectTransforms.GenerateObjRot(count);
        positions = ObjectTransforms.GenerateObjPos(count,transform.position,spacing);
    }

    void OnValidate()
    {
        SetUp();
    }

    void OnDrawGizmos()
    {
        for(int i=0;i<count;i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(positions[i], 0.5f);
        }
    }
}