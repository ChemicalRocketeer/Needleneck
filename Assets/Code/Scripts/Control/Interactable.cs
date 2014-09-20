using UnityEngine;
using System.Collections;

public abstract class Interactable : MonoBehaviour {
	public abstract void OnClick();
	public abstract void OnHold();
	public abstract void OnRelease();
	public abstract void OnExit();
	public abstract void OnDrag(Vector2 distance);
}
