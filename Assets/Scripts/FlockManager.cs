using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{

    //public static FlockManager FM;
    public GameObject flockPrefab1, flockPrefab2;
    public int flockSize = 20;
    public GameObject[] allObjects;
    public Vector3 movementLimits = new Vector3(5.0f, 5.0f, 5.0f);
    public Vector3 goalPos = Vector3.zero;

    [Header("Flock Settings")]
    [Range(0.0f, 1.0f)] public float minSpeed;
    [Range(0.0f, 5.0f)] public float maxSpeed;
    [Range(1.0f, 10.0f)] public float neighbourDistance;
    [Range(1.0f, 15.0f)] public float rotationSpeed;
    
    //[Range(0.7f, 1.3f)] float instanceScale;

    void Start()
    {

        allObjects = new GameObject[flockSize];

        for (int i = 0; i < flockSize; ++i)
        {

            Vector3 pos = this.transform.position + new Vector3(
                Random.Range(-movementLimits.x, movementLimits.x),
                Random.Range(-movementLimits.y, movementLimits.y),
                Random.Range(-movementLimits.z, movementLimits.z));

            if (Random.value >= 0.5f) allObjects[i] = Instantiate(flockPrefab1, pos, Quaternion.identity);
            else allObjects[i] = Instantiate(flockPrefab2, pos, Quaternion.identity);
            allObjects[i].GetComponent<Flock>().Init(this);
            allObjects[i].transform.parent = this.transform;

            allObjects[i].transform.localScale *= Random.Range(0.7f,1.3f);

            //allObjects[i].transform.Find("Butterfly").GetComponent<MeshRenderer>().materials[0] = mat1;
            //else allObjects[i].transform.Find("Butterfly").GetComponent<MeshRenderer>().materials[0] = mat2;
        }

        goalPos = this.transform.position;
    }


    void Update()
    {

        if (Random.Range(0, 100) < 10)
        {

            goalPos = this.transform.position + new Vector3(
                Random.Range(-movementLimits.x, movementLimits.x),
                Random.Range(-movementLimits.y, movementLimits.y),
                Random.Range(-movementLimits.z, movementLimits.z));
        }
    }
}