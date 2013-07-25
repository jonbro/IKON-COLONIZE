using UnityEngine;
using System.Collections;

public class InitGame : MonoBehaviour {
	PhotonView photonView;
	// Use this for initialization
	void Start(){
        PhotonNetwork.isMessageQueueRunning = true;
        if(PhotonNetwork.isMasterClient){
	        PhotonNetwork.InstantiateSceneObject("HBGameController", Vector3.zero, Quaternion.identity, 0, null);
        }
	}
}