using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ButterflyBehaviour : MonoBehaviour
{
    MaterialPropertyBlock materialProperty;
    float targettedHeight = 0;
    Renderer rend;
    Rigidbody rg;
    public float speed = 0.5f, minSpeed = 0.1f, maxSpeed = 8, flapStrength = 4, rgMultiplier, limitDistanceBeforeNewTarget = 1;

    public Vector3 movementLimits = new Vector3(5.0f, 5.0f, 5.0f), strengthMultiplier;

    bool hasFlapped;
    Vector3 targetPos;
    float timeCount = 0;

    // Start is called before the first frame update
    void Init()
    {
        materialProperty = new MaterialPropertyBlock();
        rend = GetComponent<MeshRenderer>();
        //speed = materialProperty.GetFloat("_Speed");
        rg = GetComponent<Rigidbody>();

        //InvokeRepeating("AdjustWingsSpeed", 0, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        // sin(2x) - 1/4 * sin(4x)
        float x = timeCount * 2 * Mathf.PI * speed;
        float bounce = Mathf.Sin(2*x) - 1/4 * Mathf.Sin(4*x);
        //Debug.Log(bounce);

        if (bounce < 0 && !hasFlapped)
        {
            hasFlapped = true;
            Invoke("WingFlap", 0);
            Debug.Log("flap");
        }
        else if (hasFlapped && bounce > 0) hasFlapped = false;


        if (Vector3.Distance(transform.position, targetPos) < limitDistanceBeforeNewTarget)
        {
            Debug.Log("new target");
            targetPos = transform.position + new Vector3(
                UnityEngine.Random.Range(-movementLimits.x, movementLimits.x),
                UnityEngine.Random.Range(-movementLimits.y, movementLimits.y),
                UnityEngine.Random.Range(-movementLimits.z, movementLimits.z));
        }

        timeCount += Time.deltaTime;
    }


    void WingFlap()
    {
        float distanceFromOptimalHeight = Vector3.Distance(transform.position, targetPos);
        rgMultiplier = distanceFromOptimalHeight - targettedHeight;
        Vector3 addedForce = (targetPos - transform.position) * flapStrength * rgMultiplier;

        // remove eventual negative verticality in the vector
        addedForce = new Vector3(addedForce.x, Mathf.Max(addedForce.y, 0), addedForce.z);

        addedForce = Vector3.Scale(addedForce, strengthMultiplier);
        rg.AddForce(addedForce);

        //transform.rotation.SetLookRotation(rg.velocity);
        Vector3 vec = Quaternion.Euler(0, 180, 0) * rg.velocity;
        transform.rotation = Quaternion.LookRotation(vec);
        
    }


    void WingFlap1D()
    {
        float distanceFromOptimalHeight = Mathf.Abs(Mathf.Min(transform.position.y - targettedHeight, 0));
        rgMultiplier = distanceFromOptimalHeight - targettedHeight;
        rg.AddForce(new Vector3(0, flapStrength * rgMultiplier, 0));
    }

    void AdjustWingsSpeed()
    {
        if (transform.position.y > targetPos.y)
            speed = Mathf.Lerp(speed, minSpeed, 0.1f);
        else speed = Mathf.Lerp(speed, maxSpeed, 0.1f);
    }

    private void OnValidate()
    {
        Init();
        Debug.Log("value changed");
        MaterialPropertyBlock materialProperty = new MaterialPropertyBlock();
        materialProperty.Clear();
        materialProperty.SetFloat("_Speed", speed);
        rend.SetPropertyBlock(materialProperty);

    }
}
