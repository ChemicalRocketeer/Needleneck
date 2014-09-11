using UnityEngine;
using System.Collections.Generic;

public class ActorController : Interactable {

	public static IList<ActorController> instances = new List<ActorController>();

	public int playerID;
	public Vector3 startPosition;
	public float speed = 1f;
	public float hitRadius = 1f;
	public LayerMask wallMask;
	public LineRenderer waypointLines;
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
		waypointLines.SetPosition(0, transform.position);
	}

	void Update() {
		if (runningSim && targetWaypoint < waypoints.Count) {
			// we are moving along waypoints
			Vector2 targetPosition = Utils.Vec3to2(waypoints[targetWaypoint].transform.position);
			Vector2 distance = targetPosition - Utils.Vec3to2(transform.position);
			Vector3 movement = Utils.Vec2to3(distance.normalized * speed * Time.deltaTime);

			transform.LookAt(new Vector3(targetPosition.x, targetPosition.y, transform.position.z), transform.up);
			Debug.DrawRay(transform.position, transform.up);
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
		targetWaypoint = 0;
		waypointLines.SetVertexCount(waypoints.Count + 1);
		waypointLines.SetPosition(0, transform.position);
		for (int i = 0; i < waypoints.Count; i++) {
			waypoints[i].renderer.enabled = true;
			UpdateWaypointLine(i);
		}
	}
	
	public void UpdateWaypointLine(Waypoint w) {
		UpdateWaypointLine(waypoints.IndexOf(w));
	}

	public void Kill(GameObject killer) {
		alive = false;
		gameObject.SetActive(false);
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
