// system
using System;
using System.Reflection;
using System.Collections.Generic;

// unity
using UnityEngine;

namespace Ex{

    public enum Task
    {
        Sound, Colors, Depth, Handedness
    }

    public class FamiliarizationModule : BaseCompiledCSharpComponent{

        // ### Ex component flow functions, uncomment to use
        // # main functions
        // public override bool initialize() {return true;}
        // public override void start_experiment() {}

        Task currentTask;

        AudioSourceComponent note1, note2, note3;
        
        AssetBundleComponent questionModule;

        public bool allowNextRoutine;

        GameObject sphere1, sphere2, sphere3;
        int currentNoteID;

        Vector3 sphereShift = new Vector3(0.6f, 1f, -1f);

        public override void start_routine() {

            allowNextRoutine = false;    

            if (current_routine().name == "Lab_Familiarization_Sound")
            {
                currentTask = Task.Sound;
                currentNoteID = 0;
            }
            else if (current_routine().name == "Lab_Familiarization_Colors")
            {
                currentTask = Task.Colors;
                CreateSpheresPrimitives();
            }

            questionModule = get<AssetBundleComponent>("FamiliarizationInstruction");
        }

        public override void update() {

            if (currentTask == Task.Sound)
            {
                if (UnityEngine.Input.GetKeyDown("space"))
                {
                    if (currentNoteID >= 3)
                    {
                        //next();
                        allowNextRoutine = true;
                    }
                    else PlayNextNote();
                }
            }
            else if (currentTask == Task.Colors)
            {
                allowNextRoutine = true;
                
                if (UnityEngine.Input.GetKeyDown("space"))
                {
                    sphere1.SetActive(false);
                    sphere2.SetActive(false);
                    sphere3.SetActive(false);
                    DestroySpheres();
                    //currentTask = Task.Handedness;
                    //next();
                }
            }
            /*else if (currentTask == Task.Depth) 
            {
                if (UnityEngine.Input.GetKeyDown("space"))
                {
                    DestroySpheres();
                    currentTask = Task.Handedness;
                }
            }*/
            else if (currentTask == Task.Handedness) 
            {
                
            }
        }

        public void PlayNextNote()
        {
            AudioSourceComponent currentNote;
            if(currentNoteID == 0) currentNote = get<AudioSourceComponent>("FamiliarizationNote1");
            else if (currentNoteID == 1) currentNote = get<AudioSourceComponent>("FamiliarizationNote2");
            //else if (currentNoteID == 2) currentNote = get<AudioSourceComponent>("FamiliarizationNote3");
            else currentNote = get<AudioSourceComponent>("FamiliarizationNote3");
            log_message("play a note");
            currentNote.start_sound();
            currentNoteID += 1;

        }

        public void StartColorsTask()
        {

        }

        public void StartDepthTask()
        {

        }

        public void StartHandednessTask()
        {

        }


        public override void stop_routine() {

            DestroySpheres();
        }


        void CreateSpheresPrimitives()
        {
            sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere1.transform.position = new Vector3(0.5f,0,0) + sphereShift;
            sphere2.transform.position = new Vector3(0f,0,0) + sphereShift;
            sphere3.transform.position = new Vector3(-0.5f,0,0) + sphereShift;
            sphere1.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            sphere2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            sphere3.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            var renderer1 = sphere1.GetComponent<Renderer>();
            renderer1.material = new Material(Shader.Find("Standard"));
            renderer1.material.SetColor("_Color", Color.red);

            var renderer2 = sphere2.GetComponent<Renderer>();
            renderer2.material = new Material(Shader.Find("Standard"));
            renderer2.material.SetColor("_Color", Color.green);

            var renderer3 = sphere3.GetComponent<Renderer>();
            renderer3.material = new Material(Shader.Find("Standard"));
            renderer3.material.SetColor("_Color", Color.blue);
        }

        void DestroySpheres()
        {
            GameObject.Destroy(sphere1);
            GameObject.Destroy(sphere2);
            GameObject.Destroy(sphere3);
        }


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
