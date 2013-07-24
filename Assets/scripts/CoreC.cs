using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CoreC : MonoBehaviour {
	public Unit u{
		get{
			return GetComponent<UnitBase>().u;
		}
	}
	List<UnitBase> targets;
	// Use this for initialization
	void Start () {
		targets = new List<UnitBase>();
	}
	public void Setup(int _pid, Color _ourColor){
		int pid = _pid;
        GetComponent<UnitBase>().Setup(_pid, _ourColor);
		u.position = (Vector2.one*130).Rotate((pid*(360/3.0f))*Mathf.Deg2Rad);
		u.displaySize = 25;
		GetComponent<UnitBase>().attackRadius = 27;
		u.attackPower = 100;
		u.attackCooldown = 10f;
		u.SetHealth(400);
	}	
	// Update is called once per frame
	void Update () {
		// display current xp
	}
	public void CheckNeighbors(List<UnitBase> units){
		targets.Clear();

		foreach(UnitBase unit in units){
			float dist = Vector2.Distance(unit.u.position, u.position);
			if(dist - unit.u.displaySize < GetComponent<UnitBase>().attackRadius){
				// we are within the attack radius!
				unit.temporaryDistance = dist;
				targets.Add(unit);
			}
		}
		targets.Sort(delegate(UnitBase p1, UnitBase p2)
		    {
		        return (p1.temporaryDistance > p2.temporaryDistance)?-1:1;
		    }
		);
		GetComponent<UnitBase>().attacking = false;
		if(targets.Count > 0){
			// if the nearest target is our player, then heal to the maximum
			if(targets[0].GetComponent<PlayerManager>() && targets[0].u.owner == u.owner){
				// heal!
				targets[0].u.health = targets[0].u.fullHealth;
				GetComponent<UnitBase>().attackCooldownCounter = u.attackCooldown;
			}else if(targets[0].u.owner != u.owner){
				GetComponent<UnitBase>().attacking = true;
				GetComponent<UnitBase>().AttackTarget(targets[0]);
			}
		}
	}
}
