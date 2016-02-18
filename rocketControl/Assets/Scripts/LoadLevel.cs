using UnityEngine;
using System.Collections;

public class LoadLevel : MonoBehaviour {

	public GameObject loadingImage;

	void Start(){
		Screen.orientation = ScreenOrientation.LandscapeRight;
	}
	
	public void LoadScene(int level)
	{
		loadingImage.SetActive(true);
		Application.LoadLevel(level);
	}
}
