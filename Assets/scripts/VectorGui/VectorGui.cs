using UnityEngine;
using System.Collections;

// an immediate mode gui
public class VectorGui : MonoBehaviour {

	protected DrawString strings;
	protected LineRenderManager lines;
	protected Vector2 penPosition;
	protected Transform penTransform;

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
	void Update(){
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
	public static void Label(string LabelText){
		// draw the label
		Label(LabelText, 0.1f);
	}
	public static void Label(string LabelText, float scale){
		Instance._Label(LabelText, scale);
	}
	public void _Label(string LabelText, float scale){
		strings.Text(LabelText, Pen(), scale);
		// move the pen in world space to calculate the new screen space position
		Vector3 wp = Pen().position;
		// the text is 3 tall
		// going with 0.1 as the default line padding
		wp -= new Vector3(0, scale*3.0f + 0.1f*3.0f, 0);
		// and move the pen
		Vector3 sp = Camera.main.WorldToScreenPoint(wp);
		penPosition = new Vector2(sp.x, sp.y);		
	}
	public static bool Button(string buttonText){
		return instance._Button(buttonText);
	}
	public bool _Button(string buttonText){
		// auto gen a rectangle for this button
		Rect buttonRect = new Rect(penPosition.x-5, penPosition.y+5-30, 200, 30);
		bool hovered = false;
		Color buttonColor = Color.white;
		if(buttonRect.Contains(Input.mousePosition)){
			hovered = true;
			buttonColor = Color.green;
		}
		// draw the outlines of the button
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.y, 20)), Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.xMax, buttonRect.y, 20)), buttonColor);
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.xMax, buttonRect.y, 20)), Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.xMax, buttonRect.yMax, 20)), buttonColor);
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.xMax, buttonRect.yMax, 20)), Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.yMax, 20)), buttonColor);
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.yMax, 20)), Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.y, 20)), buttonColor);
		// draw the button shadown
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.y+2, 20)), Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.xMax-2, buttonRect.y+2, 20)), buttonColor*0.5f);
		lines.AddLine(Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.yMax, 20)), Camera.main.ScreenToWorldPoint(new Vector3(buttonRect.x, buttonRect.y+2, 20)), buttonColor*0.5f);


		// draw a label for this button
		strings.Text(buttonText, Pen(), 0.1f, buttonColor);

		penPosition = new Vector2(penPosition.x, penPosition.y-40);

		// check to see if the mouse went down over the rect in the last frame, if so mark it as clicked

		return hovered && Input.GetMouseButtonDown(0);
	}
}
