using UnityEngine;
using System.Collections;

public class bulletExplodeParticlesController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Destroy (gameObject, 1f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
