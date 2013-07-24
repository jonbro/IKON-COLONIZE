using UnityEngine;
using System.Collections;

public class ExplosionPoint{
	public Vector3 position;
	public Vector3 direction;
	public float speed;
	public float lifetime;
	public float age;
	public bool dead;
	public Color startColor;
	public Color endColor;
}

public class ExplosionLine{
	public Vector3 pointA, pointB;
	public Vector3 directionA, directionB;
	public float speed;
	public float lifetime;
	public float age;
	public bool dead;
	public Color startColor;
	public Color endColor;
}

public class ExplosionManager : MonoBehaviour {
	// should take a chunk of line segments and push them in a direction and fade out over time. Particles with lines basically
	// Use this for initialization
	ExplosionPoint[] points;	
	int maxPoints = 1000;
	int currentPoint = 0;

	ExplosionLine[] elines;
	int maxLines = 1000;
	int currentLine = 0;

	LineRenderManager lines;
	void Start () {
		lines = GetComponent<LineRenderManager>();
		points = new ExplosionPoint[maxPoints];
		for(int i=0;i<points.Length;i++){
			points[i] = new ExplosionPoint();
		}
		elines = new ExplosionLine[maxLines];
		for(int i=0;i<elines.Length;i++){
			elines[i] = new ExplosionLine();
		}

	}
	
	// Update is called once per frame
	void Update () {
		ExplosionPoint ep = points[0];
		for(int i=0;i<points.Length;i++){
			if(!points[i].dead){
				ep = points[i];
				// draw the point
				lines.AddLine(ep.position, ep.position+ep.direction*Time.deltaTime, Color.Lerp(ep.startColor, ep.endColor, ep.age/ep.lifetime));
				// update the point
				ep.position += ep.direction*Time.deltaTime;
				ep.age += Time.deltaTime;
				if(ep.age > ep.lifetime)
					ep.dead = true;
			}
		}
		ExplosionLine el = elines[0];
		for(int i=0;i<elines.Length;i++){
			if(!elines[i].dead){
				el = elines[i];
				// draw the point
				lines.AddLine(el.pointA, el.pointB, Color.Lerp(el.startColor, el.endColor, el.age/el.lifetime));
				// update the point
				el.pointA += el.directionA*Time.deltaTime;
				el.pointB += el.directionB*Time.deltaTime;
				el.age += Time.deltaTime;
				if(el.age > el.lifetime)
					el.dead = true;
			}
		}
	}
	public void AddLine(Vector3 pa, Vector3 pb, Vector3 da, Vector3 db, Color sc){
		ExplosionLine el = elines[currentLine];
		el.pointA = pa;
		el.pointB = pb;
		el.directionA = da;
		el.directionB = db;
		el.age = 0;
		el.dead = false;
		el.lifetime = 1.0f;
		el.startColor = sc;
		el.endColor = Color.clear;
		currentLine = (currentLine+1)%elines.Length;		
	}
	public void Emit(Vector3 startPosition, Vector3 dir){
		ExplosionPoint ep = points[currentPoint];
		ep.position = startPosition;
		ep.direction = dir;
		ep.age = 0;
		ep.dead = false;
		ep.lifetime = 1.0f;
		ep.startColor = Color.white;
		ep.endColor = Color.clear;
		currentPoint = (currentPoint+1)%points.Length;
	}
}
