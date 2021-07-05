using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class Graphics_DrawProceduralIndirectNow : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;
    public Transform offset;

    [Header("Content")]
    public Material material;

    private Camera cam;
    private Vector4[] positions;
    private Bounds bound;
    private ComputeBuffer argsBuffer;

    void Start()
    {
        SetUp();
        cam = Camera.main;
        bound = new Bounds(this.transform.position, Vector3.one*100f);

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)3; // vertex count per instance
        args[1] = (uint)count;
        args[2] = (uint)0; // start vertex location
        args[3] = (uint)0; // start instance location

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void OnPostRender()
    {
        for(int i=0; i<count; i++)
        {
            material.SetPass(0);
            material.SetVector("position",positions[i]);
            Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, argsBuffer, 0);
        }
    }

    private void SetUp()
    {
        positions = ObjectTransforms.GenerateObjPosV4(count,offset.position,spacing);
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

    void OnDisable()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        if (argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null;
        }
    }
}
