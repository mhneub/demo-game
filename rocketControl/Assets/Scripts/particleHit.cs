using UnityEngine;
using System.Collections;

public class particleHit : MonoBehaviour {

	public float force;

	GameObject parent;
	Rigidbody2D parentRigidBody;

	// Use this for initialization
	void Start () {
		parent = gameObject.transform.parent.gameObject;
		parentRigidBody = parent.GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	
	void OnParticleCollision(GameObject other) {
		//Debug.Log ("particle collided " + other.name);
		Vector3 direction = transform.position - other.transform.position;
		direction = direction.normalized;
		parentRigidBody.AddForce(direction * force);
	}
}
