using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Creep : MonoBehaviour {
	
	public bool alive = true;
	public Vector2 target;
	public Unit u{
		get{
			return GetComponent<UnitBase>().u;
		}
	}
	float maxSpeed = 10.0f;
	float maxForce = 2.5f;
	Vector2 vel;
	Vector2 acc;
	List<UnitBase> targets;
	// Use this for initialization
	LineRenderManager lines;
	void Start () {
		targets = new List<UnitBase>();
		// maxForce *= Time.fixedDeltaTime;
		maxSpeed *= Time.fixedDeltaTime;
        lines = GameObject.Find("LineRenderManager").GetComponent<LineRenderManager>();
	}
	new public void Setup(int _pid, Color _ourColor, Unit ourCore, Unit targetCore){
		GetComponent<UnitBase>().Setup(_pid, _ourColor);
		Vector2 dir = targetCore.position-ourCore.position;
		target = targetCore.position;
		u.position = ourCore.position + dir.normalized * (2.0f+Random.value*0.2f) ;
		u.displaySize = 3;
		u.SetHealth(50);
		u.attackPower = 20;
		u.attackCooldown = 1f;
		GetComponent<UnitBase>().attackRadius = 6;
	}
	public void ApplySteering(List<UnitBase> units){
		// accumulate forces and apply to the creep
		if(GetComponent<UnitBase>().attacking)
			return;
		Vector2 targetForce = target-u.position;
		
		targetForce = targetForce.normalized * maxSpeed;
		targetForce -= vel;
		targetForce = Vector2.ClampMagnitude(targetForce, maxForce);
		
		Vector2 attract = Attract(units);
		Vector2 sep = Sep(units);
		if(attract != Vector2.zero){
			targetForce *= 0.7f;
			sep *= 0.2f;
			acc += sep;
			acc += attract;

		}else{
			targetForce *= 0.5f;
			sep *= 0.5f;
			acc += targetForce;
			acc += sep;
		}
		vel += acc;

		vel = Vector2.ClampMagnitude(vel, maxSpeed);		
		u.position += vel;

		acc = Vector2.zero;
	}
	void Update(){
		if(targets[0] && targets[0].temporaryDistance < 70){
			lines.AddLine(targets[0].u.position, u.position, Color.white);
		}
	}
	Vector2 Attract(List<UnitBase> units){
		// should just attract to the nearest one
		targets.Clear();
		foreach(UnitBase unit in units){
			if(unit.u.owner != u.owner){
				float dist = Vector2.Distance(unit.u.position, u.position);
				unit.temporaryDistance = dist;
				targets.Add(unit);
			}
		}
		targets.Sort(delegate(UnitBase p1, UnitBase p2)
		    {
		        return (p1.temporaryDistance < p2.temporaryDistance)?-1:1;
		    }
		);
		// return an attraction force towards the nearest unit
		if(targets[0] && targets[0].temporaryDistance < 70){
			return ((targets[0].u.position - u.position).normalized*maxSpeed);
		}
		return Vector2.zero;
	}
	Vector2 Sep(List<UnitBase> units){
		// should steer away from units that are on the same team
		Vector2 steer = Vector2.zero;
		int count = 0;
		foreach(UnitBase c in units){
			if(c.u.owner == u.owner){
				float targetSep = (u.displaySize+c.u.displaySize)*2;
				float d = Vector2.Distance(u.position, c.u.position);
				// should account for the towers, possibly have different weighting for those...
				if(d>0 && d<targetSep){
					Vector2 dir = u.position - c.u.position;
					steer += dir.normalized/d;
					count++;
				}				
			}
		}
		if(count > 0){
			steer /= count;
			steer = steer.normalized*maxSpeed;
			steer -= vel;
			steer = Vector2.ClampMagnitude(steer, maxForce);
		}
		return steer;
	}
}