// system
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// unity
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
//using UnityEngine.Rendering.Universal;

namespace Ex{

    public class UIQuestionModule : BaseCompiledCSharpComponent{

        // ### Ex component flow functions, uncomment to use
        // # main functions
        // public override bool initialize() {return true;}
        // public override void start_experiment() {}

        bool active;
        GameObject myCamera;
        PlaneComponent cursor;
        RaycastHit HitInfo;
        bool active_selection;
        GameObject radialFiller;
        AssetBundleComponent questionModule;
        List<GameObject> answers;
        Image fillerImage;

        string currentQuestion, modulename;
        IEnumerable<string> currentAnswers;
        string[] lines, answerStrings;
        int currentQuestionID, answersCount;
        bool questionLoaded;
  


        public override void slot1(object value)
        {
            string str = (string)value;
            //currentQuestionID = 0;
            lines = str.Split2("\n").ToArray();
            
            var values = lines[currentQuestionID].Split2(";");// ToString();
            
            currentQuestion = values.First();
            answerStrings = values.Skip(1).ToArray();
            //StartCoroutine("Init");

        }

        public override void slot2(object value)
        {
            Init();
        }

        public void Init()
        {
            log_message("Initing question module");
            //currentQuestionID = 0;

            cursor = get<PlaneComponent>("Cursor");

            if (current_config().name == "1answer")
            {
                questionModule = get<AssetBundleComponent>("QuestionModule_1answer");
                answers = new List<GameObject>();
                modulename = "questionmodule_1answer";
                answersCount = 1;
            }
            else if (current_config().name == "4answers")
            {
                questionModule = get<AssetBundleComponent>("QuestionModule_4answers");
                answers = new List<GameObject>();
                modulename = "questionmodule_4answers";
                answersCount = 4;
            }
            else if(current_config().name == "7answers")
            {
                questionModule = get<AssetBundleComponent>("QuestionModule_7answers");
                answers = new List<GameObject>();
                modulename = "questionmodule_7answers";
                answersCount = 7;
            }
            else if (current_config().name == "pbb")
            {
                questionModule = get<AssetBundleComponent>("QuestionModule_PBB");
                answers = new List<GameObject>();
                modulename = "questionmodule_pbb";
                answersCount = 7;
            }
            else if (current_config().name == "fbi")
            {
                questionModule = get<AssetBundleComponent>("QuestionModule_7answers");
                answers = new List<GameObject>();
                modulename = "questionmodule_7answers";
                answersCount = 7;
            }

            for (int i = 0; i < answersCount; i++)
            {
                answers.Add(questionModule.transform.Find(modulename+"/Canvas/Answer" + (i + 1)).gameObject);
                log_message(i.ToString() + " " + answers[i].name);
            }

            radialFiller = questionModule.transform.Find(modulename + "/Canvas/RadialFiller").gameObject; //GameObject.Find("RadialFiller");
            radialFiller.SetActive(true);
            fillerImage = GameObject.Find("Filler").GetComponent<Image>();
            radialFiller.SetActive(false);
            
            myCamera = ExVR.Display().cameras().get_eye_camera_transform().gameObject;// GameObject.Find("[CameraRig]").transform.Find("Cameras").gameObject;
            active = true;

            LoadNextQuestion();
        } 


        public override void update() {

            
            if (!active && currentQuestionID == 0) // last check to avoid to launch init if we're currently switching to next routine
            {
                //Init();
            }

            else
            {


                bool hasHit = Physics.Raycast(myCamera.transform.position, myCamera.transform.forward, out HitInfo, 100.0f);

                string hitName = "";
                if (hasHit) hitName = HitInfo.transform.gameObject.name;

                
                // if we look at the target for the first time
                if (hasHit && hitName.Contains("Answer") && !active_selection)
                {
                    active_selection = true;
                    InvokeRepeating("FillRadialUI", 0, Time.deltaTime);
                    log_message(HitInfo.point.ToString() + ", " + HitInfo.transform.gameObject);

                }
                // if no hit or hitting something else
                else if(!hasHit || !hitName.Contains("Answer"))
                { 
                    fillerImage.fillAmount = 0;
                    radialFiller.SetActive(false);
                    CancelInvoke("FillRadialUI");
                    active_selection = false;
                    cursor.transform.position = HitInfo.point;// + new Vector3(0.1f, 0.1f, 0);
                }
            }
        }

        void FillRadialUI()
        {
            if(!radialFiller.activeSelf) radialFiller.SetActive(true);
            radialFiller.gameObject.transform.position = HitInfo.point;
            cursor.transform.position = new Vector3(0, -10, 0);
            fillerImage.fillAmount += Time.deltaTime;

            if (fillerImage.fillAmount >= 1 && active)
            {
                fillerImage.fillAmount = 0;
                currentQuestionID += 1;
                radialFiller.SetActive(false);
                CancelInvoke("FillRadialUI");

                SendLogSignal();

                if (currentQuestionID < lines.Count())
                {
                    log_message("Loading next question.");
                    LoadNextQuestion();
                }
                else //if(current_routine().name.Contains("Questionnaire"))
                {
                    log_message("End of questions. Next.");
                    currentQuestionID = 0;
                    active = false;
                    active_selection = false;
                    next();
                }
            }
        }


        void SendLogSignal()
        {
            TimeManager timeManager = FindObjectOfType<TimeManager>();
            double time = timeManager.ellapsed_exp_ms();

            string str = time.ToString() + ";" + currentQuestion + ";" + HitInfo.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text;
            invoke_signal1(str);

        }


        /*public override void stop_routine() {
            active = false;
        }*/

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
        
       


        void LoadNextQuestion()
        {            
            var values = lines[currentQuestionID].Split2(";");// ToString();

            currentQuestion = values.First();
            currentQuestion = currentQuestion.Replace("%", "\n");
            log_message(currentQuestion);
            answerStrings = values.Skip(1).ToArray();

            questionModule.transform.Find(modulename+"/Canvas/Question/Text").GetComponent<UnityEngine.UI.Text>().text = currentQuestion;

            for (int i = 0; i < answers.Count; i++)
            {
                if (i < answerStrings.Length) answers[i].transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = answerStrings[i];
                else answers[i].SetActive(false);
            }
            questionLoaded = true;
        }

        /*public void invoke_signal1(object value)
        {

        }*/

        // public override void slot2(object value){}
        // public override void slot3(object value){}
        // public override void slot4(object value){}        
        // public override void slot5(IdAny idValue){
        // 	int id = idValue.id;
        // 	object value = idValue.value;
        // }  
    }

    public static class StringExtensions
    {

        public static IEnumerable<string> Split2(this string source, string delim)
        {
            int oldIndex = 0, newIndex;
            while((newIndex = source.IndexOf(delim, oldIndex)) != -1)
            {
                yield return source.Substring(oldIndex, newIndex-oldIndex);
                oldIndex = newIndex+ delim.Length;
            }
            yield return source.Substring(oldIndex);
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
