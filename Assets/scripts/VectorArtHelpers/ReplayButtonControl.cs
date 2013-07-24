using UnityEngine;
using System.Collections;

public class ReplayButtonControl : MonoBehaviour {
	float countDown = 1;
	public touchManager button;
	// Use this for initialization
	LineRenderManager lines;
	void Start () {
		lines = GameObject.Find("LineRenderManager").GetComponent<LineRenderManager>();
	}
	
	// Update is called once per frame
	void Update () {
		lines.AddCircle(transform.position, transform.localScale.x/2.0f, Color.white);
		for(int i=0;i<30;i++){
			lines.AddCircle(transform.position, transform.localScale.x/2.0f+i*0.1f+(Mathf.Sin(-Time.time*2.0f+i*0.4f)+2.0f)*0.1f, Color.white*1.0f/i);
		}
		countDown -= Time.deltaTime;
		if(countDown < 0 && button.justTouched){
			Application.LoadLevel("MainScene");
		}
	}
}
