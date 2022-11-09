// system
using System;
using System.Collections;
using System.Collections.Generic;

// unity
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Ex
{

    public class SceneConfig{

        public string sceneName;
        public bool closeScreensBeforeFading, fadeSounds, keepShelterLightOn, activateFog;
        public Color fogColor;
        public float fogDensity;
        public float[] bloomValues;
        public Color bloomColor;
    }



    public class SceneFaderExVR : BaseCompiledCSharpComponent
    {

        public GameObject[] shelterScreens;
        //UnityEngine.Rendering.Volume globalVolume;
        public float curtainsClosingSpeed = 1f, fadeSpeed = 1f, waitingTimeBetweenScenes = 2f;
        public bool hasFaded, isFading, fadingDone;

        public List<Light> lights;
        Light shelterTransitionLight;
        public float[] lightsLastIntensity;
        Material tmpSkybox;
        float skyboxExposure, skyboxSunsize, skyboxConvergence, skyboxThickness;

        public string[] scenesArray;
        public int currentScenesArrayID;

        OmniController omniController;
        GraphicsHandler graphicsHandler;

        SceneConfig[] sceneConfigs;
        SceneConfig currentSceneConfig, lastSceneConfig;

        public SceneConfig[] InitSceneConfigArray()
        {
            sceneConfigs = new SceneConfig[4];
            sceneConfigs[0] = new SceneConfig { sceneName = "LabScene", closeScreensBeforeFading = true, fadeSounds = true, keepShelterLightOn = false, activateFog = true, fogColor = new Color(0.67f, 0.72f, 0.72f, 1f), fogDensity = 0.0045f, bloomValues = new float[4] { 1.96f, 0.78f, 0.5f, 10f }, bloomColor = new Color(1f, 0f, 0f, 1f) };
            sceneConfigs[1] = new SceneConfig { sceneName = "WhiteScene", closeScreensBeforeFading = true, fadeSounds = true, keepShelterLightOn = false, activateFog = true, fogColor = new Color(0.67f, 0.72f, 0.72f, 1f), fogDensity = 0.01f, bloomValues = new float[4] { 3f, 0.61f, 0.5f, 10f }, bloomColor = new Color(0.8f, 0.46f, 0f, 1f) };
            sceneConfigs[2] = new SceneConfig { sceneName = "ForestScene", closeScreensBeforeFading = true, fadeSounds = true, keepShelterLightOn = true, activateFog = true, fogColor = new Color(0.47f, 0.60f, 0.60f, 1f), fogDensity = 0.0045f, bloomValues = new float[4] { 1.13f, 2.19f, 0.5f, 10f }, bloomColor=new Color(0.98f, 0.43f, 0f, 1f) };
            sceneConfigs[3] = new SceneConfig { sceneName = "WhiteScene", closeScreensBeforeFading = true, fadeSounds = true, keepShelterLightOn = false, activateFog = true, fogColor = new Color(0.67f, 0.72f, 0.72f, 1f), fogDensity = 0.01f, bloomValues = new float[4] { 3f, 0.61f, 0.5f, 10f }, bloomColor = new Color(0.8f, 0.46f, 0f, 1f) };
            sceneConfigs[4] = new SceneConfig { sceneName = "LabScene", closeScreensBeforeFading = true, fadeSounds = true, keepShelterLightOn = false, activateFog = true, fogColor = new Color(0.67f, 0.72f, 0.72f, 1f), fogDensity = 0.0045f, bloomValues = new float[4] { 1.96f, 0.78f, 0.5f, 10f }, bloomColor = new Color(1f, 0f, 0f, 1f) };
            sceneConfigs[5] = new SceneConfig { sceneName = "LabScene", closeScreensBeforeFading = true, fadeSounds = true, keepShelterLightOn = false, activateFog = true, fogColor = new Color(0.67f, 0.72f, 0.72f, 1f), fogDensity = 0.0045f, bloomValues = new float[4] { 1.96f, 0.78f, 0.5f, 10f }, bloomColor = new Color(1f, 0f, 0f, 1f) };


            //sceneConfigs[3] = new SceneConfig { sceneName = "Forest FOA", closeScreensBeforeFading = false, fadeSounds = true, keepShelterLightOn = false, activateFog = true, fogColor = new Color(0.29f, 0.32f, 0.32f, 1f), fogDensity = 0.0045f, bloomValues = new float[4] { 2.06f, 0.5f, 0.5f, 10f }, bloomColor = new Color(1f, 0f, 0f, 1f) };
            return sceneConfigs;
        }

        public enum FadeDirection
        {
            In, //Alpha = 1
            Out // Alpha = 0
        }



        #region INIT

        public override void start_experiment()
        {
            log_message("Starting Experiment");
            StartCoroutine(PrepareExperiment());
        }


        private IEnumerator PrepareExperiment()
        {
            yield return new WaitForEndOfFrame();
            omniController = FindObjectOfType<OmniController>();
            omniController.InitExperiment();
            if (omniController != null) log_message(omniController.name + " found");

            graphicsHandler = FindObjectOfType<GraphicsHandler>();
            if (omniController != null) log_message(graphicsHandler.name + " found");

            sceneConfigs = InitSceneConfigArray();
            currentScenesArrayID = GetScenesArrayIDFromRoutineName(current_routine().name);
            
            currentSceneConfig = sceneConfigs[currentScenesArrayID];

        }

        int GetScenesArrayIDFromRoutineName(string routineName)
        {
            if (routineName.StartsWith("1_")) return 0;
            else if (routineName.StartsWith("2_")) return 1;
            else if (routineName.StartsWith("3_")) return 2;
            else if (routineName.StartsWith("4_")) return 3;
            else if (routineName.StartsWith("5_")) return 4;
            else if (routineName.StartsWith("6_")) return 5;
            else return 0;
            /*switch (routineName)
            {
                case "LabScene1PP":
                    return 0;
                    break;
                case "WhiteScene":
                    return 1;
                    break;
                case "ForestScene":
                    return 2;
                    break;
                case "Forest FOA":
                    return 3;
                    break;
                case "LabScene3PP":
                    return 0;
                    break;
            }
            return 0;*/
        }


        public override void start_routine()
        {
            StartCoroutine(PrepareNewRoutine());
        }
        

        private IEnumerator PrepareNewRoutine()
        {
            yield return 0;

            int i = 0;
            foreach (SceneConfig config in sceneConfigs)
            {
                if (current_routine().name.Contains(sceneConfigs[i].sceneName))
                {
                    currentScenesArrayID = i;// GetScenesArrayIDFromRoutineName(current_routine().name);
                    currentSceneConfig = sceneConfigs[currentScenesArrayID];
                    log_message("Starting Routine " + sceneConfigs[i].sceneName);
                    break;
                }
                i += 1;
            }

            omniController.PrepareSoundInNewRoutine(currentSceneConfig.sceneName);
            graphicsHandler.SetGraphicsForRoutine(currentSceneConfig);

            //yield return new WaitForEndOfFrame();

            /*if (shelterTransitionLight == null)
            {
                shelterTransitionLight = GameObject.Find("ShelterTransitionLight").GetComponent<Light>();
                shelterTransitionLight.gameObject.SetActive(false);
            }*/

            shelterScreens = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => (obj.name == "ShelterScreen" && obj.activeInHierarchy)).ToArray();
            
            if(lastSceneConfig == null)
            {
                foreach (GameObject screen in shelterScreens)
                {
                    Material mat = screen.GetComponent<MeshRenderer>().materials[0];
                    mat.SetColor("_Color", new Color(mat.color.r, mat.color.g, mat.color.b, 1));
                }
            }

            //CopyCurrentSkybox();
            GetLightsReferences();
            ShutEveryLights();

            var ppvp = ExVR.Display().postProcessingVolume.profile;
            var bl = ppvp.GetSetting<Bloom>();
            bl.active = true;
            bl.enabled.value = true;
            bl.intensity.value = currentSceneConfig.bloomValues[0];
            bl.threshold.value = currentSceneConfig.bloomValues[1];
            bl.softKnee.value = currentSceneConfig.bloomValues[2];
            bl.diffusion.value = currentSceneConfig.bloomValues[3];
            bl.color.value = currentSceneConfig.bloomColor;

            yield return 0;

            

            StartCoroutine(FadeIn());
            
        }


        public override void update()
        {

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && !isFading)
            {
                isFading = true;

                lastSceneConfig = currentSceneConfig;
                currentScenesArrayID += 1;// GetScenesArrayIDFromRoutineName(current_routine().name);
                currentSceneConfig = sceneConfigs[currentScenesArrayID];
                LogNewConfig(currentSceneConfig);

                StartCoroutine(FadeOut());

            }

            if (isFading && fadingDone)
            {
                fadingDone = false;
                log_message("Next routine");
                next();
            }

            if (omniController.isInitialized) omniController.UpdateSound();
        }
        #endregion




        #region FADES

    
        private IEnumerator FadeOut()
        {
            log_message("fading out");

            float alpha = 0;
            float fadeEndValue = 1;
            
            // first we check if we need to fade screens
            if (currentSceneConfig.closeScreensBeforeFading)
            {
                foreach (GameObject screen in shelterScreens) screen.GetComponent<MeshRenderer>().enabled = true;
                

                while (alpha <= fadeEndValue)
                {
                    foreach (GameObject screen in shelterScreens)
                    {
                        Material mat = screen.GetComponent<MeshRenderer>().materials[0];
                        mat.SetColor("_Color", new Color(mat.color.r, mat.color.g, mat.color.b, alpha));
                    }

                    /* int i;
                    foreach(Light light in lights)
                    {
                        lights[i].intensity = lightsLastIntensity[i] * alpha;
                        i++;
                    }*/

                    alpha += Time.deltaTime * (1.0f / curtainsClosingSpeed);
                    yield return null;
                }
            }


            // lights and sound
            alpha = 0;
            fadeEndValue = 1;

            while (alpha <= fadeEndValue)
            {
                FadeLightsAndSounds(alpha, FadeDirection.Out);
                alpha += Time.deltaTime * (1.0f / fadeSpeed);
                yield return null;
            }

            // post process and skybox
            StartCoroutine(LerpToNextRoutineValues(lastSceneConfig, currentSceneConfig));
        }



        private IEnumerator FadeIn()
        {
            log_message("fading in");

            float alpha = 1f;
            float fadeEndValue = 0f;

            while (alpha >= fadeEndValue)
            {
                FadeLightsAndSounds(alpha, FadeDirection.In);

                alpha -= Time.deltaTime * (1.0f / fadeSpeed);
                yield return null;
            }


            if (currentSceneConfig.closeScreensBeforeFading)
            {
                alpha = 1f;
                fadeEndValue = 0f;

                while (alpha >= fadeEndValue)
                {
                    foreach (GameObject screen in shelterScreens)
                    {
                        Material mat = screen.GetComponent<MeshRenderer>().materials[0];
                        mat.SetColor("_Color", new Color(mat.color.r, mat.color.g, mat.color.b, alpha));
                    }
                    alpha -= Time.deltaTime * (1.0f / curtainsClosingSpeed);
                    yield return null;
                }
                foreach (GameObject screen in shelterScreens) screen.GetComponent<MeshRenderer>().enabled = false;
            }

            // fade cycle is now complete
            log_message("fade in done");
            isFading = false;
        }



        private void FadeLightsAndSounds(float alpha, FadeDirection fadeDirection)
        {

            // if light is already on, don't light it
            //if(fadeDirection == FadeDirection.Out && !lastSceneConfig.keepShelterLightOn) shelterTransitionLight.intensity = alpha;

            // if current scene asks to keep shelter light on, don't do anything
            //if(fadeDirection == FadeDirection.In && !currentSceneConfig.keepShelterLightOn) shelterTransitionLight.intensity = alpha;

            // lights
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].intensity = lightsLastIntensity[i] * (1 - alpha);
            }

            //globalVolume.weight = 1 - alpha;

            // skybox
            //tmpSkybox.SetFloat("_SunSize", skyboxSunsize * (1 - alpha));
            //tmpSkybox.SetFloat("_SunSizeConvergence", skyboxConvergence * (1 - alpha));
            //tmpSkybox.SetFloat("_AtmosphereThickness", skyboxThickness * (1 - alpha));
            //tmpSkybox.SetFloat("_Exposure", skyboxExposure * (1 - alpha));

            // sounds
            omniController.omniSoundsMainVolume = 1-alpha;

        }


        private IEnumerator LerpToNextRoutineValues(SceneConfig lastSceneConfig, SceneConfig currentSceneConfig)
        {
            float amount = 0.0f;

            Light sun = GameObject.Find("Sun").GetComponent<Light>();

            //tmpSkybox = new Material(RenderSettings.skybox.shader);
            SkyComponent skyComponent = get<SkyComponent>("SkyBox");
            List<ComponentConfig> skyConfigs = skyComponent.configs;
            ComponentConfig currentSkyConfig = skyConfigs[0];
            foreach (ComponentConfig config in skyConfigs) if (config.name == currentSceneConfig.sceneName) currentSkyConfig = config;

            float skyboxSunsize1 =     RenderSettings.skybox.GetFloat("_SunSize");
            float skyboxConvergence1 = RenderSettings.skybox.GetFloat("_SunSizeConvergence");
            float skyboxThickness1 =   RenderSettings.skybox.GetFloat("_AtmosphereThickness");
            float skyboxExposure1 =    RenderSettings.skybox.GetFloat("_Exposure");
            Color skyTint1 =           RenderSettings.skybox.GetColor("_SkyTint");
            Color groundTint1 =        RenderSettings.skybox.GetColor("_GroundColor");

            float ambientIntensity1 =  RenderSettings.ambientIntensity; // skybox.GetFloat("_AmbientIntensity");
            float sunIntensity1 =      sun.intensity;

            float skyboxSunsize2 =     currentSkyConfig.get<float>("sun-size");
            float skyboxConvergence2 = currentSkyConfig.get<float>("convergence");
            float skyboxThickness2 =   currentSkyConfig.get<float>("atmosphere-thickness");
            float skyboxExposure2 =    currentSkyConfig.get<float>("procedural-exposure");
            Color skyTint2 =           currentSkyConfig.get_color("procedural-sky-tint");
            Color groundTint2 =        currentSkyConfig.get_color("procedural-ground-color");

            float ambientIntensity2 =  currentSkyConfig.get<float>("ambient_intensity");
            float sunIntensity2 =      currentSkyConfig.get<float>("sun_intensity");


            /*PostprocessComponent postProcessComponent = get<SkyComponent>("PostProcess");
            List<ComponentConfig> ppConfigs = postProcessComponent.configs;
            ComponentConfig currentPPConfig = ppConfigs[0];
            foreach (ComponentConfig config in ppConfigs) if (config.name == currentSceneConfig.sceneName) currentPPConfig = config;*/


            // TO DO Dir lights & postprocess ?
            // check in editor what's going on with the lights

            //log_message("ambient int going from " + ambientIntensity1.ToString() + " to " + ambientIntensity2.ToString());

            //log_message("ambient int now " + RenderSettings.ambientIntensity);


            while (amount <= 1)
            {
                RenderSettings.skybox.SetFloat("_SunSize", Mathf.Lerp(skyboxSunsize1, skyboxSunsize2, amount));
                RenderSettings.skybox.SetFloat("_SunSizeConvergence", Mathf.Lerp(skyboxConvergence1, skyboxConvergence2, amount));
                RenderSettings.skybox.SetFloat("_AtmosphereThickness", Mathf.Lerp(skyboxThickness1, skyboxThickness2, amount));
                RenderSettings.skybox.SetFloat("_Exposure", Mathf.Lerp(skyboxExposure1, skyboxExposure2, amount));
                RenderSettings.skybox.SetColor("_SkyTint", Color.Lerp(skyTint1, skyTint2, amount));
                RenderSettings.skybox.SetColor("_GroundColor", Color.Lerp(groundTint1, groundTint2, amount));

                sun.intensity = Mathf.Lerp(sunIntensity1, sunIntensity2, amount);
                //RenderSettings.skybox.SetFloat("_AmbientIntensity", Mathf.Lerp(ambientIntensity1, ambientIntensity2, amount));
                
                
                RenderSettings.ambientIntensity = Mathf.Lerp(ambientIntensity1, ambientIntensity2, amount);
                
                //RenderSettings.skybox = tmpSkybox;

                amount += Time.deltaTime * (1.0f / fadeSpeed);
                //log_message(amount.ToString());
                yield return null;
            }

            //log_message("ambient int now " + RenderSettings.ambientIntensity);

            yield return new WaitForEndOfFrame();
            
            PutEveryLightBackToItsOriginalIntensity();

            log_message("fade out done");
            fadingDone = true;
        }


        #endregion



        public void GetLightsReferences()
        {
            Light[] lightArray = FindObjectsOfType<Light>();
            lights = new List<Light>();

            foreach (Light light in lightArray)
            {
                //if (light.tag != "MeditationShelter") lights.Add(light);
                if (light.gameObject.name != "ShelterTransitionLight" && light.type != LightType.Directional) lights.Add(light);
            }

            log_message(lights.Count.ToString() + " lights found");

            lightsLastIntensity = new float[lights.Count];
            for (int i = 0; i < lights.Count; i++)
            {
                lightsLastIntensity[i] = lights[i].intensity;
            }
        }

        void PutEveryLightBackToItsOriginalIntensity()
        {
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].intensity = lightsLastIntensity[i];
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
        }


        void LogNewConfig(SceneConfig currentSceneConfig)
        {
            log_message("New config : " + currentSceneConfig.sceneName);
            log_message("Prefs : " + currentSceneConfig.closeScreensBeforeFading.ToString()+" "+ currentSceneConfig.fadeSounds.ToString() + " "+ currentSceneConfig.keepShelterLightOn.ToString());
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
