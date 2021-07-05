using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class Graphics_DrawProceduralIndirect : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Material material;
    public ShadowCastingMode castShadows;
    public bool receiveShadows = true;
    public LayerMask layer = 0;

    private Camera cam;
    private Vector4[] positions;
    private Bounds bound;
    private MaterialPropertyBlock properties;
    private ComputeBuffer argsBuffer;

    void Start()
    {
        SetUp();
        cam = Camera.main;
        bound = new Bounds(this.transform.position, Vector3.one*100f);
        properties = new MaterialPropertyBlock();

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)3; // vertex count per instance
        args[1] = (uint)count;
        args[2] = (uint)0; // start vertex location
        args[3] = (uint)0; // start instance location

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void Update()
    {
        for(int i=0; i<count; i++)
        {
            properties.SetVector("position",positions[i]);
            Graphics.DrawProceduralIndirect(material, bound, MeshTopology.Triangles, argsBuffer, 0, cam, properties, castShadows, receiveShadows, layer);
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
