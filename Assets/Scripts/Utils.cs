using UnityEngine;
using System.Collections;

public class Utils {

	public static Vector2 Vec3to2(Vector3 v) {
		return new Vector2(v.x, v.y);
	}
	public static Vector3 Vec2to3(Vector2 v) {
		return new Vector3(v.x, v.y, 0);
	}
	
	public static Vector3 PutOnGround(Vector2 v) {
		Ray ray = new Ray(new Vector3(v.x, v.y, -10000), Vector3.down);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			return hit.point;
		}
		return Vec2to3(v);
	}

	public static void LookAt2D(Transform transform, Vector2 target) {
		Vector2 dir = target - Vec3to2(transform.position);
		float angle = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

    public static bool InLayerMask(int layer, int mask) {
        return ((mask >> layer) & 1) == 1;
    }
}
