// system
using System;
using System.Collections;
using System.Collections.Generic;

// unity
using System.Linq;
using UnityEngine;


namespace Ex
{

    public class SceneFaderExVR : BaseCompiledCSharpComponent
    {

        public GameObject[] fadeScreens;
        //UnityEngine.Rendering.Volume globalVolume;
        public float fadeSpeed = 3.8f, waitingTimeBetweenScenes = 2f;
        public string[] scenesArray;
        public int currentScenesArrayID;
        public bool hasFaded, isFading;

        public List<Light> lights;
        Light shelterTransitionLight;
        public float[] lightsLastIntensity;
        Material tmpSkybox;
        float skyboxExposure, skyboxSunsize, skyboxConvergence, skyboxThickness;

        string lastRoutineName = "";

        OmniController omniController;
        GraphicsHandler graphicsHandler;

        public enum FadeDirection
        {
            In, //Alpha = 1
            Out // Alpha = 0
        }

        // ### Ex component flow functions, uncomment to use
        // # main functions
        
        /*public override bool initialize() {
            return true;
        }*/

        public override void start_experiment()
        {
            StartCoroutine(PrepareExperiment());
        }

        public override void start_routine()
        {
            StartCoroutine(PrepareNewRoutine());
        }

        private IEnumerator PrepareExperiment()
        {
            yield return new WaitForEndOfFrame();
            omniController = FindObjectOfType<OmniController>();
            omniController.InitExperiment();
            if (omniController != null) log_message(omniController.name + " found");

            graphicsHandler = FindObjectOfType<GraphicsHandler>();
            if (omniController != null) log_message(graphicsHandler.name + " found");
        }

        private IEnumerator PrepareNewRoutine()
        {
            yield return new WaitForEndOfFrame();
            
            omniController.PrepareSoundInNewRoutine(current_routine().name);
            graphicsHandler.SetGraphicsForRoutine(current_routine().name);

            if (current_routine().name == "LabScene" || current_routine().name == "WhiteScene" || current_routine().name == "ForestScene" || lastRoutineName == "")
            {
                StartCoroutine(PrepareNewSceneFadeIn());
            }
        }

        public override void update()
        {

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && !isFading)
            {
                if (current_routine().name == "LabScene" || current_routine().name == "WhiteScene")
                {
                    isFading = true;
                    //currentScenesArrayID += 1;
                    //if (currentScenesArrayID >= scenesArray.Length) currentScenesArrayID = 0;
                    StartCoroutine(FadeOutAndLoadScene());
                }
                else
                {
                    next();
                }
            }

            omniController.UpdateSound();
        }

        private IEnumerator PrepareNewSceneFadeIn()
        {
            //globalVolume = FindObjectOfType<UnityEngine.Rendering.Volume>();
            //globalVolume.weight = 0;
            
            yield return 0;
             
            fadeScreens = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => (obj.name == "ShelterScreen" && obj.activeInHierarchy)).ToArray();
            
            CopyCurrentSkybox();

            GetLightsReferences();
            ShutEveryLights();

            yield return 0;

            StartCoroutine(Fade(FadeDirection.In));
        }



        private IEnumerator Fade(FadeDirection fadeDirection)
        {
            float alpha = (fadeDirection == FadeDirection.Out) ? 0 : 1;
            float fadeEndValue = (fadeDirection == FadeDirection.Out) ? 1 : 0;

            if (fadeDirection == FadeDirection.In)
            {
                log_message("fading in");
                while (alpha >= fadeEndValue)
                {
                    ApplyFadeAmount(ref alpha, fadeDirection);
                    yield return null;
                }
                log_message("fade in done");
                //foreach (GameObject screen in fadeScreens) screen.SetActive(false);
            }
            else
            {
                log_message("fading out");
                //foreach (GameObject screen in fadeScreens) screen.SetActive(true);
                while (alpha <= fadeEndValue)
                {
                    ApplyFadeAmount(ref alpha, fadeDirection);
                    yield return null;
                }
                log_message("fade out done");
                lastRoutineName = current_routine().name;
                yield return new WaitForEndOfFrame();
                next();
            }

            // fade cycle is now complete
            if (fadeDirection == FadeDirection.In) isFading = false;
        }

        private void ApplyFadeAmount(ref float alpha, FadeDirection fadeDirection)
        {

            // meditation shelter
            foreach (GameObject screen in fadeScreens)
            {
                Material mat = screen.GetComponent<MeshRenderer>().materials[0];
                mat.SetColor("_Color", new Color(mat.color.r, mat.color.g, mat.color.b, alpha));
            }

            if(current_routine().name != "ForestScene" && current_routine().name != "Forest FOA") shelterTransitionLight.intensity = alpha;

            // lights
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].intensity = lightsLastIntensity[i] * (1 - alpha);
            }

            //globalVolume.weight = 1 - alpha;

            // skybox
            tmpSkybox.SetFloat("_SunSize", skyboxSunsize * (1 - alpha));
            tmpSkybox.SetFloat("_SunSizeConvergence", skyboxConvergence * (1 - alpha));
            tmpSkybox.SetFloat("_AtmosphereThickness", skyboxThickness * (1 - alpha));
            tmpSkybox.SetFloat("_Exposure", skyboxExposure * (1 - alpha));

            // sounds
            omniController.omniSoundsMainVolume = 1-alpha;

            alpha += Time.deltaTime * (1.0f / fadeSpeed) * ((fadeDirection == FadeDirection.In) ? -1 : 1);
        }


        public IEnumerator FadeOutAndLoadScene()
        {
            yield return Fade(FadeDirection.Out);

            
            //SceneManager.LoadScene(scenesArray[currentScenesArrayID]);

            // remove already present dontdestroyonload object (atm shelter)
            /*
            yield return 0;

            //if (FindObjectsOfType<DontDestroyOnLoad>().Length > 1) Destroy(FindObjectsOfType<DontDestroyOnLoad>()[1].gameObject);

            yield return 0; // wait for the frame to end and the objects to be destroyed

            PrepareNewSceneFadeIn();

            yield return new WaitForSeconds(waitingTimeBetweenScenes);
            yield return Fade(FadeDirection.In);*/
        }




        public void GetLightsReferences()
        {
            shelterTransitionLight = GameObject.Find("ShelterTransitionLight").GetComponent<Light>();

            Light[] lightArray = FindObjectsOfType<Light>();
            lights = new List<Light>();

            foreach (Light light in lightArray)
            {
                //if (light.tag != "MeditationShelter") lights.Add(light);
                if (light.gameObject.name != "ShelterTransitionLight") lights.Add(light);
            }

            lightsLastIntensity = new float[lights.Count];

            for (int i = 0; i < lights.Count; i++)
            {
                lightsLastIntensity[i] = lights[i].intensity;
            }
        }


        void ShutEveryLights()
        {
            foreach (Light light in lights)
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

        // public override void stop_routine() {}
        // public override void stop_experiment(){}
        // public override void play(){}
        // public override void pause(){}
        // public override void set_update_state(bool doUpdate) { }        
        // public override void set_visibility(bool visibility) { }

        // # for advanced users 
        // public override void clean(){}
        // public override void pre_update() {}
        // public override void post_update() {}
        // public override void update_parameter_from_gui(XML.Arg arg){}
        // public override void update_from_current_config(){}

        // # slots
        // public override void slot1(object value){}
        // public override void slot2(object value){}
        // public override void slot3(object value){}
        // public override void slot4(object value){}        
        // public override void slot5(IdAny idValue){
        // 	int id = idValue.id;
        // 	object value = idValue.value;
        // }  
    }
}


