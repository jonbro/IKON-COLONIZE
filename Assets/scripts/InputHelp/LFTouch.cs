using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

// treats the mouse as a two finger touch thing, helpful for simulating touching in the editor

public class LFInput : MonoBehaviour{
	// called when the button configuration is done
	public delegate void OnConfigCompleteDelegate();
	public OnConfigCompleteDelegate ConfigComplete;
	public List<LFTouch> _tempTouch = new List<LFTouch>();
	bool[] buttonsDown;
	List<LFButtonInputMap> buttonMapping = new List<LFButtonInputMap>();
	List<LFAxisInputMap> axisMapping = new List<LFAxisInputMap>();
	public InputMap inputMap;
	private static LFInput instance;
	string configPath;
	bool configuring;
	string configId;
	int configType;
	LFTouch[] touchBuffer;
	public static LFInput Instance {
		get
		{
			if(instance == null){
				GameObject _instance = (GameObject)new GameObject("PoolManager");
				instance = _instance.AddComponent<LFInput>();
			}
			return instance;
		}
	}

	void Awake(){
		DontDestroyOnLoad(gameObject);
		buttonsDown = new bool[3];
		touchBuffer = new LFTouch[20];
		// set up the default controller configs
		#if !UNITY_WEBPLAYER
			configPath = Application.persistentDataPath + "/InputConfig.xml";
			if(File.Exists(configPath)){
				inputMap = InputMap.Load(configPath);
			}else{
		#endif
			inputMap = new InputMap();
			inputMap.SetDefaults();
		#if !UNITY_WEBPLAYER
			inputMap.Save(configPath);
		}
		#endif
	}
	void Start(){
	}
	void SetupDefaultMapping(){
	}
	public static void StartConfigButton(OnConfigCompleteDelegate _configComplete, string _configId){
		Instance.ConfigComplete = _configComplete;
		Instance.configuring = true;
		Instance.configType = 0;
		Instance.configId = _configId;
		// find the id in the list of things that we can config to confirm that it is configurable
		for(int i=0;i<Instance.inputMap.buttonMapping.Count;i++){
			if(Instance.inputMap.buttonMapping[i].id == Instance.configId){
				return;
			}
		}
		Instance.configuring = false;
	}
	public static LFTouch[] touches{
		set{
			// can't do it!
		}
		get{
			return LFInput.Instance.GetTouches();	
		}
	}
	public LFTouch[] GetTouches(){
		return _tempTouch.ToArray();
	}
	void OnGUI(){
		#if UNITY_STANDALONE
		// because we want to be able to capture and process arbitrary keyboard events
		// for some reason it throws KeyCode.None a bunch in between key presses
		if(configuring){
	        if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode != KeyCode.None){
	            // find the id
				for(int i=0;i<inputMap.buttonMapping.Count;i++){
					if(inputMap.buttonMapping[i].id == configId){
						// remove the current mapping and replace with the incoming one
						inputMap.buttonMapping.Remove(inputMap.buttonMapping[i]);
						break;
					}
				}
				inputMap.buttonMapping.Add(new LFButtonInputMap(configId, LFButtonInputMap.buttonType.KEYBOARD, Event.current.keyCode, 0,0));
				configuring = false;
				if(ConfigComplete != null) ConfigComplete();
	        }			
		}
		#endif
	}
	public static bool GetButtonDown(string buttonName){
		return LFInput.Instance._GetButtonDown(buttonName);
	}
	bool _GetButtonDown(string buttonName){
		// look at the dictionary mapping, and check the correct button
		bool rVal = false;
		for(int i=0;i<inputMap.buttonMapping.Count;i++){
			if(inputMap.buttonMapping[i].id == buttonName){
				if(inputMap.buttonMapping[i].bType == LFButtonInputMap.buttonType.JOYSTICK){
					rVal = rVal || Input.GetKeyDown(inputMap.buttonMapping[i].joystickString);
				}else if(inputMap.buttonMapping[i].bType == LFButtonInputMap.buttonType.JOYSTICK_AXIS){
					rVal = rVal || inputMap.buttonMapping[i].down;
				}else{
					rVal = rVal || Input.GetKeyDown(inputMap.buttonMapping[i].key);
				}
			}

		}
		return rVal;
	}
	public static bool GetButton(string buttonName){
		return LFInput.Instance._GetButton(buttonName);
	}
	bool _GetButton(string buttonName){
		// look at the dictionary mapping, and check the correct button
		bool rVal = false;
		for(int i=0;i<inputMap.buttonMapping.Count;i++){
			if(inputMap.buttonMapping[i].id == buttonName){
				if(inputMap.buttonMapping[i].bType == LFButtonInputMap.buttonType.JOYSTICK){
					rVal = rVal || Input.GetKey(inputMap.buttonMapping[i].joystickString);
				}else if(inputMap.buttonMapping[i].bType == LFButtonInputMap.buttonType.JOYSTICK_AXIS){
					rVal = rVal || inputMap.buttonMapping[i].down;
				}else{
					rVal = rVal || Input.GetKey(inputMap.buttonMapping[i].key);
				}
			}

		}
		return rVal;
	}
	public static bool GetAxisNegDown(string axisName){
		return LFInput.Instance._GetAxisNegDown(axisName);
	}
	public static bool GetAxisPosDown(string axisName){
		return LFInput.Instance._GetAxisPosDown(axisName);
	}
	bool _GetAxisNegDown(string axisName){
		bool rVal = false;
		for(int i=0;i<inputMap.axisMapping.Count;i++){
			if(inputMap.axisMapping[i].id == axisName){
				rVal = inputMap.axisMapping[i].GetDownNeg()||rVal;
			}
		}
		return rVal;
	}
	bool _GetAxisPosDown(string axisName){
		bool rVal = false;
		for(int i=0;i<inputMap.axisMapping.Count;i++){
			if(inputMap.axisMapping[i].id == axisName){
				rVal = inputMap.axisMapping[i].GetDownPos()||rVal;
			}
		}
		return rVal;
	}

	public static float GetAxis(string buttonName){
		return LFInput.Instance._GetAxis(buttonName);
	}
	float _GetAxis(string axisName){
		// look at the dictionary mapping, and check the correct button
		float rVal = 0;
		for(int i=0;i<inputMap.axisMapping.Count;i++){
			if(inputMap.axisMapping[i].id == axisName){
				rVal += inputMap.axisMapping[i].GetRaw();
			}
		}
		return rVal;
	}	
	public static float GetAxisRaw(string buttonName){
		return LFInput.Instance._GetAxisRaw(buttonName);
	}
	float _GetAxisRaw(string axisName){
		// look at the dictionary mapping, and check the correct button
		float rVal = 0;
		for(int i=0;i<inputMap.axisMapping.Count;i++){
			if(inputMap.axisMapping[i].id == axisName){
				if(inputMap.axisMapping[i].aType == LFAxisInputMap.axisType.JOYSTICK){
					rVal += Input.GetAxisRaw(inputMap.axisMapping[i].joystickString);
				}else{
					rVal += (Input.GetKey(inputMap.axisMapping[i].keyNeg)?-1:0)+(Input.GetKey(inputMap.axisMapping[i].keyPos)?1:0);
				}
			}
		}
		return rVal;
	}
	void Update(){
		// update the button mappings, for using joystick axis as buttons
		#if UNITY_STANDALONE
			for(int i=0;i<inputMap.buttonMapping.Count;i++){
				inputMap.buttonMapping[i].Update();
			}
			for(int i=0;i<inputMap.axisMapping.Count;i++){
				inputMap.axisMapping[i].Update();
			}

		#endif
		_tempTouch.Clear();
		#if (!UNITY_IPHONE && !UNITY_ANDROID) || UNITY_EDITOR
			// if we are in the editor, then copy in the mouse information into the structs
			for(int i=0;i<2;i++){
				LFTouch t = new LFTouch();
				bool touchAdded = false;
				if(Input.GetMouseButtonDown(i)){
					// Debug.Log("mouse down");
					buttonsDown[i] = true;
					t.phase = TouchPhase.Began;
					touchAdded = true;
				}else if(Input.GetMouseButton(i)){
					t.phase = TouchPhase.Moved;
					touchAdded = true;
				}else if(Input.GetMouseButtonUp(i)){
					// Debug.Log("mouse up");
					buttonsDown[i] = false;
					t.phase = TouchPhase.Ended;
					touchAdded = true;
				}
				// seemed to be losing hits, so have to force them here
				if(buttonsDown[i] && !Input.GetMouseButton(i)){
					// Debug.Log("button force");
					buttonsDown[i] = false;
					t.phase = TouchPhase.Ended;
					touchAdded = true;
				}
				if(touchAdded){
					t.fingerId = i;
					t.position = Input.mousePosition;
					_tempTouch.Add(t);
				}
			}
			_tempTouch.ToArray();
		#else
			// copy in the actual touches
			for(int i=0;i<Input.touches.Length;i++){
				LFTouch t = touchBuffer[i];
				t.fingerId = Input.touches[i].fingerId;
				t.position = Input.touches[i].position;
				t.phase = Input.touches[i].phase;
				_tempTouch.Add(t);
			}
		#endif
	}
}

public struct LFTouch{
	public int fingerId;
	public Vector2 position;
	public Vector2 deltaPosition;
	public float deltaTime;
	public int tapCount;
	public TouchPhase phase;
}
