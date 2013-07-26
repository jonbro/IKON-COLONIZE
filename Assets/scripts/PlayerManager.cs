using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour {
	public Unit u{
		get{
			return GetComponent<UnitBase>().u;
		}
	}
	public Unit core;
	Unit[] creeps;
	LineRenderManager lines;
	Color ourColor;
	int pid = -1;
    bool setup;
	public int xp = 0;
	DrawString strings;
	enum Modes {
		ATTACK,
		HEAL,
		DASH,
		TURTLE
	};
	Modes currentMode;
	List<UnitBase> targets;
	bool selectingMode;
	public bool respawning;
	int killCount = 0;
	float respawnRemain;

    void Start(){
        lines = GameObject.Find("LineRenderManager").GetComponent<LineRenderManager>();
		strings = GetComponent<UnitBase>().lines.GetComponent<DrawString>();
		currentMode = Modes.ATTACK;
		targets = new List<UnitBase>();
    }

	[RPC]
	public void Setup(){
		u.owner = -1;
		// check to see what players are open to instance... bail if it there are none available
		bool[] takenPlayers = new bool[3];
        foreach(PlayerManager p in GameObject.Find("HBGameController").GetComponentsInChildren<PlayerManager>()){
        	if(p.u.owner < 3 && p.u.owner >=0){
        		takenPlayers[p.u.owner] = true;
        	}
		}
		int _pid = -1;
		int _ourColor = -1;
		for(int i=0;i<3;i++){
			if(!takenPlayers[i]){
				_pid = i;
				break;
			}
		}
		if(_pid < 0){
			return;
		}

		Debug.Log("Adding player: "+_pid+" : "+GetComponent<PhotonView>().isMine);
        _ourColor = pid = _pid;
		GetComponent<UnitBase>().Setup(_pid, HullBreachGameController.globalColors[_ourColor]);
        ResetPosition();
        u.displaySize = 6f;
        GetComponent<UnitBase>().attackRadius = 12;
        setup = true;
        // 
        foreach(CoreC c in GameObject.Find("HBGameController").GetComponentsInChildren<CoreC>()){
			if(c.u.owner == _pid){
		        core = c.u;
		        break;
			}        	
        }
		u.attackPower = 20;
		u.attackCooldown = 1f;
		ourColor = HullBreachGameController.globalColors[_ourColor];
    }

    [RPC]
    public void Killed(){
    	killCount++;
    	respawning = true;
    	respawnRemain = killCount*2;
    	ResetPosition();
    	GetComponent<UnitBase>().enabled = false;
    }
	public void ResetPosition(){
        u.position = (Vector2.one*83).Rotate((pid*(360/3.0f))*Mathf.Deg2Rad);		
        u.SetHealth(100);
        u.alive = true;
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	    if (stream.isWriting)
	    {
	        // We own this player: send the others our data
	        stream.SendNext(u.position);
	        stream.SendNext(GetComponent<UnitBase>().attacking);
	    }
	    else
	    {
	        // Network player, receive data
	        this.u.position = (Vector2)stream.ReceiveNext();
	        this.GetComponent<UnitBase>().attacking = (bool)stream.ReceiveNext();
	    }
	}
	[RPC]
	void Respawn(){
		respawning = false;
    	GetComponent<UnitBase>().enabled = true;
	}
	// Update is called once per frame
	void Update () {
        if(!setup){
            return;
        }
        if(respawning){
        	respawnRemain -= Time.deltaTime;

			// draw all the various info on the screen
			transform.position = core.pos3+Vector3.left*-30;
			transform.localScale = new Vector3(10, 10, 10);
			strings.Text("wait: "+respawnRemain+" / ", transform, 0.2f, ourColor);
        	if(respawnRemain <= 0){
		        if(GetComponent<PhotonView>().isMine){
		        	GetComponent<PhotonView>().RPC("Respawn", PhotonTargets.AllBuffered);
		        }
        	}
        }
        if(GetComponent<PhotonView>().isMine){
			if(GetComponent<PhotonView>().isMine && LFInput.GetButtonDown("p1 flip")){
				selectingMode = true;
			}
			if(selectingMode){
				// check to see what mode we should switch into
				if(LFInput.GetAxis("p1 vertical") > 0){
					currentMode = Modes.ATTACK;
					selectingMode = false;
				}else if(LFInput.GetAxis("p1 vertical") < 0){
					currentMode = Modes.HEAL;
					selectingMode = false;
				}else if(LFInput.GetAxis("p1 horizontal") < 0){
					currentMode = Modes.DASH;
					selectingMode = false;
				}else if(LFInput.GetAxis("p1 horizontal") > 0){
					currentMode = Modes.TURTLE;
					selectingMode = false;
				}
				DrawOptions();
			}else if(currentMode == Modes.TURTLE){
				if(GetComponent<UnitBase>().attackCooldownCounter <= 0){
					GetComponent<PhotonView>().RPC("SetHealth", PhotonTargets.AllBuffered, u.health+15);
					GetComponent<UnitBase>().attackCooldownCounter = u.attackCooldown;
				}
			}else if(!respawning){
				Vector2 input = new Vector2(LFInput.GetAxis("p1 horizontal"), -LFInput.GetAxis("p1 vertical"));
				float speed = 40.0f;
				if(currentMode == Modes.DASH){
					speed += speed*0.5f;
				}
				u.position += input*(Time.deltaTime*speed);
				transform.position = u.pos3+Vector3.left*-10;
				transform.localScale = new Vector3(10, 10, 10);
				strings.Text(currentMode.ToString(), transform, 0.1f, ourColor);
			}        	
        }
	}
	void DrawOptions(){
		
		transform.position = u.pos3+Vector3.up*-20;
		transform.localScale = new Vector3(10, 10, 10);
		strings.Text(Modes.ATTACK.ToString(), transform, 0.1f, ourColor);

		transform.position = u.pos3+Vector3.up*20;
		transform.localScale = new Vector3(10, 10, 10);
		strings.Text(Modes.HEAL.ToString(), transform, 0.1f, ourColor);

		transform.position = u.pos3+Vector3.left*20;
		transform.localScale = new Vector3(10, 10, 10);
		strings.Text(Modes.DASH.ToString(), transform, 0.1f, ourColor);

		transform.position = u.pos3+Vector3.left*-20;
		transform.localScale = new Vector3(10, 10, 10);
		strings.Text(Modes.TURTLE.ToString(), transform, 0.1f, ourColor);

	}
	public void CheckNeighbors(UnitBase[] units){
		targets.Clear();
		foreach(UnitBase unit in units){
			if((currentMode == Modes.ATTACK && unit.u.owner != u.owner) || (currentMode == Modes.HEAL && unit.u != u && unit.u.owner == u.owner && !unit.GetComponent<CoreC>())){
				if(Vector2.Distance(unit.u.position, u.position) - unit.u.displaySize < GetComponent<UnitBase>().attackRadius){
					// we are within the attack radius!
					targets.Add(unit);
				}
			}
		}
		GetComponent<UnitBase>().attacking = false;
		if(targets.Count > 0){
			if(currentMode == Modes.HEAL && GetComponent<UnitBase>().attackCooldownCounter <= 0){

				GetComponent<UnitBase>().attackCooldownCounter = u.attackCooldown;
				GetComponent<PhotonView>().RPC("SetCooldown", PhotonTargets.AllBuffered, u.attackCooldown);
				targets[0].GetComponent<PhotonView>().RPC("SetHealth", PhotonTargets.AllBuffered, targets[0].u.health + 20);
				lines.AddLine(u.pos3.Variation(1), targets[0].u.pos3.Variation(1), ourColor);
				return;
			}
			if(currentMode == Modes.ATTACK){
				GetComponent<UnitBase>().attackCooldownCounter = u.attackCooldown;
				GetComponent<UnitBase>().attacking = true;
				GetComponent<UnitBase>().AttackTarget(targets[0]);
			}
		}
	}

}
