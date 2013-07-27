using UnityEngine;
using System.Collections;

public class LineRenderManager : MonoBehaviour {
	int maxLines = 10000;
	public float mag = 100;
	private Mesh mesh;
	
	Vector3[] verts;
	Vector2[] uv;
	int[] indices;
	Color32[] cs;
	int currentLine = 0;
	public bool exploding;
	// Use this for initialization
	void Awake () {
		if(mesh == null){
			GetComponent<MeshFilter>().mesh = mesh = new Mesh();
			mesh.hideFlags = HideFlags.HideAndDontSave;
		}
        mesh.Clear();
        // instantiate all the bits that we are going to need to fill out the line mesh
        verts = new Vector3[maxLines*2];
        uv = new Vector2[maxLines*2];
        cs = new Color32[maxLines*2];
        // blank everything so it doesn't go out of bounds
        for(int i=0;i<maxLines*2;i++){
        	verts[i] = Vector3.zero;
        }
        indices = new int[maxLines*2];
        // SetupMesh();
        mesh.vertices = verts;

        mesh.SetIndices(indices, MeshTopology.Lines, 0);
	}
	
	// Update is called once per frame
	void Update () {
		ClearLines();
		// SetupMesh();
	}
	public void AddCircle(Vector3 center, float mag, Color c, float angleOffset, int numPoints = 40){
		// int numPoints = 40;		
		for(int i = 0;i<numPoints;i++){
			float sAngle = (i/(float)(numPoints)*360.0f+angleOffset)*Mathf.Deg2Rad;
			float eAngle = ((i+1f)/(float)(numPoints)*360.0f+angleOffset)*Mathf.Deg2Rad;
			AddLine(new Vector3(Mathf.Cos(sAngle)*mag+center.x, Mathf.Sin(sAngle)*mag+center.y, center.z),
				new Vector3(Mathf.Cos(eAngle)*mag+center.x, Mathf.Sin(eAngle)*mag+center.y, center.z), c);
		}
	}
	public void AddCircle(Vector3 center, float mag, Color c, int numPoints = 40){
		// int numPoints = 40;		
		for(int i = 0;i<numPoints;i++){
			AddLine(new Vector3(Mathf.Cos(i/(float)(numPoints)*360.0f*Mathf.Deg2Rad)*mag+center.x, Mathf.Sin(i/(float)(numPoints)*360.0f*Mathf.Deg2Rad)*mag+center.y, center.z),
				new Vector3(Mathf.Cos((i+1)/(float)(numPoints)*360.0f*Mathf.Deg2Rad)*mag+center.x, Mathf.Sin((i+1)/(float)(numPoints)*360.0f*Mathf.Deg2Rad)*mag+center.y, center.z), c);
		}
	}
	public void AddDashedCircle(Vector3 center, float mag, Color c, float angleOffset, int numPoints = 40){
		for(int i = 0;i<numPoints;i++){
			float sAngle = (i/(float)(numPoints)*360.0f+angleOffset)*Mathf.Deg2Rad;
			float eAngle = ((i+0.5f)/(float)(numPoints)*360.0f+angleOffset)*Mathf.Deg2Rad;
			AddLine(new Vector3(Mathf.Cos(sAngle)*mag+center.x, Mathf.Sin(sAngle)*mag+center.y, center.z),
				new Vector3(Mathf.Cos(eAngle)*mag+center.x, Mathf.Sin(eAngle)*mag+center.y, center.z), c);
		}
	}
	public void AddCircleExplosion(Vector3 center, float mag, Color c, float angleOffset, int numPoints = 40){
		ExplosionManager em = GetComponent<ExplosionManager>();
		for(int i = 0;i<numPoints;i++){
			float sAngle = (i/(float)(numPoints)*360.0f+angleOffset)*Mathf.Deg2Rad;
			float eAngle = ((i+1f)/(float)(numPoints)*360.0f+angleOffset)*Mathf.Deg2Rad;
			Vector3 pa = new Vector3(Mathf.Cos(sAngle)*mag+center.x, Mathf.Sin(sAngle)*mag+center.y, center.z);
			Vector3 pb = new Vector3(Mathf.Cos(eAngle)*mag+center.x, Mathf.Sin(eAngle)*mag+center.y, center.z);
			Vector3 da = (pa-center).normalized.Rotate((Random.value-0.5f)*0.8f)*20.0f;
			Vector3 db = (pb-center).normalized.Rotate((Random.value-0.5f)*0.8f)*20.0f;
			em.AddLine(pa, pb, da, db, c);
		}
	}
	void SetupMesh(){
		for(int j=0;j<6;j++){
			float nMag = mag*(j+1);
			AddCircle(transform.position, nMag, Color.red);
		}
	}
	void LateUpdate(){
		// GetComponent<MeshFilter>().mesh.uv = uv;
		// GetComponent<MeshFilter>().mesh.colors32 = cs;
		mesh.vertices = verts;
		mesh.colors32 = cs;
		mesh.uv = uv;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
	}
	public void AddLine(Vector3 a, Vector3 b, Color c){
		if(exploding){
			GetComponent<ExplosionManager>().AddLine(a, b, Vector3.zero.Variation(new Vector3(15, 15, 0)), Vector3.zero.Variation(new Vector3(15, 5, 0)), c);
			return;
		}
		// bail if the current line is out of bounds
		if(currentLine+1 > maxLines){
			// ClearLines();
			Debug.Log("ran out of lines");
			return;
		}
		int index = currentLine*2;

		verts[index] = a;
		verts[index+1] = b;
		indices[index] = index;
		indices[index+1] = index+1;
		cs[index] = (Color32)c;
		cs[index+1] = (Color32)c;
		currentLine++;
	}
	void ClearLines(){
		currentLine = 0;
        for(int i=0;i<maxLines*2;i++){
        	verts[i] = Vector3.zero;
        }
		// clear all the indexes
		mesh.Clear();
	}
}