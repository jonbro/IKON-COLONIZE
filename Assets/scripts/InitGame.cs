using UnityEngine;
using System.Collections;

public class InitGame : MonoBehaviour {
	PhotonView photonView;
	// Use this for initialization
	void Awake(){
        PhotonNetwork.isMessageQueueRunning = true;
        // send an rpc to inform the server
        int id1 = PhotonNetwork.AllocateViewID();
  		photonView = GetComponent<PhotonView>();
    	photonView.RPC("SpawnOnNetwork", PhotonTargets.AllBuffered, id1, PhotonNetwork.player);
    	// if we are the master server, setup all the players, and assign ownership
	}

	[RPC]
	void SpawnOnNetwork(int id1, PhotonPlayer np)
	{ 
		GameObject newPlayer = (GameObject)Instantiate(Resources.Load("PlayerManager"), Vector3.zero, Quaternion.identity);
		PhotonView nView = newPlayer.gameObject.GetComponent<PhotonView>();
		nView.viewID = id1;
	}
}