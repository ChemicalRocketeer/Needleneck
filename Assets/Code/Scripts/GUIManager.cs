using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GUIManager : MonoBehaviour {

	private static List<Rect> guiElements = new List<Rect>();
	private static List<Rect> toRemove = new List<Rect>();

	void LateUpdate() {
		foreach (Rect r in toRemove) {
			guiElements.Remove(r);
		}
	}

	public static void AddGUIElement(Rect r) {
		guiElements.Add(r);
	}

	public static void RemoveGUIElement(Rect r) {
		toRemove.Add(r);
	}

	public static bool IsInGUIElement(Vector2 v) {
		// for gui elements, the y axis is upside down, so adjust for that
		v = new Vector2(v.x, Screen.height - v.y);
		foreach (Rect r in guiElements) {
			if (r.Contains(v)) return true;
		}
		return false;
	}
}
