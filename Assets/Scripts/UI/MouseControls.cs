using UnityEngine;
using System.Collections;

public class MouseControls : MonoBehaviour {

	public ActorController selected;
	public LayerMask interactableMask;

	public Waypoint modelWaypoint;

	private bool mouseDownLastFrame = false;

	void Update () {

		if (Input.GetMouseButtonDown(0)) {

			Vector3 groundPoint = Input.mousePosition;
			groundPoint.z = -Camera.main.transform.position.z;
			groundPoint = Camera.main.ScreenToWorldPoint(groundPoint);

			Vector3 rayOrigin = groundPoint;
			rayOrigin.z = Camera.main.transform.position.z;
			Ray ray = new Ray(rayOrigin, Vector3.forward);
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableMask)) { // the user clicked on something to interface with it
				ActorController actor = hit.transform.GetComponent<ActorController>();
				if (actor != null) {
					actor.OnClick();
					selected = actor;
				}
			} else { // user clicked to create a waypoint
				Waypoint waypoint = (Waypoint) Instantiate(modelWaypoint, groundPoint, Quaternion.identity);
				waypoint.actorController = selected;
				waypoint.transform.position = new Vector3(waypoint.transform.position.x, waypoint.transform.position.y, selected.transform.position.z);
				selected.AddWaypoint(waypoint);
			}
		}
	}
}
