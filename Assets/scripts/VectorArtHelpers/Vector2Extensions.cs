using UnityEngine;
using System.Collections;

public static class TensionExtensions
{
	#region Vector2 extensions
	public static Vector2 Rotate(this Vector2 v, float a){
		float px = v.x*Mathf.Cos(a) - v.y*Mathf.Sin(a);
		float py = v.x*Mathf.Sin(a) + v.y*Mathf.Cos(a);
		return new Vector2(px, py);
	}
	public static float Angle(this Vector2 v){
		return Mathf.Atan2(v.x, v.y)*180.0f/Mathf.PI;
	}
	#endregion

	#region Vector3 extensions
	public static Vector3 Variation(this Vector3 v, float amount){
		return new Vector3(v.x+(Random.value-0.5f)*amount, v.y+(Random.value-0.5f)*amount, v.z+(Random.value-0.5f)*amount);
	}
	#endregion
	#region Color extensions
	public static Color Alpha(this Color c, float newAlpha){
		return new Color(c.r, c.g, c.b, newAlpha);
	}
	#endregion
}
