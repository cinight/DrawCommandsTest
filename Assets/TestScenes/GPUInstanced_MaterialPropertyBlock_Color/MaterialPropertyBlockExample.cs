//This example comes from documentation - https://docs.unity3d.com/Manual/gpu-instancing-shader.html#changing-per-instanced-data-at-runtime
using UnityEngine;

public class MaterialPropertyBlockExample : MonoBehaviour
{
    public GameObject[] objects;

    void Start()
    {
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        MeshRenderer renderer;

        foreach (GameObject obj in objects)
        {
            float r = Random.Range(0.0f, 1.0f);
            float g = Random.Range(0.0f, 1.0f);
            float b = Random.Range(0.0f, 1.0f);
            Color col = new Color(r, g, b);
            props.SetColor("_Color", col);
            props.SetColor("_BaseColor", col);

            renderer = obj.GetComponent<MeshRenderer>();
            renderer.SetPropertyBlock(props);
        }
    }
}