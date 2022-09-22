using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchSoundAnim : MonoBehaviour
{

    public float nonSpatializedSoundVolume, spatializedSoundVolume;
    public GameObject sphere1, sphere2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            sphere1.GetComponent<Animation>().Play();
            sphere1.GetComponentInChildren<AudioSource>().volume = nonSpatializedSoundVolume;
            sphere1.GetComponentInChildren<AudioSource>().spatialize = false;
            sphere1.GetComponentInChildren<AudioSource>().Play();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            sphere1.GetComponent<Animation>().Play();
            sphere1.GetComponentInChildren<AudioSource>().volume = spatializedSoundVolume;
            sphere1.GetComponentInChildren<AudioSource>().spatialize = true;
            sphere1.GetComponentInChildren<AudioSource>().Play();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            sphere2.GetComponent<Animation>().Play();
            sphere2.GetComponentInChildren<AudioSource>().volume = nonSpatializedSoundVolume;
            sphere2.GetComponentInChildren<AudioSource>().spatialize = false;
            sphere2.GetComponentInChildren<AudioSource>().Play();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            sphere2.GetComponent<Animation>().Play();
            sphere2.GetComponentInChildren<AudioSource>().volume = spatializedSoundVolume;
            sphere2.GetComponentInChildren<AudioSource>().spatialize = true;
            sphere2.GetComponentInChildren<AudioSource>().Play();
        }

    }
}
