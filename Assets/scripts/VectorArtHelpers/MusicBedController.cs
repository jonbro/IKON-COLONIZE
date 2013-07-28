using UnityEngine;
using System.Collections;

[System.Serializable]
public class MusicBedGroup{
	public AudioClip[] loops;
	public string levelName;
}

public class MusicBedController : MonoBehaviour {

	public MusicBedGroup[] MusicBeds;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
