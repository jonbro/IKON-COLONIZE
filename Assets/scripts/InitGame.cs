using UnityEngine;
using System.Collections;

public class InitGame : MonoBehaviour {
	PhotonView photonView;
	public static int[] playerMapper;
	int currentPlayer = 0;
	int playersMapped = 0;
	bool started;
	// Use this for initialization
	void Start(){
        PhotonNetwork.isMessageQueueRunning = true;
        playerMapper = new int[3];
        if(PhotonNetwork.isMasterClient){
        	GetComponent<PhotonView>().RPC("AssignIdToPlayer", PhotonTargets.AllBuffered, currentPlayer, PhotonNetwork.player.ID);
        	currentPlayer++;
        }
	}
	void Update(){
		// wait until all players are connected
		if(playersMapped < 3){
            VectorGui.Label("Waiting for " + (3-playersMapped) + " players");
		}else{
	        if(PhotonNetwork.isMasterClient && !started){
		        PhotonNetwork.InstantiateSceneObject("HBGameController", Vector3.zero, Quaternion.identity, 0, null);
		        started = true;
	        }
		}
	}
    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        if(PhotonNetwork.isMasterClient){
        	GetComponent<PhotonView>().RPC("AssignIdToPlayer", PhotonTargets.AllBuffered, currentPlayer, player.ID);
        	currentPlayer++;
        }
    }

    [RPC]
    void AssignIdToPlayer(int p, int playerId){
    	if(p<3){
    		playerMapper[p] = playerId;
    		playersMapped++;
    	}
    }
}