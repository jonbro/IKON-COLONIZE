using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChatItem{
	public string text;
	public string player;
	public double time;
}

public class InitGame : MonoBehaviour {
	PhotonView photonView;
	public static int[] playerMapper;
	List<string> playerNames;

	int currentPlayer = 0;
	int playersMapped = 0;
	bool started;
	List<ChatItem> chatLog;
	string currentChat;
	// Use this for initialization
	void Start(){
		playerNames = new List<string>();
		chatLog = new List<ChatItem>();
		for(int i=0;i<25;i++){
			chatLog.Add(new ChatItem());
		}
		currentChat = "";
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
            foreach(ChatItem chatLine in chatLog){
            	VectorGui.Label(chatLine.player + " : " +chatLine.text);
            }
            currentChat = VectorGui.TextInput(currentChat);
            if(currentChat.Length > 0){
	            char lastChar = currentChat[currentChat.Length-1];
	            if(lastChar == "\n"[0] || lastChar == "\r"[0]){
	            	GetComponent<PhotonView>().RPC("AddChat", PhotonTargets.All, currentChat, PhotonNetwork.player.name, PhotonNetwork.time);
	            	currentChat = "";
	            }            	
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
        	// dump my chatlog so that the new player gets it
            foreach(ChatItem chatLine in chatLog){
            	GetComponent<PhotonView>().RPC("AddChat", PhotonTargets.All, chatLine.text, chatLine.player, chatLine.time);
            }
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
    void AddChat(string text, string player, double time){
    	// check to see if this is duped in the logs
    	foreach(ChatItem c in chatLog){
    		if(c.time == time && player == c.player){
    			return;
    		}
    	}
    	chatLog.RemoveAt(0);
    	ChatItem chat = new ChatItem();
    	chat.text = text;
    	chat.player = player;
    	chat.time = time;
    	chatLog.Add(chat);
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