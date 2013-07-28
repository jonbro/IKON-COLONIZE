using UnityEngine;
using System.Collections;

// an immediate mode gui
public class VectorGui : MonoBehaviour {

	protected DrawString strings;
	protected LineRenderManager lines;
	protected Vector2 penPosition;
	protected Transform penTransform;
	public static int activeId;
	public static int currentId;	
	private static VectorGui instance;
	float padding = 20;

	public static VectorGui Instance {
		get
		{
			if(instance == null){
				GameObject vg = new GameObject("VectorGui");
				instance = vg.AddComponent<VectorGui>();
			}
			return instance;
		}
	}
	void Awake(){
		DontDestroyOnLoad(gameObject);
		// setup the string art and the line art
		penPosition = new Vector2(0, Camera.main.pixelHeight);
		if(!GameObject.Find("LineRenderManager")){
			Debug.LogError("Add a line renderer to the scene");
		}
		penTransform = new GameObject().transform;
		DontDestroyOnLoad(penTransform);
		lines = GameObject.Find("LineRenderManager").GetComponent<LineRenderManager>();
		strings = lines.GetComponent<DrawString>();
	}
	void Start(){
	}
	static int GetId(){
		currentId++;
		return currentId;
	}
	void Update(){
		currentId = 0;
		if(lines == null){
			lines = GameObject.Find("LineRenderManager").GetComponent<LineRenderManager>();
			strings = lines.GetComponent<DrawString>();
		}
		// reset the start position
		penPosition = new Vector2(padding, Camera.main.pixelHeight-padding);
	}
	public static Transform Pen(){
		Instance.penTransform.position = Camera.main.ScreenToWorldPoint(new Vector3(Instance.penPosition.x, Instance.penPosition.y, 20));
		return Instance.penTransform;
	}
	public static void ProgressBar(float ratio, Color color){
		Instance._ProgressBar(ratio, color);
	}
	public void _ProgressBar(float ratio, Color color){
		// draw the bounding box
		Rect r = new Rect(penPosition.x-5, penPosition.y+5-10, 200, 10);
		DrawRect(r, Color.Lerp(color, Color.black, 0.5f));
		// fill the rect with the various bits
		for(int i=0;i<(int)(50*ratio);i++){
			float xpos = r.x+200/50.0f*i;
			lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(xpos, r.y, 20)), Camera.main.ScreenToWorldPoint(new Vector3(xpos, r.yMax, 20)), color);
		}
	}
	public static void Label(string LabelText){
		// draw the label
		Label(LabelText, 0.1f);
	}
	public static void Label(string LabelText, float scale, Color color){
		// draw the label
		Instance._Label(LabelText, scale, color);
	}
	public static void Label(string LabelText, float scale){
		Instance._Label(LabelText, scale);
	}
	public void _Label(string LabelText, float scale){
		_Label(LabelText, scale, Color.white);
	}
	public void _Label(string LabelText, float scale, Color c){
		strings.Text(LabelText, Pen(), scale, c);
		// move the pen in world space to calculate the new screen space position
		Vector3 wp = Pen().position;
		// the text is 3 tall
		// going with 0.1 as the default line padding
		wp -= new Vector3(0, scale*3.0f + 0.1f*3.0f, 0);
		// and move the pen
		Vector3 sp = Camera.main.WorldToScreenPoint(wp);
		penPosition = new Vector2(sp.x, sp.y);		
	}
	public static string TextInput(string inputText){
		// render a button here
		int id = GetId();
		if(activeId == currentId){
			// we are currently using the text field, so should render a cursor...
			//
			foreach (char c in Input.inputString) {
			    if (c == "\b"[0]){
			        if (inputText.Length != 0){
			        	Debug.Log(inputText.Length);
			        	if(inputText.Length == 1){
			        		inputText = "";
			        	}else{
				            inputText = inputText.Substring(0, inputText.Length - 1);
			        	}
			        }else{
			        	Debug.Log("should be deleting, but not?");
			        }
			    } else {
			        // if (c == "\n"[0] || c == "\r"[0]) {
			        	// pressed enter
			            // print("User entered his name: " + inputText);
			        // }else{
			            inputText += c;
			        // }
			    }
			}
		}
		Instance._Button(inputText, id);
		return inputText;
	}
	public static bool Button(string buttonText){
		return instance._Button(buttonText);
	}
	public bool _Button(string buttonText, int _id=-1){
		// auto gen a rectangle for this button
		int id = _id;
		if(_id<0){
			id = GetId();
		}
		Rect buttonRect = new Rect(penPosition.x-5, penPosition.y+5-30, 200, 30);
		bool hovered = false;
		Color buttonColor = Color.white;
		if(buttonRect.Contains(Input.mousePosition)){
			hovered = true;
			buttonColor = Color.green;
		}
		// draw the outlines of the button
		DrawRect(buttonRect, buttonColor);
		// draw the button shadown
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.y+2, 20)), Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.xMax-2, buttonRect.y+2, 20)), buttonColor*0.5f);
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.yMax, 20)), Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.y+2, 20)), buttonColor*0.5f);


		// draw a label for this button
		strings.Text(buttonText, Pen(), 0.1f, buttonColor);

		penPosition = new Vector2(penPosition.x, penPosition.y-40);

		// check to see if the mouse went down over the rect in the last frame, if so mark it as clicked
		if(hovered && Input.GetMouseButtonDown(0)){
			activeId = id;
			return true;
		}
		return false;
	}
	public void DrawRect(Rect r, Color c){
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(r.x, r.y, 20)), Camera.main.ScreenToWorldPoint(new Vector3(r.xMax, r.y, 20)), c);
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(r.xMax, r.y, 20)), Camera.main.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, 20)), c);
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, 20)), Camera.main.ScreenToWorldPoint(new Vector3(r.x, r.yMax, 20)), c);
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(r.x, r.yMax, 20)), Camera.main.ScreenToWorldPoint(new Vector3(r.x, r.y, 20)), c);
	}
}
