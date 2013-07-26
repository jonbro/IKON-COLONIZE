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
	void OnPhotonInstantiate(PhotonMessageInfo info){
		Setup((int)GetComponent<PhotonView>().instantiationData[0], (int)GetComponent<PhotonView>().instantiationData[1]);
	}
	public void Setup(int _pid, int _ourColor){
		int pid = _pid;
		GetComponent<UnitBase>().Setup(_pid, HullBreachGameController.globalColors[_ourColor]);
		u.position = (Vector2.one*130).Rotate((pid*(360/3.0f))*Mathf.Deg2Rad);
		u.displaySize = 25;
		GetComponent<UnitBase>().attackRadius = 27;
		u.attackPower = 100;
		u.attackCooldown = 10f;
		u.SetHealth(400);
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	    if (stream.isWriting)
	    {
	        // We own this player: send the others our data
	        stream.SendNext(u.position);
	        stream.SendNext(u.health);
	        stream.SendNext(GetComponent<UnitBase>().attacking);
	        stream.SendNext(GetComponent<UnitBase>().attackCooldownCounter);
	    }
	    else
	    {
	        // Network player, receive data
	        this.u.position = (Vector2)stream.ReceiveNext();
	        this.u.health = (float)stream.ReceiveNext();
	        this.GetComponent<UnitBase>().attacking = (bool)stream.ReceiveNext();
	        this.GetComponent<UnitBase>().attackCooldownCounter = (float)stream.ReceiveNext();
	    }
	}
	// Update is called once per frame
	void Update () {
		// display current xp
	}
	public void CheckNeighbors(UnitBase[] units){
		targets.Clear();

		foreach(UnitBase unit in units){
			if(unit.GetComponent<PlayerManager>()&&unit.u.owner == u.owner){
				float dist = Vector2.Distance(unit.u.position, u.position);
				if(dist - unit.u.displaySize < GetComponent<UnitBase>().attackRadius){
					// we are within the attack radius!
					unit.temporaryDistance = dist;
					targets.Add(unit);
				}				
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
				GetComponent<PhotonView>().RPC("SetCooldown", PhotonTargets.AllBuffered, u.attackCooldown);
				targets[0].GetComponent<PhotonView>().RPC("SetHealth", PhotonTargets.AllBuffered, targets[0].u.fullHealth);
			}else if(targets[0].u.owner != u.owner){
				GetComponent<UnitBase>().attacking = true;
				GetComponent<UnitBase>().AttackTarget(targets[0]);
			}
		}
	}
}
