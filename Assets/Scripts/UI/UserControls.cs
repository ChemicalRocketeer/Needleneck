using UnityEngine;
using System.Collections;

public class UserControls : MonoBehaviour {

	public int playerID = 0;
	public ActorController selected;
	public LayerMask interactableMask;

	public Waypoint modelWaypoint;

	public float normalTimeScale = 1f;
	public float slowTimeScale = .2f;

	private SimulationStates simState;

	public enum SimulationStates {
		PLANNING_MODE,
		RUNNING,
		PAUSED
	}

	void Update () {

		if (Input.GetMouseButtonDown(0)) {

			if (selected.simState == ActorController.SimStates.DEAD) {
				selected = ResolveDeadSelectedActor(ActorController.instances.IndexOf(selected));
			}

			Vector3 groundPoint = Input.mousePosition;
			groundPoint.z = -Camera.main.transform.position.z;
			groundPoint = Camera.main.ScreenToWorldPoint(groundPoint);

			Vector3 rayOrigin = groundPoint;
			rayOrigin.z = Camera.main.transform.position.z;
			Ray ray = new Ray(rayOrigin, Vector3.forward);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableMask)) { 
				// interact with something
				ActorController actor = hit.transform.GetComponent<ActorController>();
				if (actor != null) {
					actor.OnClick();
					selected = actor;
				}
			} else { // create a waypoint
				Waypoint waypoint = (Waypoint) Instantiate(modelWaypoint, groundPoint, Quaternion.identity);
				waypoint.actorController = selected;
				waypoint.transform.position = new Vector3(waypoint.transform.position.x, waypoint.transform.position.y, selected.transform.position.z);
				selected.AddWaypoint(waypoint);
			}
		}

		if (Input.GetKeyDown("backspace")) {
			if (simState != SimulationStates.PLANNING_MODE) {
				ResetSimulation();
			} else {
				selected.RemoveWaypoint();
			}
		}
		if (Input.GetKeyDown("space")) {
			ToggleSimulation();
		}
		if (Input.GetKeyDown("f2")) {
			ToggleSlowTime();
		}
	}

	public void ToggleSimulation() {
		bool runningSim = false;
		if (simState != SimulationStates.RUNNING) {
			runningSim = true;
			simState = SimulationStates.RUNNING;
		} else {
			simState = SimulationStates.PAUSED;
		}
		foreach (ActorController c in ActorController.instances) {
			c.runningSim = runningSim;
		}
	}

	public void ResetSimulation() {
		simState = SimulationStates.PLANNING_MODE;
		foreach (ActorController c in ActorController.instances) {
			c.runningSim = false;
			c.ResetSimulation();
		}
	}

	public void ToggleSlowTime() {
		if (Time.timeScale == normalTimeScale) {
			Time.timeScale = slowTimeScale;
		} else {
			Time.timeScale = normalTimeScale;
		}
	}

	private ActorController ResolveDeadSelectedActor(int index) {
		int i = (index + 1) % ActorController.instances.Count;
		while (i != index) {
			ActorController next = ActorController.instances[i];
			if (next.simState != ActorController.SimStates.DEAD) return next;
			i = (index + 1) % ActorController.instances.Count;
		}
		return ActorController.instances[index];
	}
}
