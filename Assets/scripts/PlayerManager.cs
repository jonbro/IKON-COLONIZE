using UnityEngine;
using System.Collections;

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
	int pid = 0;
    bool setup;

    void Start(){
        lines = GameObject.Find("LineRenderManager").GetComponent<LineRenderManager>();
    }
    public void Setup(int _pid, Color _ourColor, Unit ourCore){
        pid = _pid;
        GetComponent<UnitBase>().Setup(_pid, _ourColor);
        ResetPosition();
        u.displaySize = 6f;
        GetComponent<UnitBase>().attackRadius = 12;
        setup = true;
        core = ourCore;
		u.attackPower = 20;
		u.attackCooldown = 1f;
    }
	public void ResetPosition(){
        u.position = (Vector2.one*83).Rotate((pid*(360/3.0f))*Mathf.Deg2Rad);		
        u.SetHealth(100);
        u.alive = true;
	}
	// Update is called once per frame
	void Update () {
        if(!setup)
            return;		
		// Camera.main.transform.position = new Vector3(hero.position.x, hero.position.y, Camera.main.transform.position.z);
		u.position += (new Vector2(LFInput.GetAxis("p"+(pid+1)+" horizontal"), -LFInput.GetAxis("p"+(pid+1)+" vertical")))*(Time.deltaTime*40.0f);
		Debug.Log(pid+" : health : "+u.health);
	}
}
