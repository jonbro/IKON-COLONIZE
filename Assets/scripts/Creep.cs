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

	void Start(){
		// maxForce *= Time.fixedDeltaTime;
		maxSpeed *= Time.fixedDeltaTime;
	}
	new public void Setup(int _pid, Color _ourColor, Unit ourCore, Unit targetCore){
		GetComponent<UnitBase>().Setup(_pid, _ourColor);
		Vector2 dir = targetCore.position-ourCore.position;
		target = targetCore.position;
		u.position = ourCore.position + dir.normalized * (2.0f+Random.value*0.2f) ;
		u.displaySize = 3;
		u.SetHealth(50);
		u.attackPower = 20;
		u.attackCooldown = 1.0f;
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
		Vector2 sep = Sep(units);
		targetForce *= 0.25f;
		sep *= 0.75f;
		acc += targetForce;
		acc += sep;
		vel += acc;

		vel = Vector2.ClampMagnitude(vel, maxSpeed);		
		u.position += vel;

		acc = Vector2.zero;
	}
	Vector2 Sep(List<UnitBase> units){
		Vector2 steer = Vector2.zero;
		int count = 0;
		foreach(UnitBase c in units){
			float targetSep = u.displaySize+c.u.displaySize;

			float d = Vector2.Distance(u.position, c.u.position);
			// should account for the towers, possibly have different weighting for those...
			if(d>0 && d<targetSep){
				Vector2 dir = u.position - c.u.position;
				steer += dir.normalized/d;
				count++;
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