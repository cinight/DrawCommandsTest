using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Graphics_DrawMeshInstancedProcedural : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Mesh mesh;
    public Material material;

    private Vector3[] positions;
    private ComputeBuffer positionBuffer;
    private Bounds bound;

    void Start()
    {
        SetUp();

        positionBuffer = new ComputeBuffer(count, 12);
        positionBuffer.SetData(positions);
        material.SetBuffer("positionBuffer", positionBuffer);

        bound = new Bounds(this.transform.position, Vector3.one*100f);
    }

    void Update()
    {
        Graphics.DrawMeshInstancedProcedural(mesh,0,material,bound,count);
    }

    private void CleanUp()
    {
        if (positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
    }

    void OnDisable()
    {
        CleanUp();
    }

    private void SetUp()
    {
        positions = ObjectTransforms.GenerateObjPos(count,transform.position,spacing);
    }

    void OnGUI()
    {
        EditmodeUpdate.Update();
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
