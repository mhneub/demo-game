using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class rocketController : MonoBehaviour
{

	// public variables
	public LayerMask ignore;
	public GameObject YouWinText;
	public Rigidbody2D bullet;
	public Sprite spriteNoFlame;
	public Sprite spriteWithFlame;
	public ParticleSystem explodeParticles;
	public AudioClip shootSound;
	public float shootSoundScale;
	public AudioClip explodeSound;
	public float explodeSoundScale;
	public AudioClip thrustSound;
	public float thrustSoundScale;
	public AudioClip swishSound;
	public float swishSoundScale;

	AudioSource audioSource;
	AudioSource audioSource_thrust;

	// constant variables
	const float Pi = 3.14159f;

	// physics parameters
	float allowedSpeed = 0.5f;
	float allowedAngle = 5f;
	float thrusterSpeed = 1f;
	float gunSpeed = 60f;	// per second!
	float rotationScale = -8f;
	float bulletSpeed = 10f;
	float lowPassFilterFactor = 0.2f;
	float bulletTimeInterval = 0.1f;	// minimum time between bullets in seconds
	float timeSinceLastBullet = 0.0f;
	float swipeTimeInterval = 0.2f;
	float timeSinceLastSwipe = 1000f;

	int flipped = 0;
	Vector3 originPosition;
	Vector3 originAngles;
	SpriteRenderer spriteRenderer;

	bool isThrusting;
	bool isShooting;
	bool rocketDead;

	ParticleSystem thrustParticleSystem;
	ParticleSystem thrustBurstParticleSystem;

	int lastLevel = 3;


	void Init()
	{
		YouWinText.gameObject.GetComponent<Renderer>().enabled = false;
		//transform.position = originPosition;
		transform.eulerAngles = originAngles;
		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		GetComponent<Rigidbody2D>().angularVelocity = 0f;
	}

	// Use this for initialization
	void Start()
	{
		Screen.orientation = ScreenOrientation.LandscapeRight;
		originAngles = new Vector3(0f, 0f, 0f);
		Init();

		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = spriteNoFlame;
		isThrusting = false;

		ParticleSystem[] ps = GetComponentsInChildren<ParticleSystem> ();
		thrustParticleSystem = ps [0];
		thrustBurstParticleSystem = ps [1];
		thrustParticleSystem.Stop ();
		thrustBurstParticleSystem.Stop ();
		rocketDead = false;

		audioSource = GameObject.Find("audioSource").GetComponent<AudioSource> ();
		audioSource_thrust = GameObject.Find("audioSource_thrust").GetComponent<AudioSource> ();
	}

	void shoot()
	{
		// don't shoot if rocket died
		if (timeSinceLastBullet >= bulletTimeInterval && !rocketDead) {
			Rigidbody2D instantiatedBullet = Instantiate(bullet, transform.position + 0.5f*transform.up, transform.rotation) as Rigidbody2D;
			instantiatedBullet.velocity = transform.up * bulletSpeed;	
			Physics2D.IgnoreCollision(instantiatedBullet.GetComponent<Collider2D>(), GetComponent<Rigidbody2D>().GetComponent<Collider2D>());

			GetComponent<Rigidbody2D>().AddForce(-gunSpeed * bulletTimeInterval * transform.up);

			audioSource.PlayOneShot(shootSound, shootSoundScale);

			timeSinceLastBullet = 0.0f;
		}
	}

	void thrust()
	{
		GetComponent<Rigidbody2D>().AddForce (thrusterSpeed * transform.up);
	}

	void rotateRocket(float axis)
	{
		Quaternion intermediateQuat = Quaternion.Euler(transform.eulerAngles);
		Quaternion targetQuat;
		targetQuat = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, rotationScale * axis * 180 / Pi + flipped);
		transform.rotation = Quaternion.Lerp(intermediateQuat, targetQuat, lowPassFilterFactor);
	}

	IEnumerator delayWin()
	{
		YouWinText.gameObject.GetComponent<Renderer>().enabled = true;
		yield return new WaitForSeconds(1);
		if (Application.loadedLevel == lastLevel) {
			Application.LoadLevel(0);
		} else {
			Application.LoadLevel(Application.loadedLevel + 1);
		}
	}

	IEnumerator delayLose()
	{
		yield return new WaitForSeconds(2);
		Application.LoadLevel(Application.loadedLevel);
	}

	void rocketDeath()
	{
		if (!rocketDead) {
			rocketDead = true;

			// actions on death
			gameObject.GetComponent<Collider2D>().enabled = false;
			gameObject.GetComponent<Renderer>().enabled = false;
			gameObject.GetComponentInChildren<CapsuleCollider> ().enabled = false;
			thrustParticleSystem.Stop ();

			Instantiate(explodeParticles, transform.position, transform.rotation);

			audioSource.PlayOneShot(explodeSound, explodeSoundScale);
			audioSource_thrust.Stop();

			StartCoroutine (delayLose());
		}
	}

	void win()
	{
		rocketDead = true;	// disables thrust, gun, etc.
		StartCoroutine(delayWin());
	}

	void OnCollisionEnter2D(Collision2D col)
	{
		Debug.Log ("rocket collided with " + col.gameObject.name);
		if (col.gameObject.name == "platform") {
			if (GetComponent<Rigidbody2D>().velocity.magnitude < allowedSpeed 
				&& (Mathf.Abs(transform.eulerAngles.z) < allowedAngle || Mathf.Abs(transform.eulerAngles.z) > (360 - allowedAngle))) {
				win();
			} else {
				rocketDeath();
			}
		}
		else if (col.gameObject.tag == "enemy" || col.gameObject.tag == "terrain") {
			rocketDeath();
		}
	}

	void rotate180()
	{
		if (flipped == 180)
			flipped = 0;
		else if (flipped == 0)
			flipped = 180;

		audioSource.PlayOneShot (swishSound, swishSoundScale);
	}

	void OnThrustStart() 
	{
		isThrusting = true;
		if (!rocketDead) {
			spriteRenderer.sprite = spriteWithFlame;
			thrustBurstParticleSystem.Emit(12);
			thrustParticleSystem.Play();
			audioSource_thrust.PlayOneShot (thrustSound, thrustSoundScale);
		}
	}

	void OnThrustStop() 
	{
		isThrusting = false;
		spriteRenderer.sprite = spriteNoFlame;
		thrustParticleSystem.Stop();
		audioSource_thrust.Stop();
	}

	void OnGunStart() 
	{
		isShooting = true;
	}

	void OnGunStop() 
	{
		isShooting = false;
	}

	void OnDoubleSwipe() 
	{
		rotate180();
	}

	void ButtonDown(string buttonTag) 
	{
		if (buttonTag == "thruster") {
			OnThrustStart ();
		} else if (buttonTag == "gun") {
			OnGunStart ();
		}
	}

	void ButtonUp(string buttonTag) 
	{	// means button was released and not swiped
		if (buttonTag == "thruster") {
			OnThrustStop ();
		} else if (buttonTag == "gun") {
			OnGunStop ();
		}
	}

	void ButtonSwiped(string buttonTag) 
	{
		// see if double-swipe happened
		if (buttonTag == "thruster") {
			OnThrustStop();
			if (timeSinceLastSwipe >= swipeTimeInterval) {
				OnDoubleSwipe();
			}
		} else if (buttonTag == "gun") {
			OnGunStop ();
			if (timeSinceLastSwipe >= swipeTimeInterval) {
				OnDoubleSwipe();
			}
		}
		timeSinceLastSwipe = 0f;
	}
		
	void FixedUpdate()
	{
		// update timeSince counters
		timeSinceLastBullet += Time.deltaTime;
		timeSinceLastSwipe += Time.deltaTime;

		if (isShooting) {
			shoot ();
		} 
		if (isThrusting) {
			thrust();
		}

		rotateRocket (Input.acceleration.x);
	}
}