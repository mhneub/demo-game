using UnityEngine;
using System.Collections;

public class bulletController : MonoBehaviour {

	public GameObject explodeParticles;
	public AudioClip hitSound;
	public float hitSoundScale;

	AudioSource audioSource;

	// Use this for initialization
	void Start () {
		audioSource = GameObject.Find ("audioSource").GetComponent<AudioSource> ();
		Destroy (gameObject, 5f);
	}

	void OnTriggerEnter2D(Collider2D col) {
		// destroy bullet if it hits asteroid or terrain
		if (col.gameObject.tag == "terrain" || col.gameObject.tag == "enemy") {
			Destroy(gameObject);

			audioSource.PlayOneShot(hitSound, hitSoundScale);

			// explode
			Instantiate(explodeParticles, transform.position, transform.rotation);
		}
	}
}