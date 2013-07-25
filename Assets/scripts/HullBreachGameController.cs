using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HullBreachGameController : MonoBehaviour {
	PlayerManager[] players;
	Tower[] towers;
	public CoreC[] cores;
	List<Creep> creeps;
	
	List<UnitBase> units;
	public GameObject corePrefab;
	public Color[] pColors;
	public static Color[] globalColors;
	float creepSpawnRate = 18.0f;
	float nextCreepWave;
	bool setup;
	bool gameOver;
	int winner;
	void Awake(){
		
	}
	void Start () {
		// copy the colors over
		globalColors = new Color[pColors.Length];
		for(int i=0;i<pColors.Length;i++){
			globalColors[i] = pColors[i];
		}
		gameObject.name = "HBGameController";
		creeps = new List<Creep>();
		units = new List<UnitBase>();
		cores = new CoreC[3];
		players = new PlayerManager[3];
		towers = new Tower[3*2];
		for(int i=0;i<3;i++){
			// setup cores
			cores[i] = ((GameObject)PhotonNetwork.Instantiate("CoreC", Vector3.zero, Quaternion.identity, 0)).GetComponent<CoreC>();
			// cores[i].GetComponent<PhotonView>().RPC("Setup", PhotonTargets.AllBuffered, i, pColors[i], 0);
			cores[i].Setup(i, pColors[i]);
			units.Add(cores[i].GetComponent<UnitBase>());

			// setup players
			players[i] = ((GameObject)PhotonNetwork.Instantiate("PlayerManager", Vector3.zero, Quaternion.identity, 0)).GetComponent<PlayerManager>();
			players[i].Setup(i, pColors[i]);
			// players[i].GetComponent<PhotonView>().RPC("Setup", PhotonTargets.AllBuffered, i, pColors[i], 0);

			players[i].GetComponent<UnitBase>().player = players[i];
			cores[i].GetComponent<UnitBase>().player = players[i];
			
			units.Add(players[i].GetComponent<UnitBase>());

		}
		// go through and add turrets for each of the cores
		for(int i=0;i<3;i++){
			for(int j=0;j<2;j++){
				Tower t = ((GameObject)PhotonNetwork.Instantiate("Tower", Vector3.zero, Quaternion.identity, 0)).GetComponent<Tower>();
				t.Setup(i, pColors[i], players[i].core, players[(i+j+1)%3].core);				
				towers[i] = t;
				t.GetComponent<UnitBase>().player = players[i];
				units.Add(t.GetComponent<UnitBase>());
			}
		}
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
					Creep c = ((GameObject)PhotonNetwork.InstantiateSceneObject("Creep", Vector3.zero, Quaternion.identity, 0, null)).GetComponent<Creep>();
					c.GetComponent<PhotonView>().RPC("Setup", PhotonTargets.AllBuffered, i, i, (i+j+1)%3);
					c.GetComponent<UnitBase>().player = players[i];
					creeps.Add(c);
					units.Add(c.GetComponent<UnitBase>());
				}
			}
		}		

	}
	void Update(){
		if(!setup) return;
		if(PhotonNetwork.isMasterClient){
			nextCreepWave -= Time.deltaTime;
			if(nextCreepWave<=0) SpawnCreeps();
			for(int i=0;i<units.Count;i++){
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
	void FixedUpdate(){
		// update all of the existing creeps
		for(int i=0;i<creeps.Count;i++){
			creeps[i].ApplySteering(units);
		}
		if(!PhotonNetwork.isMasterClient){
			return;
		}
		// remove all of the dead creeps
		for(int i=units.Count-1;i>=0;i--){
			if(!units[i].u.alive){
				// remove from the unit list as well, and destroy the gameobject
				if(units[i].GetComponent<Creep>()){
	 				creeps.Remove(units[i].GetComponent<Creep>());
				}
				units[i].GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All);
				if(units[i].GetComponent<PlayerManager>()){
					// reset the player back to the base
					units[i].GetComponent<PlayerManager>().Killed();

				}else if(units[i].GetComponent<CoreC>() && !gameOver){
					// check to see which player has the highest health on their core
					float topHealth = 0;
					for(int w=0;w<3;w++){
						Debug.Log("TOP CORE: "+winner+" : health : "+cores[w].u.health);
						if(topHealth<cores[w].u.health){
							topHealth = cores[w].u.health;
							winner = w;
						}
					}
					Debug.Log("TOP CORE: "+winner+" : health : "+cores[winner].u.health);
					gameOver = true;
				}else{
					PhotonNetwork.Destroy(units[i].gameObject);
					units.Remove(units[i]);					
				}
			}
		}
	}
}
