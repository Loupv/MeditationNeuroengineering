// system
using System;
using System.Reflection;
using System.Collections.Generic;

// unity
using UnityEngine;

namespace Ex{

    public class CameraController : BaseCompiledCSharpComponent{

        // ### Ex component flow functions, uncomment to use
        // # main functions
        // public override bool initialize() {return true;}
        // public override void start_experiment() {}
        // public override void start_routine() {}
        // public override void update() {}
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

        public float turnSpeed = 0.5f;      // Speed of camera turning when mouse moves in along an axis
        public float panSpeed = 0.5f;       // Speed of the camera when being panned
        public float zoomSpeed = 0.3f;      // Speed of the camera going back and forth
        public float translateSpeed = 0.002f;
        private Vector3 mouseOrigin;    // Position of cursor when mouse dragging starts
        private bool isPanning;     // Is the camera being panned?
        private bool isRotating;    // Is the camera being rotated?
        private bool isZooming;     // Is the camera zooming?

        GameObject cameraRig;

        public override void start_experiment()
        {
            //camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            cameraRig = GameObject.Find("[CameraRig]");
            //cameraOrigin = cameraRig.transform.Find("Cameras").position;

            //Camera.main.transform.position = camera.transform.position;
            //Camera.main.transform.rotation = camera.transform.rotation;
            //camera.transform.parent = transform;


        }

        public override void update()
        {

            if (UnityEngine.Input.GetKey("up"))
            {
                cameraRig.transform.Find("Cameras").transform.Translate(new Vector3(0, 0, 1) * translateSpeed);
            }

            if (UnityEngine.Input.GetKey("down"))
            {
                cameraRig.transform.Find("Cameras").transform.Translate(new Vector3(0, 0, -1) * translateSpeed);
            }

            if (UnityEngine.Input.GetKey("left"))
            {
                cameraRig.transform.Find("Cameras").transform.Translate(new Vector3(-1, 0, 0) * translateSpeed);
            }

            if (UnityEngine.Input.GetKey("right"))
            {
                cameraRig.transform.Find("Cameras").transform.Translate(new Vector3(1, 0, 0) * translateSpeed);
            }


            // Get the left mouse button
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                // Get mouse origin
                mouseOrigin = UnityEngine.Input.mousePosition;
                isRotating = true;
            }

            // Get the right mouse button
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                // Get mouse origin
                mouseOrigin = UnityEngine.Input.mousePosition;
                isPanning = true;
            }

            // Get the middle mouse button
            if (UnityEngine.Input.GetMouseButtonDown(2))
            {
                // Get mouse origin
                mouseOrigin = UnityEngine.Input.mousePosition;
                isZooming = true;
            }

            // Disable movements on button release
            if (!UnityEngine.Input.GetMouseButton(0)) isRotating = false;
            if (!UnityEngine.Input.GetMouseButton(1)) isPanning = false;
            if (!UnityEngine.Input.GetMouseButton(2)) isZooming = false;

            // Rotate camera along X and Y axis
            if (isRotating)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(UnityEngine.Input.mousePosition - mouseOrigin);

                cameraRig.transform.Find("Cameras").transform.RotateAround(cameraRig.transform.Find("Cameras").transform.position, cameraRig.transform.Find("Cameras").transform.right, -pos.y * turnSpeed);
                cameraRig.transform.Find("Cameras").transform.RotateAround(cameraRig.transform.Find("Cameras").transform.position, Vector3.up, pos.x * turnSpeed);
            }

            // Move the camera on it's XY plane
            if (isPanning)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(UnityEngine.Input.mousePosition - mouseOrigin);

                //Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
                Vector3 move = new Vector3(0, pos.y * panSpeed, 0);
                cameraRig.transform.Find("Cameras").transform.Translate(move, Space.Self);
            }

            // Move the camera linearly along Z axis
            if (isZooming)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(UnityEngine.Input.mousePosition - mouseOrigin);

                Vector3 move = pos.y * zoomSpeed * cameraRig.transform.Find("Cameras").transform.forward;
                cameraRig.transform.Find("Cameras").transform.Translate(move, Space.World);
            }
        }

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
