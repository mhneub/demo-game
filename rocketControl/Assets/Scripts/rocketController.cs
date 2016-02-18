using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class rocketController : MonoBehaviour {

	// public variables
	public LayerMask ignore;
	public GameObject YouLoseText;
	public GameObject YouWinText;
	public Rigidbody2D bullet;


	// constant variables
	const float Pi = 3.14159f;

	// physics parameters
	float allowedSpeed = 0.5f;
	float allowedAngle = 5f;
	float rocketSizeScale = 0.5f;
	float thrusterSpeed = 1f;
	float rotationScale = -4f;
	float bulletSpeed = 5;
	float lowPassFilterFactor = 0.5f;
	float minSwipeDist = 50;
	float swipeDistVertical;
	int bulletTimeInterval = 10;
	int swipeCount = 0;
	int flipped = 0;
	bool touchThruster = false;
	bool touchGun = false;
	Vector3 originPosition;
	Vector3 originAngles;
	Vector2 vel;
	Dictionary<int,Vector2> initialTouchPos;
	Dictionary<int, float> swipeDist;



	//old physics parameters
	/*float gravity = -0.5f;
	Vector3 acc;
	Vector3 velocity;
	Vector3 jerk;
	Vector3 G;
	float gunRecoil = -10000f;
	float friction = 0.5f;
	float rotationSpeed = -5f;
	int rotationScale = 5;*/

	void Init() {
		YouLoseText.gameObject.renderer.enabled = false;
		YouWinText.gameObject.renderer.enabled = false;
		transform.position = originPosition;
		transform.eulerAngles = originAngles;
		rigidbody2D.velocity = Vector3.zero;
		rigidbody2D.angularVelocity = 0f;

	}

	// Use this for initialization
	void Start () {
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		vel = new Vector3(0f, 0f, 0f);
		originPosition = new Vector3 (0f, 0f, 0f);
		originAngles = new Vector3 (0f, 0f, 0f);
		transform.localScale = new Vector3 (rocketSizeScale, rocketSizeScale, rocketSizeScale);
		initialTouchPos = new Dictionary<int, Vector2> ();
		swipeDist = new Dictionary<int, float> ();
		Init ();
	}

	void thrust(){
		RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up);
		if (hit.collider != null) {
			GameObject recipient = hit.transform.gameObject;
			if (recipient.tag == "enemy") {
				float dist = (recipient.transform.position - transform.position).magnitude;
				recipient.rigidbody2D.AddForce(-transform.up * 10/(dist)); //add distance parameter maybe
			}
		}
	}

	void shoot(){
		if ((int)(Time.time * 100) % bulletTimeInterval == 0) {
			Rigidbody2D instantiatedBullet = Instantiate (bullet, transform.position, transform.rotation) as Rigidbody2D;
			instantiatedBullet.velocity = transform.up * bulletSpeed;	
			Physics2D.IgnoreCollision(instantiatedBullet.collider2D, rigidbody2D.collider2D);
		}
	}

	/*string pushButton(Vector3 touchPosition) {
		RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);
		if(hit.collider != null){
			GameObject recipient = hit.transform.gameObject;
			if (recipient.name == "Thruster" || recipient.name == "Gun"){
			}
		}
	}*/

	void translateRocket(Vector3 touchPosition){
		RaycastHit2D hit = Physics2D.Raycast(touchPosition, Vector2.zero);
		if(hit.collider != null){
			GameObject recipient = hit.transform.gameObject;
			if (recipient.name == "Thruster"){
				rigidbody2D.AddForce(thrusterSpeed * transform.up);
				thrust();
				//acc += thrusterSpeed*transform.up;
			}
			else if(recipient.name == "Gun") {
				//acc -= thrusterSpeed*transform.up;
				rigidbody2D.AddForce(-transform.up);
				shoot();
			}
		}

	}

	void rotateRocket(float axis){
		//int rotateStep = (int)(axis * 180/Pi) / rotationScale;
		//transform.Rotate(0, 0, rotateStep * rotationScale * rotationSpeed * Pi / 180);
		Quaternion intermediateQuat = Quaternion.Euler (transform.eulerAngles);
		Quaternion targetQuat = Quaternion.Euler (transform.eulerAngles.x, transform.eulerAngles.y, rotationScale * axis * 180/Pi + flipped);
		transform.rotation = Quaternion.Lerp(intermediateQuat, targetQuat, lowPassFilterFactor);
	}

	/*void updateKinematics(){
		velocity = acc * Time.fixedDeltaTime + G * Time.fixedTime;
		acc -= friction * velocity;
		transform.position += velocity * Time.fixedDeltaTime;
	}*/

	IEnumerator delay(bool win){
		if (win) {
			YouWinText.gameObject.renderer.enabled = true;
		}
		else{
			YouLoseText.gameObject.renderer.enabled = true;
		}
		yield return new WaitForSeconds(1);
		Application.LoadLevel(Application.loadedLevel);
	}

	void rocketDeath(){
		Debug.Log ("lose");
		StartCoroutine(delay (false));
	}

	void win(){
		Debug.Log ("win");
		StartCoroutine(delay (true));
	}

	void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.tag == "enemy") {
			rocketDeath ();
		}
		if (col.gameObject.name == "platform") {
			//Debug.Log ("win " + vel.magnitude + " " + Mathf.Abs(transform.eulerAngles.z) );
			if (vel.magnitude < allowedSpeed 
			    && (Mathf.Abs(transform.eulerAngles.z) < allowedAngle || Mathf.Abs(transform.eulerAngles.z) > (360 - allowedAngle))) {
				win();
			}
			else{
				rocketDeath ();
			}
		}
	}

	bool detectSwipe(Touch touch) {
		swipeDist[touch.fingerId] = 0;
		if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved) {
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);
			if(hit.collider != null){
				GameObject recipient = hit.transform.gameObject;
				if (recipient.name == "Thruster"){
					initialTouchPos[touch.fingerId] = touch.position;
					touchThruster = true;
				}
				else if(recipient.name == "Gun") {
					initialTouchPos[touch.fingerId] = touch.position;
					touchGun = true;
				}
			}
		}
		else if (touch.phase == TouchPhase.Ended){
			swipeDist[touch.fingerId] = (new Vector2(0, touch.position.y) - new Vector2(0, initialTouchPos[touch.fingerId].y)).magnitude;
		}
		return swipeDist[touch.fingerId] > minSwipeDist;
	}

	bool detectTwoFingerSwipe() {
		foreach (Touch touch in Input.touches) {
			bool swipe = detectSwipe(touch);
			if (touchGun && touchThruster && swipe){
				touchGun = false;
				touchThruster = false;
				return true;
			}
		}
		return false;
	}

	void rotate180() {
		if (flipped == 180)
			flipped = 0;
		else if (flipped == 0)
			flipped = 180;
	}

	void FixedUpdate () {

/*#if UNITY_EDITOR
		if(Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)) {
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if(hit.collider != null){
				GameObject recipient = hit.transform.gameObject;;
				if (recipient.name == "Thruster"){
					acc += thrusterSpeed*transform.up;
				}
				else if(recipient.name == "Gun") {
					if (Input.GetMouseButtonDown(0)) {
						acc -= thrusterSpeed*transform.up;
					}
				}
			}
		}

#endif*/
		if(Input.touchCount > 0){
			bool swiped = false;
			if (Input.touchCount == 2)
				swiped = detectTwoFingerSwipe();
			if(swiped){
				rotate180();
			}
			else{
				foreach (Touch touch in Input.touches) {
					translateRocket(Camera.main.ScreenToWorldPoint(touch.position));
				}
			}

		}
		vel = rigidbody2D.velocity;
		rotateRocket (Input.acceleration.x);
		//updateKinematics();

		if (Camera.main.WorldToScreenPoint (transform.position).x < 0
						|| Camera.main.WorldToScreenPoint (transform.position).x > Screen.width
		    			|| Camera.main.WorldToScreenPoint (transform.position).y < 0
		    			|| Camera.main.WorldToScreenPoint (transform.position).y > Screen.height ) {
			rocketDeath();
		}
	}
}
