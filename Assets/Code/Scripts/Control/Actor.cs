using UnityEngine;
using System.Collections.Generic;

public class Actor : Interactable {

	public static IList<Actor> instances = new List<Actor>();

	public int teamID;
	public Vector3 startPosition;
	public Quaternion startRotation;
	public float speed = 1f;

	public float hitRadius = 1f;
	public float fieldOfView = 60f;
	public float range = 5f;
	public float aimingShootTimeFraction = 0.7f;
	public float aimingMoveSpeedFraction = 0.7f;
	public float shootTime = 0.2f;
	public float bulletSpeed = 100f;
	public LayerMask targetMask;
	public LayerMask wallMask;

	public LineRenderer waypointLines;
	public LineRenderer fovLines;
	public GameObject deadModel;
	public Waypoint waypointPrefab;
	public Bullet bullet;

	public bool runningSim = false;

	private IList<Waypoint> waypoints = new List<Waypoint>();
	private int targetWaypoint = 0;

	public SimStates simState = SimStates.WALKING;
	public enum SimStates {
		WALKING,
		FIRING,
		DEAD
	}

	private bool aiming = false;
	private float aimDirection;
	private float shootTimeDelay = 0f;
	private Actor target;
	private GameObject deadSelf;

	void Start() {
		instances.Add(this);
		startPosition = transform.position;
		startRotation = transform.rotation;
		waypointLines.SetPosition(0, transform.position);
		fovLines.SetVertexCount(3);
	}

	void Update() {

		// setup fieldOfView lines
		float fovMod = fieldOfView * Mathf.Deg2Rad * 0.5f;
		Vector2 fovPointLeft = Utils.Vec2FromAngle(Utils.AngleOf(Utils.Vec3to2(transform.right)) + fovMod, range);
		Vector2 fovPointRight = Utils.Vec2FromAngle(Utils.AngleOf(Utils.Vec3to2(transform.right)) - fovMod, range);
		fovLines.SetPosition(0, transform.position + Utils.Vec2to3(fovPointLeft));
		fovLines.SetPosition(1, transform.position);
		fovLines.SetPosition(2, transform.position + Utils.Vec2to3(fovPointRight));

		if (runningSim && simState != SimStates.DEAD) {
			// aim and stuff
			target = FindTarget();
			if (target != null) {
				simState = SimStates.FIRING;
				Utils.LookAt2D(transform, Utils.Vec3to2(target.transform.position));
				shootTimeDelay += Time.deltaTime;
				if (shootTimeDelay >= shootTime) {
					FireBullet();
					shootTimeDelay = 0;
				}
			} else {
				simState = SimStates.WALKING;
				shootTimeDelay = 0;
				target = null;
			}

			if (simState == SimStates.WALKING && targetWaypoint < waypoints.Count) {
				// move along waypoints
				Vector2 targetPosition = Utils.Vec3to2(waypoints[targetWaypoint].transform.position);
				Vector2 distance = targetPosition - Utils.Vec3to2(transform.position);
				Vector3 movement = Utils.Vec2to3(distance.normalized * speed * Time.deltaTime);

				Utils.LookAt2D(transform, targetPosition);

				Vector3 newPos = transform.position + movement;
				
				// prevent going past the waypoint
				if (distance.magnitude < movement.magnitude) {
					movement = distance;
					newPos = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
					// adjust waypoint lines
					waypoints[targetWaypoint].renderer.enabled = false;
					targetWaypoint += 1;
					int lineVertexCount = GetWaypointLineVertexCount();
					waypointLines.SetVertexCount(lineVertexCount);
					for (int i = targetWaypoint; i < waypoints.Count; i++) {
						UpdateWaypointLine(i);
					}
				}

				// prevent running through walls
				RaycastHit hit;
				if (Physics.SphereCast(transform.position, hitRadius, movement.normalized, out hit, movement.magnitude, wallMask)) {
					movement = Vector3.zero;
					newPos = transform.position;
				}

				transform.position = newPos;
			}
		}

		// adjust position with line renderer
		waypointLines.SetPosition(0, transform.position);
	}

	public void ResetSimulation() {
		simState = SimStates.WALKING;
		gameObject.SetActive(true);
		transform.position = startPosition;
		transform.rotation = startRotation;
		targetWaypoint = 0;
		waypointLines.SetVertexCount(waypoints.Count + 1);
		waypointLines.SetPosition(0, transform.position);
		shootTimeDelay = 0f;
		GameObject.Destroy(deadSelf);
		for (int i = 0; i < waypoints.Count; i++) {
			Waypoint w = waypoints[i];
			w.renderer.enabled = true;
			w.gameObject.SetActive(true);
			UpdateWaypointLine(i);
		}
	}

	public void FireBullet() {
		Bullet b = (Bullet) Instantiate(bullet, transform.position, Quaternion.identity);
		b.velocity = Utils.Vec2FromAngle(Utils.AngleOf(Utils.Vec3to2(transform.right)), bulletSpeed);
		b.teamID = teamID;
	}

	public void Kill(GameObject killer) {
		simState = SimStates.DEAD;
		gameObject.SetActive(false);
		foreach (Waypoint w in waypoints) {
			w.gameObject.SetActive(false);
		}
		deadSelf = (GameObject) Instantiate(deadModel, transform.position, transform.rotation);
	}

