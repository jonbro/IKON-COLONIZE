using UnityEngine;
using System.Collections;


public class Tower : MonoBehaviour {
	public Unit u{
		get{
			return GetComponent<UnitBase>().u;
		}
	}
	void OnPhotonInstantiate(PhotonMessageInfo info){
		Setup((int)GetComponent<PhotonView>().instantiationData[0], (int)GetComponent<PhotonView>().instantiationData[1], (Vector2)GetComponent<PhotonView>().instantiationData[2]);
	}

	[RPC]
	public void Setup(int _pid, int _ourColor, Vector2 position){
		int pid = _pid;
		GetComponent<UnitBase>().Setup(_pid, HullBreachGameController.globalColors[_ourColor]);
		// position the turret unit a 1/3 of the way to the target core
		u.position = position;
		u.displaySize = 12;
		GetComponent<UnitBase>().attackRadius = 20;
		u.SetHealth(150);
		u.attackPower = 35;
		u.attackCooldown = 2.0f;
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	    if (stream.isWriting)
	    {
	        // We own this player: send the others our data
	        stream.SendNext(u.position);
	        stream.SendNext(GetComponent<UnitBase>().attacking);
	    }
	    else
	    {
	        // Network player, receive data
	        this.u.position = (Vector2)stream.ReceiveNext();
	        this.GetComponent<UnitBase>().attacking = (bool)stream.ReceiveNext();
	    }
	}
}