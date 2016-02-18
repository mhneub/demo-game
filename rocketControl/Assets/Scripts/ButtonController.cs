using UnityEngine;
using System.Collections;

public class ButtonController : MonoBehaviour {

	public Sprite redButton;
	public Sprite yellowButton;

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<SpriteRenderer> ().sprite = redButton;
	}
	
	// Update is called once per frame
	void Update () {
		foreach(Touch touch in Input.touches){
			if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved) {
				RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero);
				if(hit.collider != null){
					GameObject recipient = hit.transform.gameObject;
					if (recipient == gameObject){
						gameObject.GetComponent<SpriteRenderer> ().sprite = yellowButton;
					}

				}
			}
			else if (touch.phase == TouchPhase.Ended){
				gameObject.GetComponent<SpriteRenderer> ().sprite = redButton;
			}
		}
	}
}
