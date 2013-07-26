using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HullBreachGameController : MonoBehaviour {
	PlayerManager[] players;
	Tower[] towers;
	public CoreC[] cores;
	List<Creep> creeps;
	
	public GameObject corePrefab;
	public Color[] pColors;
	public static Color[] globalColors;
	float creepSpawnRate = 18.0f;
	float nextCreepWave;
	bool setup;
	bool gameOver;
	int winner;
	void Awake(){
		gameObject.name = "HBGameController";		
		globalColors = new Color[pColors.Length];
		for(int i=0;i<pColors.Length;i++){
			globalColors[i] = pColors[i];
		}
	}
	void OnPhotonInstantiate(PhotonMessageInfo info){
		gameObject.name = "HBGameController";
		// instantiate a player
	}
	void Start () {
		// copy the colors over
		creeps = new List<Creep>();
		cores = new CoreC[3];
		players = new PlayerManager[3];
		towers = new Tower[3*2];

		if(PhotonNetwork.isMasterClient){
			Debug.Log("STARTUP!!");
			for(int i=0;i<3;i++){
				cores[i] = ((GameObject)PhotonNetwork.InstantiateSceneObject("CoreC", Vector3.zero, Quaternion.identity, 0, new object[] { i, i })).GetComponent<CoreC>();
			}
			// go through and add turrets for each of the cores
			for(int i=0;i<3;i++){
				for(int j=0;j<2;j++){
					Vector2 dir = cores[(i+j+1)%3].u.position-cores[i].u.position;
					Vector2 turretPos = cores[i].u.position + dir.normalized * (dir.magnitude*(1/3.0f));
					Tower t = ((GameObject)PhotonNetwork.InstantiateSceneObject("Tower", Vector3.zero, Quaternion.identity, 0, new object[] { i, i, turretPos })).GetComponent<Tower>();					
					towers[i] = t;
				}
			}
		}

		PlayerManager p = ((GameObject)PhotonNetwork.Instantiate("PlayerManager", Vector3.zero, Quaternion.identity, 0)).GetComponent<PlayerManager>();
		p.GetComponent<PhotonView>().RPC("Setup", PhotonTargets.AllBuffered);
		p.GetComponent<UnitBase>().player = p;

		setup = true;		
	}
	void SpawnCreeps(){
		nextCreepWave = creepSpawnRate;
		// boost out some creeps from each players core, set their targets towards the other players cores
		for(int i=0;i<3;i++){
			for(int j=0;j<2;j++){
				// Creep c = new Creep();
				int spawnCount = Random.Range(2, 5);
				for(int k=0;k<spawnCount;k++){
					Creep c = ((GameObject)PhotonNetwork.InstantiateSceneObject("Creep", Vector3.zero, Quaternion.identity, 0, new object[] { i, i, (i+j+1)%3})).GetComponent<Creep>();
					c.GetComponent<UnitBase>().player = players[i];
					creeps.Add(c);
				}
			}
		}		

	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	void Update(){
		if(!setup) return;
		if(PhotonNetwork.isMasterClient){
			nextCreepWave -= Time.deltaTime;
			if(nextCreepWave<=0) SpawnCreeps();
			UnitBase[] units = GetComponentsInChildren<UnitBase>();
			for(int i=0;i<units.Length;i++){
				units[i].CheckNeighbors(units);
			}
		}

		// game over
		if(gameOver){
			VectorGui.Label("GAME OVER.", 0.3f);
			VectorGui.Label("WINNER:", 0.3f);
			VectorGui.Label("Player "+winner, 0.3f, pColors[winner]);
		}
	}
	[RPC]
	void SetWinner(int _winner){
		winner = _winner;
		gameOver = true;
	}
	void FixedUpdate(){
		UnitBase[] units = GetComponentsInChildren<UnitBase>();
		// update all of the existing creeps
		Creep[] cps = GetComponentsInChildren<Creep>();
		for(int i=0;i<cps.Length;i++){
			cps[i].ApplySteering(units);
		}
		if(!PhotonNetwork.isMasterClient){
			return;
		}
		// remove all of the dead creeps
		for(int i=units.Length-1;i>=0;i--){
			if(!units[i].u.alive){
				// remove from the unit list as well, and destroy the gameobject
				if(units[i].GetComponent<Creep>()){
	 				creeps.Remove(units[i].GetComponent<Creep>());
				}
				units[i].GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All);
				if(units[i].GetComponent<PlayerManager>()){
					// reset the player back to the base
					units[i].GetComponent<PhotonView>().RPC("Killed", PhotonTargets.AllBuffered);
				}else if(units[i].GetComponent<CoreC>() && !gameOver){
					// check to see which player has the highest health on their core

					float topHealth = 0;
					int tempWinner = 0;
					for(int w=0;w<3;w++){
						if(topHealth<cores[w].u.health){
							topHealth = cores[w].u.health;
							tempWinner = w;
						}
					}
					GetComponent<PhotonView>().RPC("SetWinner", PhotonTargets.AllBuffered, tempWinner);
				}else{
					PhotonNetwork.RemoveRPCs(units[i].GetComponent<PhotonView>());
					PhotonNetwork.Destroy(units[i].gameObject);
				}
			}
		}
	}
}
