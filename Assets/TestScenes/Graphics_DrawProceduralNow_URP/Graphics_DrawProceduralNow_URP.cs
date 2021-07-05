using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class Graphics_DrawProceduralNow_URP : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;
    public Transform offset;

    [Header("Content")]
    public Material material;

    private Vector4[] positions;

    void Start()
    {
        SetUp();
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        for(int i=0; i<count; i++)
        {
            material.SetPass(0);
            material.SetVector("position",positions[i]);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 3, 1);
        }
    }

    private void SetUp()
    {
        positions = ObjectTransforms.GenerateObjPosV4(count,offset.position,spacing);
    }

    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnValidate()
    {
        SetUp();
    }

    void OnGUI()
    {
        EditmodeUpdate.Update();
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
