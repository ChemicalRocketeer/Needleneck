using UnityEngine;
using System.Collections;

public class UserControls : MonoBehaviour {

	public ActorController selected;
	public LayerMask interactableMask;

	public Waypoint modelWaypoint;

	private bool runningSim = false;

	void Update () {

		if (Input.GetMouseButtonDown(0)) {

			Vector3 groundPoint = Input.mousePosition;
			groundPoint.z = -Camera.main.transform.position.z;
			groundPoint = Camera.main.ScreenToWorldPoint(groundPoint);

			Vector3 rayOrigin = groundPoint;
			rayOrigin.z = Camera.main.transform.position.z;
			Ray ray = new Ray(rayOrigin, Vector3.forward);
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableMask)) { 				// interact with something
				ActorController actor = hit.transform.GetComponent<ActorController>();
				if (actor != null) {
					actor.OnClick();
					selected = actor;
				}
			} else { 																			// create a waypoint
				Waypoint waypoint = (Waypoint) Instantiate(modelWaypoint, groundPoint, Quaternion.identity);
				waypoint.actorController = selected;
				waypoint.transform.position = new Vector3(waypoint.transform.position.x, waypoint.transform.position.y, selected.transform.position.z);
				selected.AddWaypoint(waypoint);
			}
		}

		if (Input.GetKeyDown("backspace")) {
			if (runningSim) {
				ResetSimulation();
			} else {
				selected.RemoveWaypoint();
			}
		}
		if (Input.GetKeyDown("space")) {
			ToggleSimulation();
		}
	}

	public void ToggleSimulation() {
		runningSim = !runningSim;
		foreach (ActorController c in ActorController.instances) {
			c.runningSim = runningSim;
		}
	}

	public void ResetSimulation() {
		runningSim = false;
		foreach (ActorController c in ActorController.instances) {
			c.runningSim = runningSim;
			c.ResetSimulation();
		}
	}
}
