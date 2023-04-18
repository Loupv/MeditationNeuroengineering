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
    public float speed = 0.5f, minSpeed = 0.1f, maxSpeed = 8, flapStrength = 4, rgMultiplier, limitDistanceBeforeNewTarget = 1, limitAllowedDistanceForNewTarget = 2;

    public Vector3 movementLimits = new Vector3(5.0f, 5.0f, 5.0f), strengthMultiplier;
    public float maxVelocityMagnitude = 1f;
    bool hasFlapped, inited;
    public Vector3 targetPos, initialPos;
    float timeCount = 0;

    // Start is called before the first frame update
    public void Init(Vector3 p)
    {
        materialProperty = new MaterialPropertyBlock();
        rend = GetComponent<MeshRenderer>();
        //speed = materialProperty.GetFloat("_Speed");
        rg = GetComponent<Rigidbody>();
        inited = true;
        initialPos = p;

        targetPos = initialPos + new Vector3(
                UnityEngine.Random.Range(-movementLimits.x, movementLimits.x),
                UnityEngine.Random.Range(-movementLimits.y, movementLimits.y),
                UnityEngine.Random.Range(-movementLimits.z, movementLimits.z));
        //InvokeRepeating("AdjustWingsSpeed", 0, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {

        //if (!inited) Init();
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
            targetPos = initialPos + new Vector3(
                UnityEngine.Random.Range(-movementLimits.x, movementLimits.x),
                UnityEngine.Random.Range(-movementLimits.y, movementLimits.y),
                UnityEngine.Random.Range(-movementLimits.z, movementLimits.z));

            if (Vector3.Distance(transform.position, targetPos) > limitAllowedDistanceForNewTarget) targetPos = transform.position + (targetPos - transform.position).normalized * limitAllowedDistanceForNewTarget;
        }

        timeCount += Time.deltaTime;

        AdjustWingsSpeed();
    }


    void WingFlap()
    {
        float distanceFromTarget = Vector3.Distance(transform.position, targetPos);
        rgMultiplier = distanceFromTarget;
        Vector3 addedForce = (targetPos - transform.position) * flapStrength * Mathf.Min(rgMultiplier, maxVelocityMagnitude);

        // remove eventual negative verticality in the vector
        addedForce = new Vector3(addedForce.x, Mathf.Max(addedForce.y, 0), addedForce.z);

        addedForce = Vector3.Scale(addedForce, strengthMultiplier);
        
        rg.AddForce(addedForce);

        //if(rg.velocity.magnitude > maxVelocityMagnitude) rg.velocity = rg.velocity.normalized * maxVelocityMagnitude;

        //transform.rotation.SetLookRotation(rg.velocity);
        
        //Quaternion q = Quaternion.LookRotation(vec);
        //Vector3 vec2 = q.eulerAngles;
        //vec2 = new Vector3(Mathf.Abs(vec2.x), vec2.y, vec2.z);

        Vector3 vec = Quaternion.Euler(0, 180, 0) * rg.velocity;
        transform.rotation = Quaternion.LookRotation(vec); // Quaternion.Euler(vec2);
    }


    /*void WingFlap1D()
    {
        float distanceFromOptimalHeight = Mathf.Abs(Mathf.Min(transform.position.y - targettedHeight, 0));
        rgMultiplier = distanceFromOptimalHeight - targettedHeight;

        rg.AddForce(new Vector3(0, flapStrength * rgMultiplier,0));
    }*/

    void AdjustWingsSpeed()
    {
        if (transform.localPosition.y > targetPos.y)
            speed = minSpeed;
        else speed = maxSpeed;

        materialProperty = new MaterialPropertyBlock();
        materialProperty.Clear();
        materialProperty.SetFloat("_Speed", speed);
        rend.SetPropertyBlock(materialProperty);
    }

    /*private void OnValidate()
    {
        Init(transform.position);
        Debug.Log("value changed");
        
        materialProperty = new MaterialPropertyBlock();
        materialProperty.Clear();
        materialProperty.SetFloat("_Speed", speed);
        rend.SetPropertyBlock(materialProperty);

    }*/
}
