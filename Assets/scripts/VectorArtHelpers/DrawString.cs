using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// using MiniJson;

public class DrawString : MonoBehaviour {
	public TextAsset fontJson;
	Dictionary<string, object> dict;
	LineRenderManager lines;
	public float _scale = 0.1f;
	float scale;
	// Use this for initialization
	void Start () {
		scale = _scale;
		dict = Json.Deserialize(fontJson.text) as Dictionary<string,object>;
		lines = GameObject.Find("LineRenderManager").GetComponent<LineRenderManager>();
	}
	
	// Update is called once per frame
	void Update () {

	}
	public void Text(string output, Transform offset, float overrideScale, Color textColor){
		scale = overrideScale;
		Vector3 letterOffset = Vector3.zero;
		for(int i=0;i<output.Length;i++){
			if(dict.ContainsKey(output[i].ToString())){
				List<object> letter = (List<object>)dict[output[i].ToString()];
				for(int k=0;k<letter.Count;k++){
					List<object> points = (List<object>)letter[k];
					for(int j = 0; j<points.Count-1;j++){
						List<object> p1 = (List<object>)points[j];
						List<object> p2 = (List<object>)points[j+1];
						Vector3 pa = new Vector3(float.Parse(p1[0].ToString()), -float.Parse(p1[1].ToString()), 0);
						Vector3 pb = new Vector3(float.Parse(p2[0].ToString()), -float.Parse(p2[1].ToString()), 0);
						pa*=scale;
						pb*=scale;
						lines.AddLine(offset.TransformPoint(pa+letterOffset), offset.TransformPoint(pb+letterOffset), textColor);
					}
				}
			}
			letterOffset += new Vector3(3*scale, 0, 0);
		}		
	}
	public void Text(string output, Transform offset, float overrideScale){
		Text(output, offset, overrideScale, Color.white);
	}
}
