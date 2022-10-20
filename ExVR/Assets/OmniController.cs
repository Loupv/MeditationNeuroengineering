// system
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

// unity
using UnityEngine;
using UnityEngine.Audio;


namespace Ex
{
    public enum MusicType
    {
        Spatialized,
        Ambient
    }


    public class OmniController : BaseCompiledCSharpComponent{


		private AudioSourceComponent _0deg;
		private AudioSourceComponent _90deg;
		private AudioSourceComponent _180deg;
		private AudioSourceComponent _270deg;

		public Transform myCameraTransform;
		public float mainVolume = 1f;

        float backgroundMusicVolume;
        bool isInitialized, fading;
        MusicType musicType;
        //public float playDelay;

        private AudioSourceComponent _SpatializedBackgroundMusic, _AmbientBackgroundMusic;

        public void InitExperiment()
        {
            myCameraTransform = ExVR.Display().cameras().get_eye_camera_transform();

            _AmbientBackgroundMusic = get<AudioSourceComponent>("BackgroundMusic_Ambient");
            
            if(current_routine().name == "LabScene") // starting in labroom
            {
                _SpatializedBackgroundMusic = get<AudioSourceComponent>("BackgroundMusic_Spatialized");
                backgroundMusicVolume = _SpatializedBackgroundMusic.audioSource.volume;
                musicType = MusicType.Spatialized; 
            }
            else // starting elsewhere
            {
                backgroundMusicVolume = _AmbientBackgroundMusic.audioSource.volume;
                musicType = MusicType.Ambient; 
            }

            isInitialized = true;
        }

        // called by scene fader
        public void PrepareSoundInNewRoutine(string routineName)
		{
            //myCamera = get<CameraComponent>("FPPCamera");
            //var disp = ExVR.Display.cameras();
            //myCamera 


            //log_message(myCameraTransform.name);
            if(routineName == "LabScene"){
                _0deg = get<AudioSourceComponent>("LabRoom_Front");
                _90deg = get<AudioSourceComponent>("LabRoom_Right");
			    _180deg = get<AudioSourceComponent>("LabRoom_Back");
			    _270deg = get<AudioSourceComponent>("LabRoom_Left");
            }
            else if (routineName == "WhiteScene"){
                _0deg = get<AudioSourceComponent>("WhiteRoom_Front"); 
                _90deg = get<AudioSourceComponent>("WhiteRoom_Right");
                _180deg = get<AudioSourceComponent>("WhiteRoom_Back");
                _270deg = get<AudioSourceComponent>("WhiteRoom_Left");
            }
            else if (routineName == "ForestScene" || routineName == "Forest FOA")
            {
                _0deg = get<AudioSourceComponent>("AmbianceForest_Front"); 
                _90deg = get<AudioSourceComponent>("AmbianceForest_Right");
                _180deg = get<AudioSourceComponent>("AmbianceForest_Back");
                _270deg = get<AudioSourceComponent>("AmbianceForest_Left");
            }

            log_message("sounds set");

            mainVolume = 0;
            _0deg.set_volume(0);
            _90deg.set_volume(0);
            _180deg.set_volume(0);
            _270deg.set_volume(0);
        }

