using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour {

    //public static FlockManager FM;
    public GameObject flockPrefab;
    public int flockSize = 20;
    public GameObject[] allObjects;
    public Vector3 movementLimits = new Vector3(5.0f, 5.0f, 5.0f);
    public Vector3 goalPos = Vector3.zero;
    
    public Material mat1, mat2;

    [Header("Flock Settings")]
    [Range(0.0f, 5.0f)] public float minSpeed;
    [Range(0.0f, 5.0f)] public float maxSpeed;
    [Range(1.0f, 10.0f)] public float neighbourDistance;
    [Range(1.0f, 5.0f)] public float rotationSpeed;

    void Start() {

        allObjects = new GameObject[flockSize];

        for (int i = 0; i < flockSize; ++i) {

            Vector3 pos = this.transform.position + new Vector3(
                Random.Range(-movementLimits.x, movementLimits.x),
                Random.Range(-movementLimits.y, movementLimits.y),
                Random.Range(-movementLimits.z, movementLimits.z));

            allObjects[i] = Instantiate(flockPrefab, pos, Quaternion.identity);
            allObjects[i].GetComponent<Flock>().Init(this);
            allObjects[i].transform.parent = this.transform;

            if (Random.value >= 0.5f) allObjects[i].transform.Find("Butterfly").GetComponent<MeshRenderer>().materials[0] = mat2;
            else allObjects[i].transform.Find("Butterfly").GetComponent<MeshRenderer>().materials[0] = mat1;
        }

        goalPos = this.transform.position;
    }


    void Update() {

        if (Random.Range(0, 100) < 10) {

            goalPos = this.transform.position + new Vector3(
                Random.Range(-movementLimits.x, movementLimits.x),
                Random.Range(-movementLimits.y, movementLimits.y),
                Random.Range(-movementLimits.z, movementLimits.z));
        }
    }
}