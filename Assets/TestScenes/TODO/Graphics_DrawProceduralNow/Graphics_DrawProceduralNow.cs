using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Graphics_DrawProceduralNow : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Material material;

    private Vector4[] positions;

    void Start()
    {
        SetUp();
    }

    void OnPostRender()
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
        positions = ObjectTransforms.GenerateObjPosV4(count,transform.position,spacing);
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