        // Update is called once per frame
        public void UpdateSound()
		{
            if (isInitialized)
            {
                SetOmniControllerOrientation();

                if (UnityEngine.Input.GetKeyDown(KeyCode.A) && !fading) { 

                    StartCoroutine(SwitchFromSpatializedToAmbientMusic(5f));
                }
            }

			//log_message(azimuth.ToString() +", "+ _0deg.audioSource.volume+", "+ _90deg.audioSource.volume+", "+ _180deg.audioSource.volume+", "+ _270deg.audioSource.volume);

			/*if (_0deg.time < playDelay)
			{
				Debug.Log("Looping to custom clip start");
				_0deg.time = playDelay;
				_90deg.time = playDelay;
				_180deg.time = playDelay;
				_270deg.time = playDelay;
			}*/

		}

		
        IEnumerator SwitchFromSpatializedToAmbientMusic(float fadeSpeed)
        {
            float sound_fader, end_value;
            
            // switch to spatialized music (3d)
            if (musicType == MusicType.Ambient)
            {
                sound_fader = 0; // 0 
                end_value = 1;

                log_message("Switching to spatialized");
                while (sound_fader < end_value)
                {
                    fading = true;
                    log_message(sound_fader.ToString());
                    _AmbientBackgroundMusic.set_volume((1 - sound_fader) *backgroundMusicVolume);
                    _SpatializedBackgroundMusic.set_volume((sound_fader) * backgroundMusicVolume);
                    sound_fader += Time.deltaTime * (1.0f / fadeSpeed);
                    yield return null;
                }
                musicType = MusicType.Spatialized;
                fading = false;
                log_message("Switch done");
            }
            // switch to ambient music (2d)
            else
            {
                sound_fader = 1;
                end_value = 0;

                log_message("Switching to ambient");
                while (sound_fader > end_value)
                {
                    fading = true;
                    log_message(sound_fader.ToString());
                    _AmbientBackgroundMusic.set_volume((1-sound_fader) * backgroundMusicVolume);
                    _SpatializedBackgroundMusic.set_volume((sound_fader) * backgroundMusicVolume);
                    sound_fader -= Time.deltaTime * (1.0f / fadeSpeed);
                    yield return null;
                }
                musicType = MusicType.Ambient;
                fading = false;
                log_message("Switch done");
            }

            yield return 0;
        }


        void SetOmniControllerOrientation()
        {
            float azimuth = myCameraTransform.rotation.eulerAngles.y;
            //float azimuthRad = azimuth * Mathf.Deg2Rad;

            //log_message(myCameraTransform.position.y.ToString() +" "+ azimuth.ToString());

            if (azimuth <= 90.0f)
            {
                _0deg.set_volume(Mathf.Abs(Remap(azimuth % 360, 0, 90, 1, 0)) * mainVolume);
                _90deg.set_volume(Mathf.Abs(Remap(azimuth % 360, 0, 90, 0, 1)) * mainVolume);
                _180deg.set_volume(0.0f);
                _270deg.set_volume(0.0f);
            }
            else if (azimuth > 90.0f && azimuth <= 180.0f)
            {
                _0deg.set_volume(0.0f);
                _90deg.set_volume(Mathf.Abs(Remap(azimuth % 360, 90, 180, 1, 0)) * mainVolume);
                _180deg.set_volume(Mathf.Abs(Remap(azimuth % 360, 90, 180, 0, 1)) * mainVolume);
                _270deg.set_volume(0.0f);
            }
            else if (azimuth > 180.0f && azimuth <= 270.0f)
            {
                _0deg.set_volume(0.0f);
                _90deg.set_volume(0.0f);
                _180deg.set_volume(Mathf.Abs(Remap(azimuth % 360, 180, 270, 1, 0)) * mainVolume);
                _270deg.set_volume(Mathf.Abs(Remap(azimuth % 360, 180, 270, 0, 1)) * mainVolume);
            }
            else if (azimuth > 270.0f && azimuth <= 360.0f)
            {
                _0deg.set_volume(Mathf.Abs(Remap(azimuth % 360, 270, 360, 0, 1)) * mainVolume);
                _90deg.set_volume(0.0f);
                _180deg.set_volume(0.0f);
                _270deg.set_volume(Mathf.Abs(Remap(azimuth % 360, 270, 360, 1, 0)) * mainVolume);
            }
        }


        /*public AudioSourceComponent PickRandomMusic()
        {

        }*/
        
        public float Remap(float value, float from1, float from2, float to1, float to2)
        {
            return (value - from1) / (from2 - from1) * (to2 - to1) + to1;
        }

    }


}

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
