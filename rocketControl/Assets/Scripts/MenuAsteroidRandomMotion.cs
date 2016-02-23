using UnityEngine;
using System.Collections;

public class MenuAsteroidRandomMotion : MonoBehaviour {

	public bool startClockwise = false; 
	private bool clockwise;
	private int counter = 25;

	void Start () {
		clockwise = startClockwise;
	}

	// Update is called once per frame
	void Update () {
		float degrees = clockwise ? Random.Range(0.0f, 1.0f) : -1.0f * Random.Range(0.0f, 1.0f);
		gameObject.transform.Rotate (new Vector3 (0, 0, degrees));
		counter--;
		if (counter == 0) {
			counter = 50;
			clockwise = !clockwise;
		}
	}
}
