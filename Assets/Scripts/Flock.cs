using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour {

    public float speed, lastSpeed, speedTarget, lerpTime, totalLerpTime = 2;
    bool turning = false;
    public bool lerping;
    FlockManager manager;
    MaterialPropertyBlock materialProperty;
    Renderer rend;
    //public float min = 0.3f, max = 1.0f;// randomFloat;

    public void Init(FlockManager m) {
        manager = m;
        speed = Random.Range(manager.minSpeed, manager.maxSpeed);
        rend = transform.GetChild(0).GetComponent<MeshRenderer>();
    }


    void Update() {

        Bounds b = new Bounds(manager.transform.position, manager.movementLimits * 2.0f);

        if (!b.Contains(transform.position)) {

            turning = true;
        } 
        else {

            turning = false;
        }



        if (turning) {

            Vector3 direction = manager.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                manager.rotationSpeed * Time.deltaTime);
        } 
        else {


            if (Random.Range(0, 100) < 10 && !lerping) {
                
                lastSpeed = speed;
                speedTarget = Random.Range(manager.minSpeed, manager.maxSpeed);

                materialProperty = new MaterialPropertyBlock();
                materialProperty.Clear();
                rend.SetPropertyBlock(materialProperty);
                lerpTime = 0;
                lerping = true;
            }

            else if (lerping)
            {
                //materialProperty.SetFloat("_Rand", Mathf.Lerp(lastSpeed, speedTarget, lerpTime));
                speed = Mathf.Lerp(lastSpeed, speedTarget, lerpTime);
                materialProperty.SetFloat("_Speed", speed);
                lerpTime += Time.deltaTime;
                if(lerpTime > totalLerpTime) { lerping = false; }
            }


            if (Random.Range(0, 100) < 10) {
                ApplyRules();
            }
        }

        this.transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);
    }

    private void ApplyRules() {

        GameObject[] gos;
        gos = manager.allObjects;

        Vector3 vCentre = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;

        float gSpeed = 0.01f;
        float mDistance;
        int groupSize = 0;

        foreach (GameObject go in gos) {

            if (go != this.gameObject) {

                mDistance = Vector3.Distance(go.transform.position, this.transform.position);
                if (mDistance <= manager.neighbourDistance) {

                    vCentre += go.transform.position;
                    groupSize++;

                    if (mDistance < 1.0f) {

                        vAvoid = vAvoid + (this.transform.position - go.transform.position);
                    }

                    Flock anotherFlock = go.GetComponent<Flock>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }
            }
        }

        if (groupSize > 0) {

            vCentre = vCentre / groupSize + (manager.goalPos - this.transform.position);
            //speed = gSpeed / groupSize;

            /*if (speed > manager.maxSpeed) {

                speed = manager.maxSpeed;
            }*/

            Vector3 direction = (vCentre + vAvoid) - transform.position;
            if (direction != Vector3.zero) {

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    manager.rotationSpeed * Time.deltaTime);
            }
        }
    }
}