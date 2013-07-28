using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {
	enum Fade {In, Out};
	public AudioClip[] Deaths1;
	public AudioClip[] Deaths2;
	public AudioClip[] Deaths3;
	public MusicBedController musicBeds;
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
		for (int i = 0; i < numContainers; i++) {
			GameObject g = audioContainers[i] = new GameObject();
			g.transform.position = transform.position;
			g.transform.parent = transform;
	        g.AddComponent<AudioSource>();
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
		for(int i=0;i<musicBedsForLevel.Count;i++){
			StartCoroutine(FadeAudio(bedFadeTime, Fade.Out, musicBedsForLevel[i], true));
		}
		musicBedsForLevel.Clear();
		foreach(MusicBedGroup mbg in instance.musicBeds.MusicBeds){
			if(mbg.levelName == levelName){
				foreach(AudioClip c in mbg.loops){
					GameObject g = new GameObject("music bed");
					g.transform.position = transform.position;
					g.transform.parent = transform;
			        g.AddComponent<AudioSource>();
			        g.GetComponent<AudioSource>().clip = c;
			        g.GetComponent<AudioSource>().loop = true;
			        g.GetComponent<AudioSource>().Play();
			        if(musicMuted){
			        	g.GetComponent<AudioSource>().volume = 0;
			        }
			        musicBedsForLevel.Add(g);
				}
			}
		}
	}
	public void PauseMusic(){
		for(int i=0;i<musicBedsForLevel.Count;i++){
			musicBedsForLevel[i].audio.Pause();
		}
		
	}
	public void PlayMusic(){
		for(int i=0;i<musicBedsForLevel.Count;i++){
			musicBedsForLevel[i].audio.Play();
		}
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
	public void Deaths(int count){
		if(count == 1){
			Play(Deaths1[Random.Range(0, Deaths1.Length)], transform, 1.0f, 1.0f);
		}
		if(count == 2){
			Play(Deaths2[Random.Range(0, Deaths2.Length)]);
		}
		if(count == 3){
			Play(Deaths3[Random.Range(0, Deaths3.Length)]);
		}
	}
	public void levelEnd(){

	}
	public AudioSource Play(AudioClip clip){
		return Play(clip, transform, 1.0f, 1.0f);
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