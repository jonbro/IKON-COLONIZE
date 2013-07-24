using UnityEngine;
using System.Collections;

// handles what phase of the menu we are currently in
public class MenuManager : MonoBehaviour {
    public GameObject lobby;
    public GameObject pregameChat;

    /// <summary>
    /// Enable one GO and remove the other
    /// </summary>
    public void Awake()
    {
        if (PhotonNetwork.room != null)
        {
            Destroy(this.lobby);
            // this.pregameChat.active = true;
        }
        else
        {
            Destroy(this.pregameChat);
            // this.lobby.active = true;
        }
        // now this script is not needed anymore. destroy it and it's gameobject
        Destroy(this.gameObject);
    }

}