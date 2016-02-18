using UnityEngine;
using System.Collections;

public class asteroidController : MonoBehaviour {

	Vector3 axis;
	float angle;
	Vector3 vel;
	float rotationSpeed = 0.01f;

	// Use this for initialization
	void Start () {
		transform.rotation.ToAngleAxis (out angle, out axis);
		float speed = Random.Range (0f, 1f);
		Vector3 velocityDir = new Vector3(Random.Range(-1F, 1F), Random.Range(-1F, 1F), 0).normalized;
		vel = speed * velocityDir;
	}

	void updateKinematics(){
		transform.RotateAround (transform.position, axis, angle * rotationSpeed);
		transform.position += vel * Time.fixedDeltaTime;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if (Camera.main.WorldToScreenPoint (transform.position).x < 0
						|| Camera.main.WorldToScreenPoint (transform.position).x > Screen.width){
			vel.x = -vel.x;
		}
		if (Camera.main.WorldToScreenPoint (transform.position).y < 0
		    || Camera.main.WorldToScreenPoint (transform.position).y > Screen.height){
			vel.y = -vel.y;
		}
		updateKinematics ();
	}

	void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.tag == "enemy") {
			Vector2 impulse = new Vector2(-vel.x, -vel.y);
			rigidbody2D.AddForce(impulse);
		}
	}
}
