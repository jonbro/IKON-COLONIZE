using UnityEngine;
using System.Collections;

public class PregameChat : MonoBehaviour {
	void Start (){
	}
    void Update () {
        VectorGui.Label("IKON_COLONIZE: PREGAME", 0.3f);
        VectorGui.Label(PhotonNetwork.room.name, 0.2f);
    	VectorGui.Label("Connected Players", 0.1f);

        foreach (PhotonPlayer player in PhotonNetwork.playerList){
        	VectorGui.Label(player.name, 0.1f);
        }
   }
}