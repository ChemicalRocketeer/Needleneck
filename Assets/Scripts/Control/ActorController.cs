using UnityEngine;
using System.Collections.Generic;

public class ActorController : Interactable {
	
	public Vector3 startPosition;
	public float speed = 1f;

	public bool simulateOnStart = false;
	public LineRenderer waypointLines;
	
	private IList<Waypoint> waypoints;
	private int targetWaypoint = -1;

	void Awake() {
		waypoints = new List<Waypoint>();
	}

	void Start() {
		startPosition = transform.position;
		if (simulateOnStart) {
			BeginSimulation();
		}
	}

	void Update() {
		if (targetWaypoint != -1 && targetWaypoint < waypoints.Count) {
			Vector2 targetPosition = Utils.Vec3to2(waypoints[targetWaypoint].transform.position);
			Vector2 distance = targetPosition - Utils.Vec3to2(transform.position);
			Vector2 movement = distance.normalized * speed * Time.deltaTime;
			if (distance.magnitude < movement.magnitude) { // prevent going past the waypoint
				transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
			} else {
				transform.position = transform.position + Utils.Vec2to3(movement);
			}
			transform.LookAt(Utils.Vec2to3(targetPosition));

			waypointLines.SetPosition(0, transform.position);
			if (Utils.Vec3to2(transform.position) == targetPosition) { // at waypoint
				waypoints[targetWaypoint].SetVisibile(false);
				targetWaypoint += 1;
				int lineVertexCount = GetLineVertexCount();
				if (targetWaypoint <= waypoints.Count) {
					for (int i = targetWaypoint; i < waypoints.Count; i++) {
						// targetWaypoint = waypoints traveled to so far
						waypointLines.SetPosition(i - targetWaypoint + 1, waypoints[i].transform.position);
					}
				}
				waypointLines.SetVertexCount(lineVertexCount);
			}
		}
	}

	public void UpdateWaypointLines(Waypoint w) {
		int i = waypoints.IndexOf(w) + 1;
		waypointLines.SetPosition(i, w.transform.position);
	}

	public void AddWaypoint(Waypoint w) {
		waypoints.Add(w);
		waypointLines.SetVertexCount(GetLineVertexCount());
		waypointLines.SetPosition(waypoints.Count - targetWaypoint, w.transform.position);
	}

	public void RemoveWaypoint(Waypoint w) {
		waypoints.Remove(w);
		waypointLines.SetVertexCount(GetLineVertexCount());
	}

	public void BeginSimulation() {
		transform.position = startPosition;
		targetWaypoint = 0;
	}

	private int GetLineVertexCount() {
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
