using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HullBreachGameController : MonoBehaviour {
	PlayerManager[] players;
	Tower[] towers;
	CoreC[] cores;
	List<Creep> creeps;
	
	List<UnitBase> units;
	public GameObject corePrefab;
	public Color[] pColors;
	float creepSpawnRate = 15.0f;
	float nextCreepWave;
	bool setup;
	bool gameOver;
	int winner;
	void Awake(){
		
	}
	void Start () {
		creeps = new List<Creep>();
		units = new List<UnitBase>();
		cores = new CoreC[3];
		players = new PlayerManager[3];
		towers = new Tower[3*2];
		for(int i=0;i<3;i++){
			// setup cores
			cores[i] = ((GameObject)Instantiate(corePrefab, Vector3.zero, Quaternion.identity)).GetComponent<CoreC>();
			cores[i].Setup(i, pColors[i]);
			units.Add(cores[i].GetComponent<UnitBase>());

			// setup players
			players[i] = ((GameObject)Instantiate(Resources.Load("PlayerManager"), Vector3.zero, Quaternion.identity)).GetComponent<PlayerManager>();
			players[i].Setup(i, pColors[i], cores[i].u);

			players[i].GetComponent<UnitBase>().player = players[i];
			cores[i].GetComponent<UnitBase>().player = players[i];
			
			units.Add(players[i].GetComponent<UnitBase>());

		}
		// go through and add turrets for each of the cores
		for(int i=0;i<3;i++){
			for(int j=0;j<2;j++){
				Tower t = ((GameObject)Instantiate(Resources.Load("Tower"), Vector3.zero, Quaternion.identity)).GetComponent<Tower>();
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
				for(int k=0;k<4;k++){
					Creep c = ((GameObject)Instantiate(Resources.Load("Creep"), Vector3.zero, Quaternion.identity)).GetComponent<Creep>();
					c.Setup(i, pColors[i], players[i].core, players[(i+j+1)%3].core);
					c.GetComponent<UnitBase>().player = players[i];
					creeps.Add(c);
					units.Add(c.GetComponent<UnitBase>());
				}
			}
		}		

	}
	void Update(){
		if(!setup) return;
		nextCreepWave -= Time.deltaTime;
		if(nextCreepWave<=0) SpawnCreeps();
		for(int i=0;i<units.Count;i++){
			units[i].CheckNeighbors(units);
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
		// remove all of the dead creeps
		for(int i=units.Count-1;i>=0;i--){
			if(!units[i].u.alive){
				// remove from the unit list as well, and destroy the gameobject
				if(units[i].GetComponent<Creep>()){
	 				creeps.Remove(units[i].GetComponent<Creep>());
				}
				units[i].Explode();
				if(units[i].GetComponent<PlayerManager>()){
					// reset the player back to the base
					units[i].GetComponent<PlayerManager>().ResetPosition();
				}else if(units[i].GetComponent<CoreC>()){
					// check to see which player has the highest health on their core
					int winner = 0;
					float topHealth = cores[0].u.health;
					for(int w=0;w<3;w++){
						if(topHealth<cores[w].u.health){
							winner = w;
						}
					}
					gameOver = true;
				}else{
					Destroy(units[i].gameObject);
					units.Remove(units[i]);					
				}
			}
		}
	}
}
