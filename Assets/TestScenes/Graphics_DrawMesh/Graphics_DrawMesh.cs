using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Graphics_DrawMesh : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Mesh mesh;
    public Material material;

    private Camera cam;
    private Vector3[] positions;
    private Quaternion[] rotations;

    void Start()
    {
        SetUp();
    }

    void Update()
    {
        for(int i=0; i<count; i++)
        {
            Graphics.DrawMesh(mesh,positions[i],rotations[i],material,0,cam,0);
        }
    }

    private void SetUp()
    {
        cam = Camera.main;
        rotations = ObjectTransforms.GenerateObjRot(count);
        positions = ObjectTransforms.GenerateObjPos(count,transform.position,spacing);
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