/**
namespace Ex{    

    public class BaseCompiledCSharpComponent : MonoBehaviour{

        // ExComponent class associated to the script (see ExComponent code snippet) 
        public ExComponent p = null;
        
        // init configuration for this component 
        public ComponentInitConfig init_config();
        // current configuration of the routine for this component
        public ComponentConfig current_config();
        // current routine associated to this component
        public Routine current_routine();
        // current condition of the associated routine
        public Condition current_condition();
        
        // states
        public bool is_initialized();
        public bool is_visible();
        public bool is_updating();
        public bool is_closed();

        // getters of others components
        public List<ExComponent> get_all(string category);
        public List<ExComponent> get_all(ExComponent.CType type);
        public ExComponent get(int key);
        public ExComponent get(string name);
        public List<T> get<T>() where T : ExComponent;
        public T get<T>(string name) where T : ExComponent;

        // special commands
        public void next();
        public void previous();
        public void close();
        public void stop();
        
        // timers
        public double ellapsed_time_exp_ms();
        public double ellapsed_time_routine_ms();
        
        // logging
        public void log_message(string message);
		public void log_warning(string warning);
        public void log_error(string error);
        
        // invoke signals
        public void invoke_signal1(object value);
        public void invoke_signal2(object value);
        public void invoke_signal3(object value);
        public void invoke_signal4(object value);
        
    }
		
	public class ExComponent : MonoBehaviour{

		// enums
		public enum CType { UI, Image, Video, Tracking, Model, Avatar, Audio, Script, Scene, Network, Output, Input, Text, Interaction, Feedback, Camera, Settings, Cloud, Undefined };
		public enum InitPriority { Low, Medium, Hight};

		// members
		// # misc
        public int key;
        public CType type;
        public InitPriority initPriority;

		// # data 
		public Routine currentRoutine;
		public Condition currentCondition;
        public TimeLine currentTimeline;      
        // ## configs
        public ComponentConfig currentC; // current config
        public ComponentInitConfig initC; // init config
        public List<ComponentConfig> configs;		

        // # events
        public Events.SignalsSlotsConnections events();

		// functions		
		// # misc
		public string component_name();
		public string type_str();               
        
		// # log
        static public void log_message(string message);
        static public void log_error(string error);
		
		// # time
        static public double ellapsed_time_exp_ms();
        static public double ellapsed_time_routine_ms();
		
		// # get
        static public List<ExComponent> get_all(string category);
        static public List<ExComponent> get_all(CType type);
        static public ExComponent get(int key);
        static public ExComponent get(string name);
        static public List<T> get<T>() where T : ExComponent;
        static public T get<T>(string name) where T : ExComponent;

		// # specials commands
        public static void next(); //  Send an event for moving to the next flow element        
        public static void previous(); // Send an event for moving to the previous flow element
        public static void stop();
        public static void next_element_with_name(string elementName);
        public static void previous_element_with_name(string elementName);
        public static void next_element_with_condition(string elementName);
        public static void previous_element_with_condition(string elementName);
        public static void modify_routine_action_config(string routineName, string conditionName, string componentName, string newConfigName);
		public void close(); // Disable the component, it will be not enabled again before the next routine
		
		// # configs
        public ComponentInitConfig init_config();
        public ComponentConfig current_config();
        public ComponentConfig get_config(string configName);

		// # states
        public bool is_started();
        public bool is_initialized();
        public bool is_visible();
        public bool is_updating();
        public bool is_closed();
    }
}
*/
