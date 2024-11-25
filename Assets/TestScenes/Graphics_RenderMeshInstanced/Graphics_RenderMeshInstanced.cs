// https://docs.unity3d.com/ScriptReference/Graphics.RenderMeshInstanced.html
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Graphics_RenderMeshInstanced : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Mesh mesh;
    public Material material;
    
    struct MyInstanceData
    {
        public Matrix4x4 objectToWorld; //Mandatory
        // other unity instance data, see doc
    };

    private Vector3[] positions;
    private Quaternion[] rotations;
    private RenderParams rp;
    private MyInstanceData[] instData;
    private MaterialPropertyBlock mpb;

    void OnEnable()
    {
        SetUp();
        rotations = ObjectTransforms.GenerateObjRot(count);
        
        // RenderParams and custom instance color
        Vector4[] colors = new Vector4[count];
        rp = new RenderParams(material);
        for(int i=0; i<count; ++i)
        {
            float r = Random.Range(0.0f, 1.0f);
            float g = Random.Range(0.0f, 1.0f);
            float b = Random.Range(0.0f, 1.0f);
            colors[i] = new Color(r, g, b);
        }
        mpb = new MaterialPropertyBlock();
        mpb.SetVectorArray("_ColorList", colors);
        rp.matProps = mpb;
        
        // Unity instance data
        instData = new MyInstanceData[count];
        for(int i=0; i<count; ++i)
        {
            instData[i].objectToWorld = Matrix4x4.TRS( positions[i] , rotations[i] ,Vector3.one);
        }
    }

    void Update()
    {
        Graphics.RenderMeshInstanced(rp, mesh, 0, instData, count, 0);
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
