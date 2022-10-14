using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SceneFader : MonoBehaviour
{

	public GameObject[] fadeScreens;
	//UnityEngine.Rendering.Volume globalVolume;
	public float fadeSpeed = 0.8f, waitingTimeBetweenScenes = 2f;
	public string[] scenesArray;
	public int currentScenesArrayID;
	public bool hasFaded, isFading;

	public List<Light> lights;
	Light shelterTransitionLight;
	public float[] lightsLastIntensity;
	Material tmpSkybox;
	float skyboxExposure, skyboxSunsize, skyboxConvergence, skyboxThickness; 

	public enum FadeDirection
	{
		In, //Alpha = 1
		Out // Alpha = 0
	}


	void Start()
	{
		//GetLightsIntensity();
		PrepareNewSceneFadeIn();
		StartCoroutine(Fade(FadeDirection.In));
	}


    private void Update()
    {
		
		if (Input.GetKeyDown(KeyCode.Space) && !isFading)
        {
			isFading = true;
			currentScenesArrayID += 1;
			if (currentScenesArrayID >= scenesArray.Length) currentScenesArrayID = 0;
			StartCoroutine(FadeOutAndLoadScene());
        }
	}

    public IEnumerator FadeOutAndLoadScene()
	{
		yield return Fade(FadeDirection.Out);
		SceneManager.LoadScene(scenesArray[currentScenesArrayID]);

		// remove already present dontdestroyonload object (atm shelter)

		yield return 0;

		//if (FindObjectsOfType<DontDestroyOnLoad>().Length > 1) Destroy(FindObjectsOfType<DontDestroyOnLoad>()[1].gameObject);

		yield return 0; // wait for the frame to end and the objects to be destroyed

		PrepareNewSceneFadeIn();

		yield return new WaitForSeconds(waitingTimeBetweenScenes);
		yield return Fade(FadeDirection.In);
	}


	void PrepareNewSceneFadeIn()
    {
		//globalVolume = FindObjectOfType<UnityEngine.Rendering.Volume>();
		//globalVolume.weight = 0;

		//fadeScreens = GameObject.FindGameObjectsWithTag("ShelterScreens");
		fadeScreens = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => (obj.name == "ShelterScreen" && obj.activeInHierarchy)).ToArray();

		/*try
        {
			FindObjectOfType<OmniController>().myCamera = Camera.main.gameObject;
        }
        catch
        {
			Debug.Log("No omni system or no main camera found");
        }*/

		CopyCurrentSkybox();

		GetLightsReferences();
		ShutEveryLights();
	}

	private IEnumerator Fade(FadeDirection fadeDirection)
	{
		float alpha = (fadeDirection == FadeDirection.Out) ? 0 : 1;
		float fadeEndValue = (fadeDirection == FadeDirection.Out) ? 1 : 0;

		if (fadeDirection == FadeDirection.In)
		{
			Debug.Log("fading in");
			while (alpha >= fadeEndValue)
			{
				SetGOFadeAmount(ref alpha, fadeDirection);
				yield return null;
			}
			foreach (GameObject screen in fadeScreens) screen.SetActive(false);
		}
		else
		{
			Debug.Log("fading out");
			foreach (GameObject screen in fadeScreens) screen.SetActive(true);
			while (alpha <= fadeEndValue)
			{
				SetGOFadeAmount(ref alpha, fadeDirection);
				yield return null;
			}
		}

		// fade cycle is now complete
		if (fadeDirection == FadeDirection.In) isFading = false;
	}

	private void SetGOFadeAmount(ref float alpha, FadeDirection fadeDirection)
	{
		foreach(GameObject screen in fadeScreens)
        {
			Material mat = screen.GetComponent<MeshRenderer>().materials[0];
			mat.SetColor("_Color", new Color(mat.color.r, mat.color.g, mat.color.b, alpha));
        }

		shelterTransitionLight.intensity = alpha;
		
		for (int i = 0; i<lights.Count; i++)
        {
			lights[i].intensity = lightsLastIntensity[i] * (1-alpha);
        }

		//globalVolume.weight = 1 - alpha;

		tmpSkybox.SetFloat("_SunSize", skyboxSunsize*(1-alpha));
		tmpSkybox.SetFloat("_SunSizeConvergence", skyboxConvergence * (1 - alpha));
		tmpSkybox.SetFloat("_AtmosphereThickness", skyboxThickness * (1 - alpha));
		tmpSkybox.SetFloat("_Exposure", skyboxExposure*(1-alpha));

		alpha += Time.deltaTime * (1.0f / fadeSpeed) * ((fadeDirection == FadeDirection.In) ? -1 : 1);
	}



	public void GetLightsReferences()
    {

		shelterTransitionLight = GameObject.Find("ShelterTransitionLight").GetComponent<Light>();

		Light[] lightArray = FindObjectsOfType<Light>();
		lights = new List<Light>();

		foreach(Light light in lightArray)
        {
			if (light.tag != "MeditationShelter") lights.Add(light);
        }

		lightsLastIntensity = new float[lights.Count];
		
		for (int i = 0; i < lights.Count; i++)
		{
			lightsLastIntensity[i] = lights[i].intensity;
		}
	}


	void ShutEveryLights()
    {
		foreach(Light light in lights)
        {
			light.intensity = 0;
        }
    }


	void CopyCurrentSkybox()
	{
		// copy new skyboxparameters
		skyboxSunsize = RenderSettings.skybox.GetFloat("_SunSize");
		skyboxConvergence = RenderSettings.skybox.GetFloat("_SunSizeConvergence");
		skyboxThickness = RenderSettings.skybox.GetFloat("_AtmosphereThickness");
		skyboxExposure = RenderSettings.skybox.GetFloat("_Exposure");

		//create a new one
		tmpSkybox = new Material(RenderSettings.skybox.shader);

		// set everything to 0
		tmpSkybox.SetFloat("_Exposure", 0);
		tmpSkybox.SetFloat("_SunSize", 0);
		tmpSkybox.SetFloat("_SunSizeConvergence", 0);
		tmpSkybox.SetFloat("_AtmosphereThickness", 0);

		RenderSettings.skybox = tmpSkybox;
		
		//skyboxExposure = tmpSkybox.GetFloat("_Exposure");
		//skyboxSunsize = tmpSkybox.GetFloat("_SunSize");
		//skyboxConvergence = tmpSkybox.GetFloat("_SunSizeConvergence");
		//skyboxThickness = tmpSkybox.GetFloat("_AtmosphereThickness");
		//tmpSkybox.SetFloat("_SunSize", RenderSettings.skybox.GetFloat("_SunSize"));
		//tmpSkybox.SetFloat("_SunSizeConvergence", RenderSettings.skybox.GetFloat("_SunSizeConvergence"));
		//tmpSkybox.SetFloat("_AtmosphereThickness", RenderSettings.skybox.GetFloat("_AtmosphereThickness"));
		//tmpSkybox.SetFloat("_Exposure", RenderSettings.skybox.GetFloat("_Exposure"));

	}


}