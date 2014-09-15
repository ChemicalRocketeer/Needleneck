using UnityEngine;
using System.Collections.Generic;

public class ActorController : Interactable {

	public static IList<ActorController> instances = new List<ActorController>();

	public int playerID;
	public Vector3 startPosition;
	public Quaternion startRotation;
	public float speed = 1f;
	public float hitRadius = 1f;
	public float fieldOfView = 60f;
	public float viewRange = 5f;

	public LayerMask wallMask;
	public LineRenderer waypointLines;
	public LineRenderer fovLines;
	public GameObject deadModel;

	public bool runningSim = false;
	private bool alive = true;
	public bool Alive {
		get { return alive; }
	}

	private IList<Waypoint> waypoints = new List<Waypoint>();
	private int targetWaypoint = 0;

	void Start() {
		instances.Add(this);
		startPosition = transform.position;
		startRotation = transform.rotation;
		waypointLines.SetPosition(0, transform.position);
	}

	void Update() {
		// setup fieldOfView lines
		float fovMod = fieldOfView * Mathf.Deg2Rad * 0.5f;
		Vector2 fovPointLeft = Utils.Vec2FromAngle(Utils.AngleOf(Utils.Vec3to2(transform.right)) + fovMod, viewRange);
		Vector2 fovPointRight = Utils.Vec2FromAngle(Utils.AngleOf(Utils.Vec3to2(transform.right)) - fovMod, viewRange);
		fovLines.SetPosition(0, transform.position + Utils.Vec2to3(fovPointLeft));
		fovLines.SetPosition(1, transform.position);
		fovLines.SetPosition(2, transform.position + Utils.Vec2to3(fovPointRight));

		if (runningSim && targetWaypoint < waypoints.Count) {
			// move along waypoints
			Vector2 targetPosition = Utils.Vec3to2(waypoints[targetWaypoint].transform.position);
			Vector2 distance = targetPosition - Utils.Vec3to2(transform.position);
			Vector3 movement = Utils.Vec2to3(distance.normalized * speed * Time.deltaTime);

			Utils.LookAt2D(transform, targetPosition);


			if (distance.magnitude < movement.magnitude) {
				// prevent going past the waypoint
				transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
			} else {
				// prevent running through walls
				RaycastHit hit;
				if (Physics.SphereCast(transform.position, hitRadius, movement.normalized, out hit, movement.magnitude, wallMask)) {
					movement = hit.point - transform.position;
				}
				transform.position = transform.position + movement;
			}

			if (Utils.Vec3to2(transform.position) == targetPosition) { // at waypoint
				waypoints[targetWaypoint].renderer.enabled = false;
				targetWaypoint += 1;
				int lineVertexCount = GetWaypointLineVertexCount();
				waypointLines.SetVertexCount(lineVertexCount);
				for (int i = targetWaypoint; i < waypoints.Count; i++) {
					UpdateWaypointLine(i);
				}
			}
		}

		// adjust position with line renderer
		waypointLines.SetPosition(0, transform.position);
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
		if (waypoints.Count > 0) {
			RemoveWaypoint(waypoints.Count - 1);
		}
	}
	
	public void RemoveWaypoint(int index) {
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

	public void ResetSimulation() {
		alive = true;
		gameObject.SetActive(true);
		transform.position = startPosition;
		transform.rotation = startRotation;
		targetWaypoint = 0;
		waypointLines.SetVertexCount(waypoints.Count + 1);
		waypointLines.SetPosition(0, transform.position);
		for (int i = 0; i < waypoints.Count; i++) {
			Waypoint w = waypoints[i];
			w.renderer.enabled = true;
			w.gameObject.SetActive(true);
			UpdateWaypointLine(i);
		}
	}
	
	public void UpdateWaypointLine(Waypoint w) {
		UpdateWaypointLine(waypoints.IndexOf(w));
	}

	public void Kill(GameObject killer) {
		alive = false;
		gameObject.SetActive(false);
		foreach (Waypoint w in waypoints) {
			w.gameObject.SetActive(false);
		}
		Instantiate(deadModel, transform.position, transform.rotation);
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
}
