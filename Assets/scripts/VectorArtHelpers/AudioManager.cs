using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
	enum Fade {In, Out};
	public AudioClip[] blocker;
	public AudioClip scoringOn;
	// public MusicBedController musicBeds;
	private List<GameObject> musicBedsForLevel;
	private GameObject[] audioContainers;
	private GameObject[] blockerContainers;
	private int currentContainer = 0;
	private int numContainers = 25;
	// Use this for initialization
	public float bedFadeTime = 2.0f;
	private static AudioManager instance;
	private bool musicMuted;

	public static AudioManager Instance {
		get
		{
			if(instance == null){
				instance = ((GameObject)Instantiate(Resources.Load("AudioManager"))).GetComponent<AudioManager>();
			}
			return instance;
		}
	}

	void Awake () {
		DontDestroyOnLoad(gameObject);
		musicMuted = false;
		audioContainers = new GameObject[numContainers];
		blockerContainers = new GameObject[blocker.Length];
		for (int i = 0; i < numContainers; i++) {
			GameObject g = audioContainers[i] = new GameObject();
			g.transform.position = transform.position;
			g.transform.parent = transform;
	        g.AddComponent<AudioSource>();
		}
		for(int i = 0;i<blocker.Length;i++){
			GameObject g = blockerContainers[i] = new GameObject();
			g.transform.position = transform.position;
			g.transform.parent = transform;
	        AudioSource asource = g.AddComponent<AudioSource>();
	        asource.clip = blocker[i];
	        asource.loop = true;
		}
		musicBedsForLevel = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		if(PlayerPrefs.GetInt("Music") == 0 && !musicMuted){
			// mute all of the currently playing music beds
			for(int i=0;i<musicBedsForLevel.Count;i++){
				musicBedsForLevel[i].audio.volume = 0;
			}
			musicMuted = true;
		}
		if(PlayerPrefs.GetInt("Music") == 1 && musicMuted){
			// mute all of the currently playing music beds
			for(int i=0;i<musicBedsForLevel.Count;i++){
				musicBedsForLevel[i].audio.volume = 1;
			}
			musicMuted = false;
		}
	}

	public void loadSoundsForLevel(string levelName){
		// delete all the old level sounds
		// eventually should fade these out
		GetComponent<AudioLowPassFilter>().cutoffFrequency = 22000;
		GetComponent<AudioLowPassFilter>().enabled = false;
		for(int i=0;i<musicBedsForLevel.Count;i++){
			StartCoroutine(FadeAudio(bedFadeTime, Fade.Out, musicBedsForLevel[i], true));
		}
		musicBedsForLevel.Clear();
		// foreach(MusicBedGroup mbg in instance.musicBeds.MusicBeds){
		// 	if(mbg.levelName == levelName){
		// 		foreach(AudioClip c in mbg.loops){
		// 			GameObject g = new GameObject("music bed");
		// 			g.transform.position = transform.position;
		// 			g.transform.parent = transform;
		// 	        g.AddComponent<AudioSource>();
		// 	        g.GetComponent<AudioSource>().clip = c;
		// 	        g.GetComponent<AudioSource>().loop = true;
		// 	        g.GetComponent<AudioSource>().Play();
		// 	        if(musicMuted){
		// 	        	g.GetComponent<AudioSource>().volume = 0;
		// 	        }
		// 	        musicBedsForLevel.Add(g);
		// 		}
		// 	}
		// }
	}
	public void PauseMusic(){
		// lower the lowpass filter
		GetComponent<AudioLowPassFilter>().enabled = true;
		GetComponent<AudioLowPassFilter>().cutoffFrequency = 400;
		/*
		for(int i=0;i<musicBedsForLevel.Count;i++){
			musicBedsForLevel[i].audio.Pause();
		}
		*/
	}
	public void PlayMusic(){
		GetComponent<AudioLowPassFilter>().enabled = false;
		GetComponent<AudioLowPassFilter>().cutoffFrequency = 22000;
		/*
		for(int i=0;i<musicBedsForLevel.Count;i++){
			musicBedsForLevel[i].audio.Play();
		}
		*/
	}
	IEnumerator FadeAudio (float timer, Fade fadeType, GameObject go, bool shouldDestroy) {
		float start = go.audio.volume;
		float end = fadeType == Fade.In? 1.0F : 0.0F;
		if(musicMuted){
			end = 0;
		}
		float i = 0.0F;
		float step = 1.0F/timer;
		while (i <= 1.0F) {
			i += step * Time.deltaTime;
			go.audio.volume = Mathf.Lerp(start, end, i);
			yield return new WaitForSeconds(step * Time.deltaTime);
		}
		if(shouldDestroy){
			Destroy(go);
		}
	}
	public void PlayBlocker(int blockerCount){
		blockerContainers[blockerCount].audio.Play();
		// Play(blocker[blockerCount], transform, 1, 1);
	}
	public void StopBlocker(int blockerCount){
		blockerContainers[blockerCount].audio.Stop();
		// blockerContainers[blockerCount].audio.Rewind();
		// Play(blocker[blockerCount], transform, 1, 1);
	}
	public void ScoringOn(){
		Play(scoringOn, transform, 1, 1);
	}
	public void levelEnd(){

	}

    public AudioSource Play(AudioClip clip, Transform emitter, float volume, float pitch)
    {
		GameObject g = audioContainers[currentContainer];
		currentContainer = (currentContainer+1)%numContainers;
        AudioSource source = g.audio;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
	    source.Play ();
        return source;
    }

}