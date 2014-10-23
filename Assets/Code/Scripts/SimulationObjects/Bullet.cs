using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public Vector2 velocity;
	public float radius = 1f;
	public LayerMask collisionMask;
	public GameObject source;
	public int teamID;

	void Update() {
		Vector3 movement = Utils.Vec2to3(velocity * Time.deltaTime);
		
		RaycastHit hit;
		if (Physics.SphereCast(transform.position, radius, movement.normalized, out hit, movement.magnitude, collisionMask)) {
			movement = hit.point - transform.position;
			velocity = Vector2.zero;
			//GameObject.Destroy(gameObject);
			Actor ac = hit.collider.GetComponent<Actor>();
			if (ac && ac.teamID != teamID) {
				ac.Kill(source);
			}
		}
		 
		transform.position = transform.position + movement;
	}
}
