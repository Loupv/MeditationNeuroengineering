// system
using System;
using System.Collections;
using System.Collections.Generic;

// unity
using System.Linq;
using UnityEngine;


namespace Ex
{

    public class GraphicsHandler : BaseCompiledCSharpComponent
    {

        
        bool fogActivated;

        GameObject cameraRig;
        Vector3 newCameraTarget, cameraOrigin;

        float lerpTimeInSeconds;

        float targetDistance, targetHeight, targetAngle, lookForward, smoothHandle;
        float lastTargetDistance = 0, lastTargetHeight = 0, lastTargetAngle = 0, lastLookForward = 0.5f;

        bool cameraMoving;
        float lerpStartTime;

        public void InitExperiment()
        {
            cameraRig = GameObject.Find("[CameraRig]");
            cameraOrigin = cameraRig.transform.Find("Cameras").position; // ExVR.Display().cameras().transform.position;

            // be sure that object cameras is turned looking at origin, for orbital camera purposes later
            cameraRig.transform.Find("Cameras").LookAt(cameraOrigin - new Vector3(0, 0, lookForward));
        }


        public void SetGraphicsForRoutine(SceneConfig sceneConfig)
        {

            ActivateFog(sceneConfig);
            //RenderSettings.ambientIntensity = 0.55f; // au lieu de 1.55
            
            // uncheck realtime global illumination
            // shelter transition light à 1 ou équivalent
            //yield return 0; // new WaitForEndOfFrame();

        }


        public void ActivateFog(SceneConfig sceneConfig)
        {
            if (sceneConfig.activateFog)
            {
                log_message("Activating fog");
                RenderSettings.fog = true;
                RenderSettings.fogColor = sceneConfig.fogColor;
                RenderSettings.fogDensity = sceneConfig.fogDensity;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                fogActivated = true;
            }
            else
            {
                log_message("Deactivating fog");
                RenderSettings.fog = false;
                fogActivated = false;
            }
        }
        
        public override void update()
        {

            //if (UnityEngine.Input.GetKeyDown(KeyCode.F)) ActivateFog(!fogActivated);

            if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad0) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha0)) 
                InitCameraOrbit(0);                                                                   
            if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad1) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha1)) 
                InitCameraOrbit(1);                                                                   
            if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad2) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha2)) 
                InitCameraOrbit(2);                                                                   
            if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad3) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
                InitCameraOrbit(3);                                                                   
            if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad4) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
                InitCameraOrbit(4);

        }


        public void InitCameraOrbit(int i)
        {
            if (!cameraMoving)
            {

                switch (i)
                {
                    case 0: // initial position
                        targetDistance = 0;
                        targetHeight = 0;
                        targetAngle = 0;
                        lerpTimeInSeconds = 5;
                        break;
                    case 1: // close behind
                        targetDistance = 1.7f;
                        targetHeight = 0.7f;
                        targetAngle = 0;
                        lerpTimeInSeconds = 5;
                        break;
                    case 2: // far behind
                        targetDistance = 2;
                        targetHeight = 1.5f;
                        targetAngle = 0;// Mathf.PI * 2;
                        lerpTimeInSeconds = 15;
                        break;
                    case 3: // above
                        targetDistance = 0.3f;
                        targetHeight = 2;
                        targetAngle = 0;// Mathf.PI * 4;
                        lerpTimeInSeconds = 15;
                        break;
                    case 4: // turning around
                        targetDistance = 2f;
                        targetHeight = 0.5f;
                        targetAngle = Mathf.PI * 2;
                        lerpTimeInSeconds = 45;
                        break;
                }
                lookForward = 0.5f;
                smoothHandle = 2;
                
                if (!cameraMoving)
                {
                    cameraMoving = true;
                    lerpStartTime = Time.time;
                    //StartCoroutine("CameraOrbit");
                    StartCoroutine("BodyExit");
                }
            }
        }

        IEnumerator CameraOrbit()
        {
            float t = 0;
            Vector3 newPosition;

            log_message("Camera lerp started");

            while (t <= 1)
            {
                t = (Time.time - lerpStartTime) / lerpTimeInSeconds;

                float theta = Mathf.Lerp(lastTargetAngle, targetAngle, LerpSmoother(t, smoothHandle,1)) + Mathf.PI / 2;
                float ray = Mathf.Lerp(lastTargetDistance, targetDistance, LerpSmoother(t, smoothHandle,1));
                float h = Mathf.Lerp(lastTargetHeight, targetHeight, LerpSmoother(t, smoothHandle,1));
                float l = Mathf.Lerp(lastLookForward, lookForward, LerpSmoother(t, smoothHandle,1));

                newPosition = new Vector3(ray * Mathf.Cos(theta), h, ray * Mathf.Sin(theta)) + cameraOrigin;
                log_message(ray.ToString() + ", " + h.ToString() + ", " + l.ToString());

                cameraRig.transform.Find("Cameras").position = newPosition;
                cameraRig.transform.Find("Cameras").LookAt(cameraOrigin - new Vector3(0,0,l));

                yield return new WaitForEndOfFrame();
            }

            newPosition = new Vector3(targetDistance * Mathf.Cos(targetAngle + Mathf.PI / 2), targetHeight, targetDistance * Mathf.Sin(targetAngle + Mathf.PI / 2)) + cameraOrigin;
            cameraRig.transform.Find("Cameras").position = newPosition;
            cameraRig.transform.Find("Cameras").LookAt(cameraOrigin - new Vector3(0, 0, lookForward));


            lastTargetDistance = targetDistance;
            lastTargetHeight = targetHeight;
            lastTargetAngle = targetAngle % (Mathf.PI * 2);
            lastLookForward = lookForward;

            log_message("Camera lerp stopped");

            cameraMoving = false;
            invoke_signal1(true);
        }


        // need to do an invert lerp for body entry
        IEnumerator BodyExit()
        {
            float t = 0;
            Vector3 newPosition;

            log_message("Camera lerp started");

            while (t < 1)
            {
                t = (Time.time - lerpStartTime) / lerpTimeInSeconds;

                
                float ylerper = LerpSmoother(t, 2.2f, 0.5f);
                float zlerper = LerpSmoother(t, 2.2f, 1);
                

                //float x = xorigin;
                float y = Mathf.Lerp(lastTargetHeight, targetHeight, ylerper);
                float z = Mathf.Lerp(lastTargetDistance, targetDistance, zlerper);

                cameraRig.transform.Find("Cameras").position = new Vector3(0,y,z) + cameraOrigin;
                //cameraRig.transform.Find("Cameras").LookAt(cameraOrigin - new Vector3(0, 0, l));
                log_message(t.ToString());
                yield return new WaitForEndOfFrame();
            }

            lastTargetDistance = targetDistance;
            lastTargetHeight = targetHeight;

            newPosition = new Vector3(0, targetHeight, targetDistance) + cameraOrigin;
            cameraRig.transform.Find("Cameras").position = newPosition;
            //cameraRig.transform.Find("Cameras").LookAt(cameraOrigin - new Vector3(0, 0, lookForward));

            log_message("Camera lerp stopped");

            cameraMoving = false;
            invoke_signal1(true);
        }

        // x is the time, s is the slope, v is the speed
        float LerpSmoother(float t, float s, float v)
        {
            if(t < v) return (1 / (1 + Mathf.Pow(t / (v - t), -s)));
            else return 1;
            
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
