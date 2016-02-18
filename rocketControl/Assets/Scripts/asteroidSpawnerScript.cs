using UnityEngine;
using System.Collections;

public class asteroidSpawnerScript : MonoBehaviour {

	public Rigidbody2D asteroid;

	// Use this for initialization
	void Start () {
		float z = GameObject.Find ("rocket").transform.position.z;

		for (int i = 0; i < 5; i++) {
			Vector3 position = new Vector3 (Random.Range(100, Screen.width-100), Random.Range(100, Screen.height-100), 0);
			position = Camera.main.ScreenToWorldPoint(position);
			position.z = z;
			Vector3 axis = new Vector3(Random.Range(-1F, 1F), Random.Range(-1F, 1F), Random.Range(-1F, 1F)).normalized;
			float angle = Random.Range (0F, 180F);
			Rigidbody2D instantiatedAsteroid = Instantiate(asteroid, position, Quaternion.AngleAxis(angle, axis)) as Rigidbody2D;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
