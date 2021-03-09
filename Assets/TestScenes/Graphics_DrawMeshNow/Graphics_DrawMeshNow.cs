using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphics_DrawMeshNow : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;
    public Transform anchor;

    [Header("Content")]
    public Mesh mesh;
    public Material material;

    private Vector3[] positions;
    private Quaternion[] rotations;

    void Start()
    {
        SetUp();
    }

    void OnPostRender()
    {
        material.SetPass(0);
        for(int i=0; i<count; i++)
        {
            Graphics.DrawMeshNow(mesh,positions[i],rotations[i]);
        }
    }

    private void SetUp()
    {
        rotations = ObjectTransforms.GenerateObjRot(count);
        positions = ObjectTransforms.GenerateObjPos(count,anchor.position,spacing);
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
