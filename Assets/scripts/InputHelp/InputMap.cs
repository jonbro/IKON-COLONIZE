using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

// also overloading a whole input manager replacement thing here, just for fun.
[XmlRoot("InputMap")]
public class InputMap{

	[XmlArray("buttonMapping")]
 	[XmlArrayItem("LFButtonInputMap")]
	public List<LFButtonInputMap> buttonMapping = new List<LFButtonInputMap>();

	[XmlArray("axisMapping")]
 	[XmlArrayItem("LFAxisInputMap")]
	public List<LFAxisInputMap> axisMapping = new List<LFAxisInputMap>();
	public void Save(string path){
		var serializer = new XmlSerializer(typeof(InputMap));
		using(var stream = new FileStream(path, FileMode.Create)){
			serializer.Serialize(stream, this);
		}
	}
	public static InputMap Load(string path)
 	{
 		var serializer = new XmlSerializer(typeof(InputMap));
 		using(var stream = new FileStream(path, FileMode.Open))
 		{
 			InputMap iMap = serializer.Deserialize(stream) as InputMap;
 			return iMap;
 		}
 	}
 	public LFButtonInputMap GetButtonMap(string id, LFButtonInputMap.buttonType bType){
		for(int i=0;i<buttonMapping.Count;i++){
			if(buttonMapping[i].id == id && buttonMapping[i].bType == bType){
				return buttonMapping[i];
			}
		}
		return null;
 	}
 	public LFAxisInputMap GetAxisMap(string id, LFAxisInputMap.axisType bType){
		for(int i=0;i<axisMapping.Count;i++){
			if(axisMapping[i].id == id && axisMapping[i].aType == bType){
				return axisMapping[i];
			}
		}
		return null;
 	}
 	public void SetDefaults(){
 		buttonMapping.Clear();
 		axisMapping.Clear();
		buttonMapping.Add(new LFButtonInputMap("p1 flip", LFButtonInputMap.buttonType.KEYBOARD, KeyCode.C, 0, 0));
		buttonMapping.Add(new LFButtonInputMap("p1 fire", LFButtonInputMap.buttonType.KEYBOARD, KeyCode.V, 0, 0));


		buttonMapping.Add(new LFButtonInputMap("p2 flip", LFButtonInputMap.buttonType.KEYBOARD, KeyCode.Comma, 0, 0));
		buttonMapping.Add(new LFButtonInputMap("p2 fire", LFButtonInputMap.buttonType.KEYBOARD, KeyCode.Period, 0, 0));

		buttonMapping.Add(new LFButtonInputMap("p1 weapon switch", LFButtonInputMap.buttonType.KEYBOARD, KeyCode.Space, 0, 0));

		axisMapping.Add(new LFAxisInputMap("p1 horizontal", LFAxisInputMap.axisType.KEYBOARD, KeyCode.A, KeyCode.S, 0,0));
		axisMapping.Add(new LFAxisInputMap("p2 vertical", LFAxisInputMap.axisType.KEYBOARD, KeyCode.UpArrow, KeyCode.DownArrow, 0,0));
		axisMapping.Add(new LFAxisInputMap("p2 horizontal", LFAxisInputMap.axisType.KEYBOARD, KeyCode.LeftArrow, KeyCode.RightArrow, 0,0));

		LFButtonInputMap cax;

		for(int i=1;i<=4;i++){
			axisMapping.Add(new LFAxisInputMap("p"+i+" horizontal", LFAxisInputMap.axisType.JOYSTICK, KeyCode.None, KeyCode.S, i,1));
			axisMapping.Add(new LFAxisInputMap("p"+i+" vertical", LFAxisInputMap.axisType.JOYSTICK, KeyCode.None, KeyCode.S, i,2));

			#if UNITY_STANDALONE_OSX || UNITY_EDITOR
				buttonMapping.Add(new LFButtonInputMap("p"+i+" flip", LFButtonInputMap.buttonType.JOYSTICK, KeyCode.C, i, 16));
				buttonMapping.Add(new LFButtonInputMap("p"+i+" fire", LFButtonInputMap.buttonType.JOYSTICK, KeyCode.V, i, 17));
				buttonMapping.Add(new LFButtonInputMap("p"+i+" weapon switch", LFButtonInputMap.buttonType.JOYSTICK, KeyCode.V, i, 19));
			#else
				buttonMapping.Add(new LFButtonInputMap("p"+i+" flip", LFButtonInputMap.buttonType.JOYSTICK, KeyCode.C, i, 0));
				buttonMapping.Add(new LFButtonInputMap("p"+i+" fire", LFButtonInputMap.buttonType.JOYSTICK, KeyCode.V, i, 1));
				buttonMapping.Add(new LFButtonInputMap("p"+i+" weapon switch", LFButtonInputMap.buttonType.JOYSTICK, KeyCode.V, i, 3));
			#endif		
		}
	}
}

public class LFButtonInputMap {
	public string configGroup;
	public enum buttonType {JOYSTICK, KEYBOARD, JOYSTICK_AXIS};
	[XmlAttribute("bType")]
	public buttonType bType;

