using UnityEngine;
using System.Collections;

public class Waypoint : Interactable {

	public Actor actorController;

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

	public override void OnDrag(Vector2 distance)
	{
		transform.Translate(Utils.Vec2to3(distance));
		actorController.UpdateWaypointLine(this);
	}
}
