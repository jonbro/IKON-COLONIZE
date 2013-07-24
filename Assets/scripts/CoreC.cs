using UnityEngine;
using System.Collections;

public class CoreC : MonoBehaviour {
	public Unit u{
		get{
			return GetComponent<UnitBase>().u;
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	public void Setup(int _pid, Color _ourColor){
		int pid = _pid;
        GetComponent<UnitBase>().Setup(_pid, _ourColor);
		u.position = (Vector2.one*130).Rotate((pid*(360/3.0f))*Mathf.Deg2Rad);
		u.displaySize = 25;
		GetComponent<UnitBase>().attackRadius = 27;
		u.SetHealth(400);
	}	
	// Update is called once per frame
	void Update () {
	
	}
}
