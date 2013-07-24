using UnityEngine;
using System.Collections;
using System;

public class touchManager : MonoBehaviour {

	// basic event callbacks
    public event OnPressHandler OnPress;
    public event OnReleaseHandler OnRelease;
    public EventArgs e = null;    
    public delegate void OnPressHandler(touchManager m, EventArgs e);
    public delegate void OnReleaseHandler(touchManager m, EventArgs e);
	
	bool[] fingersDown = new bool[15];
	Vector2[] lastPos = new Vector2[15];
	public bool justTouched;
	public bool justUnTouched;
	public bool touching;
	public bool draggable = false;
	public Vector2 lastMouse;
	// Use this for initialization
	void Start () {
		for (int i = 0; i < fingersDown.Length; i++) {
			fingersDown[i] = false;
		}
		justTouched = false;
		touching = false;
	}
	int countFingers(){
		int count = 0;
		for (int i = 0; i < fingersDown.Length; i++) {
			if(fingersDown[i]){
				count++;				
			}
		}
		return count;
	}	
	// Update is called once per frame
	void Update () {
		justTouched = false;
		justUnTouched = false;
        foreach (LFTouch touch in LFInput.touches) {
			if(touch.phase == TouchPhase.Began){
				// raycast into the object and see if we need to broadcast
				Ray ray = Camera.main.ScreenPointToRay(touch.position);
				RaycastHit hit;
				// Debug.DrawRay(ray.origin, ray.direction*5000, Color.white, 5);
			    if (collider.Raycast(ray, out hit, 1000)){
					if(countFingers()==0){
						if(OnPress != null){
							OnPress(this, e);
						}
						justTouched = true;
						touching = true;
					}
					fingersDown[touch.fingerId]	= true;
		    	}
			}
			if(touch.phase == TouchPhase.Moved){
				Ray ray = Camera.main.ScreenPointToRay(touch.position);
				RaycastHit hit;
				if(draggable && fingersDown[touch.fingerId]){
					//Debug.DrawLine(Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0)),
						//Camera.main.ScreenToWorldPoint(new Vector3(lastPos[touch.fingerId].x,lastPos[touch.fingerId].y, 0)));
					Vector3 screenDelta = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
					screenDelta -= Camera.main.ScreenToWorldPoint(new Vector3(lastPos[touch.fingerId].x,lastPos[touch.fingerId].y, 0));
					transform.position += screenDelta;
				}else if (collider.Raycast(ray, out hit, 1000) && !draggable){
					if(!fingersDown[touch.fingerId]){
						if(countFingers()==0){
							justTouched = true;
							touching = true;
						}						
					}
					fingersDown[touch.fingerId]	= true;
		    	}else{
					fingersDown[touch.fingerId]	= false;
					if(countFingers()==0){
						touching = false;
					}						
				}
			}
			if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled){
				if(fingersDown[touch.fingerId]){
					fingersDown[touch.fingerId]	= false;
					if(countFingers()==0){
						touching = false;
						justUnTouched = true;					
						if(OnRelease != null){
							OnRelease(this, e);
						}
					}
				}
			}
			lastPos[touch.fingerId] = touch.position;
        }
		lastMouse = Input.mousePosition;
	}
}
