using UnityEngine;
using System.Collections;

public class BulletSpawner : MonoBehaviour {

	public Bullet bullet;
	public float bulletSpeed = 10f;
	public float spawnInterval = 1f;

	private float spawnTime = 0;
	
	void Update () {
		spawnTime += Time.deltaTime;
		if (spawnTime >= spawnInterval) {
			Instantiate(bullet, transform.position, transform.rotation);
			bullet.velocity = Utils.Vec3to2(transform.right) * bulletSpeed;
			bullet.source = gameObject;
			spawnTime = 0;
		}
	}
}
