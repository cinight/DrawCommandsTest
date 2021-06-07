using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Graphics_DrawMeshInstanced : MonoBehaviour
{
    [Header("Layout")]
    public int count = 10;
    public float spacing = 1f;

    [Header("Content")]
    public Mesh mesh;
    public Material material;

    private Vector3[] positions;
    private Quaternion[] rotations;
    private Matrix4x4[] matrix;

    void Start()
    {
        SetUp();

        rotations = ObjectTransforms.GenerateObjRot(count);
        matrix = new Matrix4x4[count];
        for(int i=0; i<count; i++)
        {
            matrix[i] = Matrix4x4.TRS( positions[i] , rotations[i] ,Vector3.one);
        }
    }

    void Update()
    {
        Graphics.DrawMeshInstanced(mesh,0,material,matrix,count);
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