	private int _joystickNumber;
	[XmlAttribute("joystickNumber")]
	public int joystickNumber{
		get { return _joystickNumber;}
		set {
			_joystickNumber = value;
			joystickString = "joystick "+joystickNumber+" button "+joystickButton;
		}
	}


	private int _joystickButton;
	[XmlAttribute("joystickButton")]
	public int joystickButton{
		get { return _joystickButton;}
		set {
			_joystickButton = value;
			joystickString = "joystick "+joystickNumber+" button "+joystickButton;
		}
	}

	public string joystickString;
	public string id;
	public KeyCode key;
	public float direction;
	public float limit;
	public bool isPressed;
	public bool down;
	public LFButtonInputMap(){

	}
	public LFButtonInputMap(string _id, buttonType _bType, KeyCode _key, int jNumber, int jButton){
		id = _id;
		bType = _bType;
		key = _key;
		joystickNumber = jNumber;
		joystickButton = jButton;
		// build the joystick string based on the button input
	}
	// for axis mapping
	public void SetDirection(float _direction, float _limit){
		direction = _direction;
		limit = _limit;
	}
	public void Update(){
		down = false;
		if(bType == buttonType.JOYSTICK_AXIS){
			if(direction < 0 && Input.GetAxis("Joy"+joystickNumber+" Axis "+joystickButton) < limit){
				if(!isPressed){
					down = true;
					isPressed = true;
				}
			}else if(direction > 0 && Input.GetAxis("Joy"+joystickNumber+" Axis "+joystickButton) > limit){
				if(!isPressed){
					down = true;
					isPressed = true;
				}
			}else{
				isPressed = false;
			}
		}
	}
};

public class LFAxisInputMap {
	private int _joystickAxis;
	private int _joystickNumber;
	public enum axisType {JOYSTICK, KEYBOARD, JOYSTICK_BUTTON};
	public string configGroup;
	[XmlAttribute("aType")]
	public axisType aType;
	[XmlAttribute("joystickNumber")]
	public int joystickNumber
	{
		get { return _joystickNumber;}
		set {
			_joystickNumber = value;
			joystickString = "Joy"+joystickNumber+" Axis "+joystickAxis;
		}
	}
	[XmlAttribute("joystickAxis")]
	public int joystickAxis
	{
		get { return _joystickAxis;}
		set {
			_joystickAxis = value;
			joystickString = "Joy"+joystickNumber+" Axis "+joystickAxis;
		}
	}
   	[XmlIgnoreAttribute]
	public string joystickString;
	public string id;
	public KeyCode keyNeg;
	public KeyCode keyPos;

	public int joystickNumberNeg;
	public int joystickButtonNeg;

	public int joystickNumberPos;
	public int joystickButtonPos;

	public bool isPressedPos;
	public bool downPos;

	public bool isPressedNeg;
	public bool downNeg;

	public LFAxisInputMap(){
		
	}
	public LFAxisInputMap(string _id, axisType _aType, KeyCode _keyNeg, KeyCode _keyPos, int _joystickNumber, int _joystickAxis){
		id = _id;
		aType = _aType;
		keyPos = _keyPos;
		keyNeg = _keyNeg;
		joystickNumber = _joystickNumber;
		joystickAxis = _joystickAxis;
	}
	float neg;
	float pos;
	public float GetRaw(){
		if(aType == axisType.JOYSTICK)
			return Input.GetAxis(joystickString);
		if(aType == axisType.KEYBOARD)
			return (Input.GetKey(keyNeg)?-1:0)+(Input.GetKey(keyPos)?1:0);

		// otherwise we are using buttons for input
		neg = 0;
		pos = 0;
		if(joystickNumberNeg >=0)
			neg = (Input.GetKey("joystick "+joystickNumberNeg+" button "+joystickButtonNeg)?-1:0);
		if(joystickNumberPos >=0)
			pos = (Input.GetKey("joystick "+joystickNumberPos+" button "+joystickButtonPos)?1:0);
		return neg+pos;
	}
	public bool GetDownNeg(){
		return downNeg;
	}
	public bool GetDownPos(){
		return downPos;
	}
	float limit = 0.8f;
	public void Update(){
		downNeg = false;
		downPos = false;
		if(aType == axisType.JOYSTICK){
			if(Input.GetAxis(joystickString) < -limit){
				if(!isPressedNeg){
					downNeg = true;
					isPressedNeg = true;
				}
			}else{
				isPressedNeg = false;
			}
			if(Input.GetAxis(joystickString) > limit){
				if(!isPressedPos){
					downPos = true;
					isPressedPos = true;
				}
			}else{
				isPressedPos = false;
			}
		}else if(aType == axisType.KEYBOARD){
			downNeg = Input.GetKeyDown(keyNeg);
			downPos = Input.GetKeyDown(keyPos);
		}else{
			if(joystickNumberNeg >= 0)
				downNeg = Input.GetKeyDown("joystick "+joystickNumberNeg+" button "+joystickButtonNeg);
			if(joystickNumberPos >= 0)
				downPos = Input.GetKeyDown("joystick "+joystickNumberPos+" button "+joystickButtonPos);
		}
	}
};
