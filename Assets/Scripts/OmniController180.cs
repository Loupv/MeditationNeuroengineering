using UnityEngine;
using System.Collections;

public class OmniController180 : MonoBehaviour {

	private AudioSource _0deg;
	private AudioSource _180deg;
	
	public AudioClip clipIn0;
	public AudioClip clipIn180;
	public GameObject myCamera;
	public float vol = 100.0f;


	// Use this for initialization
	void Start () {
		vol = vol/100.0f;

		_0deg = new AudioSource ();
		_180deg = new AudioSource ();
		
		_0deg = gameObject.AddComponent<AudioSource>() as AudioSource;
		_180deg = gameObject.AddComponent<AudioSource>() as AudioSource;
		
		_0deg.clip = clipIn0;
		_180deg.clip = clipIn180;
		
		_0deg.volume = 0.0f;
		_180deg.volume = 0.0f;
		
		_0deg.spatialBlend = 0.0f;
		_180deg.spatialBlend = 0.0f;
		
		_0deg.loop = true;
		_180deg.loop = true;
		
		_0deg.Play ();
		_180deg.Play ();
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		float azimuth = myCamera.transform.eulerAngles.y;
		float azimuthRad = azimuth * Mathf.Deg2Rad;

		/*_0deg.volume = Mathf.Abs(Mathf.Cos(azimuthRad/2)) * vol;
		_180deg.volume = Mathf.Abs(Mathf.Cos(azimuthRad/2 + Mathf.PI /2)) * vol;*/

		_0deg.volume = Mathf.Abs( Remap(azimuth % 360, 0, 360, -1, 1));
		_180deg.volume = Mathf.Abs( Remap((azimuth + 180) % 360, 0, 360, -1, 1) );


		Debug.Log(_0deg.volume + ", "+_180deg.volume);

	}


	public float Remap(float value, float from1, float from2, float to1, float to2)
	{
		return (value - from1) / (from2 - from1) * (to2 - to1) + to1;
	}
}
