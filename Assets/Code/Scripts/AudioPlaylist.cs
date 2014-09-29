using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class AudioPlaylist : MonoBehaviour {

	public float timeDelay = 1f;
	public AudioClip[] clips;

	private AudioSource source;
	private int[] shuffled;
	private int index = 0;

	void Start() {
		source = GetComponent<AudioSource>();
		source.loop = false;
		shuffled = new int[clips.Length];
		Shuffle();
		source.clip = clips[shuffled[index++]];
		source.Play();
	}

	void Update() {
		if (!source.clip || source.time >= source.clip.length) {
			if (index == shuffled.Length) {
				Shuffle();
				index = 0;
			}
			source.clip = clips[shuffled[index++]];
			source.Play();
		}
	}

	public void Shuffle() {
		for (int i = 0; i < shuffled.Length; i++) {
			shuffled[i] = i;
		}
		for (int i = 0; i < shuffled.Length; i++) {
			int r = (int) Random.Range(0, shuffled.Length - 1);
			shuffled[i] = shuffled[r];
			shuffled[r] = i;
		}
	}
}
