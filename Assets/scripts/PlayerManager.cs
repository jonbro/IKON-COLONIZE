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
	public int pid = -1;
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
	bool selectingMode, ready;
	public bool respawning;
	int killCount = 0;
	float respawnRemain;
	
	float dashChargeTime = 15;
	float dashAmount = 4.0f;
	float dashTime, dashRemain;
	bool dashing;

    void Start(){
        lines = GameObject.Find("LineRenderManager").GetComponent<LineRenderManager>();
		strings = GetComponent<UnitBase>().lines.GetComponent<DrawString>();
		currentMode = Modes.ATTACK;
		targets = new List<UnitBase>();
    }

	[RPC]
	public void Setup(int _pid){
        int _ourColor = pid = _pid;
		GetComponent<UnitBase>().Setup(_pid, HullBreachGameController.globalColors[_ourColor]);
        u.displaySize = 6f;
        GetComponent<UnitBase>().attackRadius = 12;
		u.attackPower = 20;
		u.attackCooldown = 1f;
		ourColor = HullBreachGameController.globalColors[_ourColor];
        ResetPosition();
        setup = true;
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

	[RPC]
	void SetMode(int newMode){
		currentMode = (Modes)newMode;
	}
	// Update is called once per frame
	void Update () {
        if(!setup){
            return;
        }
	    foreach(CoreC c in GameObject.Find("HBGameController").GetComponentsInChildren<CoreC>()){
			if(c.u.owner == pid){
		        core = c.u;
		        break;
			}        	
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
        }else{
        	if(dashTime < dashChargeTime){
        		dashTime += Time.deltaTime;
        	}else{
        		dashTime = dashChargeTime;
        	}
        	if(!GetComponent<PhotonView>().isMine || !selectingMode){    		
				transform.position = u.pos3+Vector3.left*-10;
				transform.localScale = new Vector3(10, 10, 10);
				strings.Text(currentMode.ToString(), transform, 0.1f, ourColor);
        	}
	        if(GetComponent<PhotonView>().isMine){
	        	if(dashing){
					VectorGui.Label("DASHING", 0.1f, ourColor);
	        	}else if(dashTime<dashChargeTime){
	        		VectorGui.Label("Dash charge", 0.1f, ourColor);
	        	}else{
	        		VectorGui.Label("Dash charge READY", 0.1f, ourColor);
	        	}
	        	if(dashing){
			        VectorGui.ProgressBar(dashRemain/dashAmount, Color.Lerp(ourColor, Color.white, (Mathf.Sin(Time.time*6.0f)+1.0f)*0.5f));
	        	}else{
			        VectorGui.ProgressBar(dashTime/dashChargeTime, ourColor);
	        	}
        	}
        }
        if(GetComponent<PhotonView>().isMine){
			if(GetComponent<PhotonView>().isMine){
				if(LFInput.GetButtonDown("p1 flip")){
					GetComponent<PhotonView>().RPC("SetMode", PhotonTargets.AllBuffered, (int)Modes.ATTACK);
				}else if(LFInput.GetButtonDown("p1 fire")){
					GetComponent<PhotonView>().RPC("SetMode", PhotonTargets.AllBuffered, (int)Modes.HEAL);
				}else if(LFInput.GetButtonDown("p1 dash") && !dashing && dashTime == dashChargeTime){
					dashing = true;
					dashRemain = dashAmount;
				}
			}

			if(currentMode == Modes.TURTLE){
				if(GetComponent<UnitBase>().attackCooldownCounter <= 0){
					GetComponent<PhotonView>().RPC("SetHealth", PhotonTargets.AllBuffered, u.health+15);
					GetComponent<UnitBase>().attackCooldownCounter = u.attackCooldown;
				}
			}else if(!respawning){
				Vector2 input = new Vector2(LFInput.GetAxis("p1 horizontal"), -LFInput.GetAxis("p1 vertical"));
				float speed = 40.0f;
				if(dashing){
					speed += speed*1.5f;
					dashRemain -= Time.deltaTime;
					if(dashRemain <= 0){
						dashing = false;
					}
				}
				u.position += input*(Time.deltaTime*speed);
				// draw the target
				BuildTargetList();
				if(currentMode == Modes.ATTACK){
					foreach(UnitBase unit in targets){
						if(unit.u.owner != u.owner){
							lines.AddLine(u.pos3, unit.u.pos3, Color.white);
							break;
						}
					}
				}else if(currentMode == Modes.HEAL){
					foreach(UnitBase unit in targets){
						if(unit.u.owner == u.owner){
							lines.AddLine(u.pos3, unit.u.pos3, Color.white);
							break;
						}
					}
				}
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
	void BuildTargetList(){
		targets.Clear();
		UnitBase[] units = transform.parent.GetComponentsInChildren<UnitBase>();
		foreach(UnitBase unit in units){
			if(
				(currentMode == Modes.ATTACK && unit.u.owner != u.owner) ||
				(currentMode == Modes.HEAL && unit.u != u && unit.u.owner == u.owner && !unit.GetComponent<CoreC>()))
			{
				float dist = Vector2.Distance(unit.u.position, u.position);
				if(dist - unit.u.displaySize < GetComponent<UnitBase>().attackRadius){
					// we are within the attack radius!
					unit.temporaryDistance = dist;
					targets.Add(unit);
				}
			}
		}
		targets.Sort(delegate(UnitBase p1, UnitBase p2)
		    {
		        return (p1.temporaryDistance < p2.temporaryDistance)?-1:1;
		    }
		);
	}
	public void CheckNeighbors(UnitBase[] units){
		BuildTargetList();
		GetComponent<UnitBase>().attacking = false;
		if(targets.Count > 0){
			if(currentMode == Modes.HEAL && GetComponent<UnitBase>().attackCooldownCounter <= 0){
				Debug.Log("Healing");
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
