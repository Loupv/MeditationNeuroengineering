using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayAnimation : MonoBehaviour
{

    float delay, speed;

    // Start is called before the first frame update
    void Start()
    {
        delay = Random.Range(0, 100);
        speed = Random.Range(0.7f, 1);
        GetComponent<Animator>().speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Animator>().speed = speed + Mathf.Sin(Time.time/5)*0.3f;
    }
}
