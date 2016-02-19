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
	
	void OnParticleCollision(GameObject other) {
		Vector3 direction = transform.position - other.transform.position;
		direction = direction.normalized;
		parentRigidBody.AddForce(direction * force);
	}
}
