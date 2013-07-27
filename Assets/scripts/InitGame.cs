using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InitGame : MonoBehaviour {
	PhotonView photonView;
	public static int[] playerMapper;
	List<string> playerNames;

	int currentPlayer = 0;
	int playersMapped = 0;
	bool started;
	// Use this for initialization
	void Start(){
		playerNames = new List<string>();
        PhotonNetwork.isMessageQueueRunning = true;
        playerMapper = new int[3];
    	for(int i=0;i<3; i++){
    		playerMapper[i] = -1;
    	}

        if(PhotonNetwork.isMasterClient){
        	GetComponent<PhotonView>().RPC("AssignIdToPlayer", PhotonTargets.AllBuffered, currentPlayer, PhotonNetwork.player.ID, PhotonNetwork.player.name);
        	currentPlayer++;
        }
	}
	void Update(){
		// wait until all players are connected
		if(playersMapped < 3){
            VectorGui.Label("Aboard "+PhotonNetwork.room.name+" :: Awaiting " + (3-playersMapped) + " factions");
            foreach(string name in playerNames){
            	VectorGui.Label(name);
            }
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
        	// find a free id
        	int freeId = 0;
        	for(int i=0;i<3; i++){
        		if(playerMapper[i]<0){
        			freeId = i;
        			break;
        		}
        	}
        	GetComponent<PhotonView>().RPC("AssignIdToPlayer", PhotonTargets.AllBuffered, freeId, player.ID, player.name);
        }
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        if(PhotonNetwork.isMasterClient){
        	// remove it locally so that we don't hand out incorrect ids
		    for(int i=0;i<3; i++){
		    	if(playerMapper[i] == player.ID){
		    		playerMapper[i] = -1;
		    		break;
		    	}
		    }
        	GetComponent<PhotonView>().RPC("RemovePlayer", PhotonTargets.AllBuffered, player.ID, player.name);
        }
    }

    [RPC]
	void RemovePlayer(int playerId, string playerName){
        for(int i=0;i<playerNames.Count; i++){
        	if(playerNames[i] == playerName){
        		playerNames.RemoveAt(i);
        		break;
        	}
        }
        // find in the player mapper, and remove 
        for(int i=0;i<3; i++){
        	if(playerMapper[i] == playerId){
        		playerMapper[i] = -1;
        		break;
        	}
        }
        playersMapped--;
	}
    [RPC]
    void AssignIdToPlayer(int p, int playerId, string playerName){
    	if(p<3){
    		playerMapper[p] = playerId;
    		playerNames.Add(playerName);
    		playersMapped++;
    	}
    }
}