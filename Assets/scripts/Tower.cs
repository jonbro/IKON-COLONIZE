using UnityEngine;
using System.Collections;


public class Tower : MonoBehaviour {
	public Unit u{
		get{
			return GetComponent<UnitBase>().u;
		}
	}

	public void Setup(int _pid, Color _ourColor, Unit ourCore, Unit targetCore){
		GetComponent<UnitBase>().Setup(_pid, _ourColor);
		// position the turret unit a 1/3 of the way to the target core
		Vector2 dir = targetCore.position-ourCore.position;
		u.position = ourCore.position + dir.normalized * (dir.magnitude*(1/3.0f));
		u.displaySize = 12;
		GetComponent<UnitBase>().attackRadius = 28;
		u.SetHealth(200);
		u.attackPower = 40;
		u.attackCooldown = 0.6f;
	}
}