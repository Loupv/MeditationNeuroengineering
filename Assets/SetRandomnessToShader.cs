using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomnessToShader : MonoBehaviour
{
    Renderer rend;
    public float min = 0.3f, max = 1.0f, number;

    // Update is called once per frame
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        number = Random.Range(min, max)*10;
        //mat.SetFloat("_Rand", number);
    }

    private void Update()
    {
        //renderer.SetPropertyBlock("_Rand", number);


        MaterialPropertyBlock materialProperty = new MaterialPropertyBlock();

        materialProperty.Clear();
        materialProperty.SetFloat("_Rand", number);
        rend.SetPropertyBlock(materialProperty);

        /*Graphics.DrawMesh(aMesh, new Vector3(5, 0, 0),
            Quaternion.identity, aMaterial, 0, null, 0, materialProperty);

        // Clear any property and add a green color
        materialProperty.Clear();
        materialProperty.SetColor("_Color", Color.green);
        Graphics.DrawMesh(aMesh, new Vector3(-5, 0, 0),
            Quaternion.identity, aMaterial, 0, null, 0, materialProperty);
        */
    }

}
