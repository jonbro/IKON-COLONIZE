using UnityEngine;
using System.Collections;

public class LobbyManager : MonoBehaviour {
	// Use this for initialization
    // private string roomName = "myRoom";
    private Vector2 scrollPos = Vector2.zero;
    private bool connectFailed = false;
    public TextAsset naval;
    bool chooseRegion;
	void Start () {
        if (!PhotonNetwork.connected)
        {
            // PhotonNetwork.ConnectUsingSettings("1.0");
            ServerSettings ss = new ServerSettings();
            ss.UseCloud("c2bd3559-19e2-4734-88c9-fdd1d789be53", (int)CloudServerRegion.US);
            PhotonNetwork.Connect(ss.ServerAddress, ss.ServerPort, ss.AppID, "1.0");
            chooseRegion = true;
	    }
	    // generate a name for this player, if none is assigned yet
        if (string.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
        }

	}
	void Update(){
        if(chooseRegion){
            ConnectionUpdate();
        }else{
            VectorGui.Label("Select Region:");
            foreach(CloudServerRegion region in CloudServerRegion.GetValues(typeof(CloudServerRegion))){
                if (VectorGui.Button(region.ToString()))
                {
                    ServerSettings ss = new ServerSettings();
                    ss.UseCloud("c2bd3559-19e2-4734-88c9-fdd1d789be53", (int)region);
                    PhotonNetwork.Connect(ss.ServerAddress, ss.ServerPort, ss.AppID, "1.0");
                    chooseRegion = true;
                    break;
                }
            }
        }
    }
	// Update is called once per frame
	void ConnectionUpdate () {
		VectorGui.Label("HULL BREACH: LOBBY", 0.3f);		
        if (!PhotonNetwork.connected)
        {
            if (PhotonNetwork.connectionState == ConnectionState.Connecting)
            {
            	VectorGui.Label("Connecting " + PhotonNetwork.PhotonServerSettings.ServerAddress + " / " +Time.time.ToString());
            }
            else
            {
            	VectorGui.Label("Not connected. Check console output.");
            }
            if (this.connectFailed)
            {
            	VectorGui.Label("Connection failed. Connect to lobby server failed.");
            }
            return;
        }
    	VectorGui.Label(PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.");
        
        if (VectorGui.Button("Create Room"))
        {
			string[] navalString = naval.text.Split("\n"[0]);
            PhotonNetwork.CreateRoom(navalString[Random.Range(0, navalString.Length)] + " " +navalString[Random.Range(0, navalString.Length)], true, true, 10);
        }

        if (PhotonNetwork.GetRoomList().Length == 0)
        {
            VectorGui.Label("Currently no games are available.");
            VectorGui.Label("Rooms will be listed here, when they become available.");
        }
        else
        {
            VectorGui.Label(PhotonNetwork.GetRoomList() + " currently available. Join either:");

            // Room listing: simply call GetRoomList: no need to fetch/poll whatever!
            // this.scrollPos = VectorGui.BeginScrollView(this.scrollPos);
            foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
            {
                // VectorGui.BeginHorizontal();
                VectorGui.Label(roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers);
                if (VectorGui.Button("Join"))
                {
                    PhotonNetwork.JoinRoom(roomInfo.name);
                }
                // VectorGui.EndHorizontal();
            }

            // VectorGui.EndScrollView();
        }


	}
    private void OnJoinedRoom()
    {
        this.StartCoroutine(this.MoveToGameScene());
    }

    private void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        this.StartCoroutine(this.MoveToGameScene());
    }

    private void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    private void OnFailedToConnectToPhoton(object parameters)
    {
        this.connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters);
    }

    private IEnumerator MoveToGameScene()
    {
        PhotonNetwork.isMessageQueueRunning = false;
        while (PhotonNetwork.room == null)
        {
            yield return 0;
        }
        Application.LoadLevel("Game");
    }
}
