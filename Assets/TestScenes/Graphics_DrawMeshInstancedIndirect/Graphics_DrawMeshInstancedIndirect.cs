using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Graphics_DrawMeshInstancedIndirect : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Mesh mesh;
    public Material material;

    private Vector3[] positions;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer positionBuffer;
    private Bounds bound;

    void Start()
    {
        SetUp();

        positionBuffer = new ComputeBuffer(count, 12);
        positionBuffer.SetData(positions);
        material.SetBuffer("positionBuffer", positionBuffer);

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)count;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        bound = new Bounds(this.transform.position, Vector3.one*100f);
    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(mesh,0,material,bound,argsBuffer,0);
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
