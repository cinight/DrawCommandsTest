using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Graphics_DrawProcedural : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Mesh mesh;
    public Material material;
    public ShadowCastingMode castShadows;
    public bool receiveShadows = true;
    public LayerMask layer = 0;

    private Camera cam;
    private Vector4[] positions;
    private Bounds bound;
    private MaterialPropertyBlock properties;

    void Start()
    {
        SetUp();
        cam = Camera.main;
        bound = new Bounds(this.transform.position, Vector3.one*100f);
        properties = new MaterialPropertyBlock();
    }

    void Update()
    {
        for(int i=0; i<count; i++)
        {
            properties.SetVector("position",positions[i]);
            Graphics.DrawProcedural(material, bound, MeshTopology.Triangles, 3, count, cam, properties, castShadows, receiveShadows, layer);
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