	public bool InFOV(Transform target) {
		if (target == null) return false;
		Vector2 otherPos = Utils.Vec3to2(target.transform.position - transform.position);
		float otherAngle = Utils.AngleOf(otherPos) * Mathf.Rad2Deg;
		float fovMod = fieldOfView * 0.5f;
		float myAngle = Utils.AngleOf(transform.right) * Mathf.Rad2Deg;
		return Utils.BetweenAngles(myAngle + fovMod, otherAngle, myAngle - fovMod);
	}

	private Actor FindTarget() {
		// check if there is an existing actor that can be fired at
		if (target != null && target.simState != SimStates.DEAD) {
			Vector3 dist = target.transform.position - transform.position;
			if (dist.magnitude <= range && InFOV(target.transform)) {
				Ray ray = new Ray(transform.position, dist.normalized);
				if (!Physics.Raycast(ray, dist.magnitude, wallMask)) {
					return target;
				}
			}
		}
		// look for a new actor
		Collider[] colliders = Physics.OverlapSphere(transform.position, range, targetMask);
		foreach (Collider c in colliders) {
			if (c != this.collider) {
				Actor actor = c.GetComponent<Actor>();
				if (actor.teamID != teamID && actor.simState != SimStates.DEAD && InFOV(actor.transform)) {
					Vector3 dist = c.transform.position - transform.position;
					Ray ray = new Ray(transform.position, dist.normalized);
					if (!Physics.Raycast(ray, dist.magnitude, wallMask)) {
						return actor;
					}
				}
			}
		}
		return null;
	}

	public void SetAim(float angle) {

	}

	public void MakeWaypoint(Vector2 position) {
		Vector3 pos = Utils.PutOnGround(position, transform.position.z);
		Vector3 lastPos;
		if (waypoints.Count > 0) {
			lastPos = waypoints[waypoints.Count - 1].transform.position;
		} else {
			lastPos = transform.position;
		}
		// if the path to the new waypoint goes through any walls, put the waypoint on the near side of the wall instead
		Vector3 dist = pos - lastPos;
		RaycastHit hit;
		if (Physics.SphereCast(lastPos, hitRadius, dist.normalized, out hit, dist.magnitude, wallMask)) {
			return;
		}
		// instantiate waypoint and setup its state
		Waypoint w = (Waypoint) Instantiate(waypointPrefab, pos, Quaternion.identity);
		w.actorController = this;
		AddWaypoint(w);
	}
	
	public void AddWaypoint(Waypoint w) {
		waypoints.Add(w);
		waypointLines.SetVertexCount(GetWaypointLineVertexCount());
		UpdateWaypointLine(waypoints.Count - 1);
	}
	
	public void RemoveWaypoint(Waypoint w) {
		RemoveWaypoint(waypoints.IndexOf(w));
	}
	
	public void RemoveWaypoint() {
		RemoveWaypoint(waypoints.Count - 1);
	}
	
	public void RemoveWaypoint(int index) {
		if (waypoints.Count > 0 && index > 0 && index < waypoints.Count) {
			GameObject.Destroy(waypoints[index].gameObject);
			waypoints.RemoveAt(index);
			if (targetWaypoint > waypoints.Count) {
				targetWaypoint = waypoints.Count;
			}
			waypointLines.SetVertexCount(GetWaypointLineVertexCount());
			for (; index < waypoints.Count; index ++) {
				UpdateWaypointLine(index);
			}
		}
	}
	
	public void UpdateWaypointLine(Waypoint w) {
		UpdateWaypointLine(waypoints.IndexOf(w));
	}

	/// <summary>
	/// Resets the waypoint line at the given waypoint.
	/// Does not check whether the line is actually being rendered at that waypoint; error checking must be done outside this method
	/// </summary>
	/// <param name="index">Index (the index in the list of waypoints, not the index in the line renderer)</param>
	private void UpdateWaypointLine(int index) {
		waypointLines.SetPosition(index - targetWaypoint + 1, waypoints[index].transform.position);
	}
	
	private int GetWaypointLineVertexCount() {
		// targetWaypoint can also be used to represent the number of waypoints traveled so far.
		// Take the number of total waypoints, subtract waypoints traveled so far,
		// and you have the number of waypoints left to go.
		// Add 1 for the actor, and you get the number of vertices
		return waypoints.Count - targetWaypoint + 1;
	}

	public override void OnClick()
	{
	}
	
	public override void OnHold()
	{
	}
	
	public override void OnRelease()
	{
	}
	
	public override void OnExit()
	{
	}
	
	public override void OnDrag(Vector2 draggedTo)
    {
	}

	public void OnDrawGizmos() {
		// setup fieldOfView lines
		float fovMod = fieldOfView * Mathf.Deg2Rad * 0.5f;
		Vector2 fovPointLeft = Utils.Vec2FromAngle(Utils.AngleOf(Utils.Vec3to2(transform.right)) + fovMod, range);
		Vector2 fovPointRight = Utils.Vec2FromAngle(Utils.AngleOf(Utils.Vec3to2(transform.right)) - fovMod, range);
		Gizmos.DrawLine(transform.position, transform.position + Utils.Vec2to3(fovPointLeft));
		Gizmos.DrawLine(transform.position, transform.position + Utils.Vec2to3(fovPointRight));
	}
}
