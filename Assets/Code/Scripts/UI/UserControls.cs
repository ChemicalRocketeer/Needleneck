using UnityEngine;
using System.Collections;

public class UserControls : MonoBehaviour {

	public int playerID = 0;
	public Actor selected;
	public LayerMask interactableMask;

	public float normalTimeScale = 1f;
	public float slowTimeScale = .2f;

	public Vector2 menuButtonSize = new Vector2(100, 20);

	private SimulationStates simState;

	public enum SimulationStates {
		PLANNING_MODE,
		RUNNING,
		PAUSED
	}

	void Update () {

		if (selected != null) {
			Vector2 selPos = Utils.Vec3to2(selected.transform.position);
			if (GUI.Button(new Rect(selPos.x + 10, selPos.y - 10, menuButtonSize.x, menuButtonSize.y), "Aim")) {
				selected.
			}
		}

		if (Input.GetMouseButtonDown(0) && !GUIManager.IsInGUIElement(Input.mousePosition)) {

			if (selected.simState == Actor.SimStates.DEAD) {
				selected = ResolveDeadSelectedActor(Actor.instances.IndexOf(selected));
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
				Actor actor = hit.transform.GetComponent<Actor>();
				if (actor != null) {
					actor.OnClick();
					selected = actor;
				}
			} else { // create a waypoint
				selected.MakeWaypoint(Utils.Vec3to2(groundPoint));
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
		foreach (Actor c in Actor.instances) {
			c.runningSim = runningSim;
		}
	}

	public void ResetSimulation() {
		simState = SimulationStates.PLANNING_MODE;
		foreach (Actor c in Actor.instances) {
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

	private Actor ResolveDeadSelectedActor(int index) {
		int i = (index + 1) % Actor.instances.Count;
		while (i != index) {
			Actor next = Actor.instances[i];
			if (next.simState != Actor.SimStates.DEAD) return next;
			i = (index + 1) % Actor.instances.Count;
		}
		return Actor.instances[index];
	}
}
