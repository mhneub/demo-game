using UnityEngine;
using System.Collections;
using System.IO;

public class boundariesGenerator : MonoBehaviour {

	public GameObject boundary;
	public GameObject platform;
	public GameObject boundary_invisible;
	public float Z_EXTENT;
	public string pointsResourceName;
	public bool invertWindingOrder;

	// Use this for initialization
	void Start () {

		// get world location of bottom-left of background
		GameObject bg = GameObject.Find ("background");
		Vector3 bg_center = bg.renderer.bounds.center;
		Vector3 bg_extents = bg.renderer.bounds.extents;
		float bg_xmin = bg_center.x - bg_extents.x;
		float bg_ymin = bg_center.y - bg_extents.y;
		// get world size of background
		float bg_width = bg.renderer.bounds.size.x;
		float bg_height = bg.renderer.bounds.size.y;


		// read in points from file, convert to world coordinates
		int numpoints, platformIndex;
		Vector2[] points;

		TextAsset pointsFile = Resources.Load (pointsResourceName) as TextAsset;
		string[] lines = pointsFile.text.Split ("\n" [0]);
		numpoints = int.Parse (lines[0]);
		platformIndex = int.Parse (lines[1]);
		//Debug.Log ("n=" + numpoints + "  k=" + platformIndex);

		points = new Vector2[numpoints];
		for (int i = 0; i < numpoints; i++) {
			string[] words = lines[2+i].Split(" "[0]);
			float x = float.Parse (words[0]);
			float y = float.Parse (words[1]);
			//Debug.Log (x+" "+y);

			int index = invertWindingOrder ? numpoints - 1 - i : i;
			points[index] = new Vector2();
			points[index].x = bg_xmin + x / 1920f * bg_width;
			points[index].y = bg_ymin + y / 1080f * bg_height;
		}
		if (invertWindingOrder)
			platformIndex = numpoints - 2 - platformIndex;


		// construct mesh for mesh collider

		Vector3[] vertices = new Vector3[2*numpoints + 8];
		int[] triangles = new int[6*(numpoints-1) + 24];

		for (int i = 0; i < numpoints; i++) {
			vertices [i] = new Vector3 (points [i].x, points [i].y, -Z_EXTENT);
			vertices [numpoints + i] = new Vector3 (points [i].x, points [i].y, Z_EXTENT);
		}
		for (int i = 0; i < numpoints - 1; i++) {
			triangles[6*i] = i;
			triangles[6*i+1] = numpoints + i;
			triangles[6*i+2] = numpoints + i + 1;
			triangles[6*i+3] = i;
			triangles[6*i+4] = numpoints + i + 1;
			triangles[6*i+5] = i + 1;
		}

		// add planes for the screen boundary
		float camVolumeHalfHeight = Camera.main.orthographicSize;
		float camVolumeHalfWidth = camVolumeHalfHeight / Screen.height * Screen.width;
		float camCenterX = Camera.main.transform.position.x;
		float camCenterY = Camera.main.transform.position.y;

		int voffset = 2*numpoints;
		vertices [voffset+0] = new Vector3 (camCenterX - camVolumeHalfWidth, camCenterY - camVolumeHalfHeight, -Z_EXTENT);
		vertices [voffset+1] = new Vector3 (camCenterX + camVolumeHalfWidth, camCenterY - camVolumeHalfHeight, -Z_EXTENT);                         
		vertices [voffset+2] = new Vector3 (camCenterX + camVolumeHalfWidth, camCenterY + camVolumeHalfHeight, -Z_EXTENT);
		vertices [voffset+3] = new Vector3 (camCenterX - camVolumeHalfWidth, camCenterY + camVolumeHalfHeight, -Z_EXTENT);
		vertices [voffset+4] = new Vector3 (camCenterX - camVolumeHalfWidth, camCenterY - camVolumeHalfHeight, Z_EXTENT);
		vertices [voffset+5] = new Vector3 (camCenterX + camVolumeHalfWidth, camCenterY - camVolumeHalfHeight, Z_EXTENT);                         
		vertices [voffset+6] = new Vector3 (camCenterX + camVolumeHalfWidth, camCenterY + camVolumeHalfHeight, Z_EXTENT);
		vertices [voffset+7] = new Vector3 (camCenterX - camVolumeHalfWidth, camCenterY + camVolumeHalfHeight, Z_EXTENT);

		int toffset = 6 * (numpoints - 1);
		triangles [toffset + 0] = voffset + 0;
		triangles [toffset + 1] = voffset + 4;
		triangles [toffset + 2] = voffset + 5;
		triangles [toffset + 3] = voffset + 0;
		triangles [toffset + 4] = voffset + 5;
		triangles [toffset + 5] = voffset + 1;

		triangles [toffset + 6] = voffset + 1;
		triangles [toffset + 7] = voffset + 5;
		triangles [toffset + 8] = voffset + 6;
		triangles [toffset + 9] = voffset + 1;
		triangles [toffset + 10] = voffset + 6;
		triangles [toffset + 11] = voffset + 2;

		triangles [toffset + 12] = voffset + 2;
		triangles [toffset + 13] = voffset + 6;
		triangles [toffset + 14] = voffset + 7;
		triangles [toffset + 15] = voffset + 2;
		triangles [toffset + 16] = voffset + 7;
		triangles [toffset + 17] = voffset + 3;

		triangles [toffset + 18] = voffset + 3;
		triangles [toffset + 19] = voffset + 7;
		triangles [toffset + 20] = voffset + 4;
		triangles [toffset + 21] = voffset + 3;
		triangles [toffset + 22] = voffset + 4;
		triangles [toffset + 23] = voffset + 0;


		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;

		mesh.RecalculateNormals(); 
		mesh.RecalculateBounds(); 
		mesh.Optimize(); 

		GetComponent<MeshCollider> ().sharedMesh = mesh;



		// add 4 box colliders along the edges of the screen

		const float thickness = 1;

		// left
		GameObject bi_instance = Instantiate(boundary_invisible, 
		                                     new Vector3(camCenterX - camVolumeHalfWidth - thickness/2,
		            									camCenterY, 0f),
		                                     new Quaternion()) as GameObject;

		float bi_instance_width = bi_instance.collider2D.bounds.size.x;
		float bi_instance_height = bi_instance.collider2D.bounds.size.y;
		bi_instance.transform.localScale = new Vector3 (thickness / bi_instance_width,
		                                               2f * camVolumeHalfHeight / bi_instance_height,
		                                               1f);
		// right
		bi_instance = Instantiate(boundary_invisible, 
		                                     new Vector3(camCenterX + camVolumeHalfWidth + thickness/2,
		            									camCenterY, 0f),
		                                     new Quaternion()) as GameObject;
		bi_instance.transform.localScale = new Vector3 (thickness / bi_instance_width,
		                                                2f * camVolumeHalfHeight / bi_instance_height,
		                                                1f);

		// bottom
		bi_instance = Instantiate(boundary_invisible, 
		                                     new Vector3(camCenterX,
	            										camCenterY - camVolumeHalfHeight - thickness/2, 0f),
		                                     new Quaternion()) as GameObject;
		bi_instance.transform.localScale = new Vector3 (2f * camVolumeHalfWidth / bi_instance_width,
		                                                thickness / bi_instance_height,
		                                                1f);

		// top
		bi_instance = Instantiate(boundary_invisible, 
		                          new Vector3(camCenterX,
		            						 camCenterY + camVolumeHalfHeight + thickness/2, 0f),
		                          new Quaternion()) as GameObject;
		bi_instance.transform.localScale = new Vector3 (2f * camVolumeHalfWidth / bi_instance_width,
		                                                thickness / bi_instance_height,
		                                                1f);


		// instantiate boundaries and platform along the points

		GameObject b_instance = Instantiate(boundary, new Vector3(), new Quaternion()) as GameObject;
		float b_instance_width = b_instance.renderer.bounds.size.x;
		float b_instance_height = b_instance.renderer.bounds.size.y;
		GameObject.Destroy (b_instance);

		for (int i = 0; i < numpoints - 1; i++) {

			Vector3 center = new Vector3((points[i].x + points[i+1].x) / 2,
			                             (points[i].y + points[i+1].y) / 2,
			                             0f);
			float length = Vector2.Distance(points[i], points[i+1]);
			float angle = Mathf.Atan2(points[i+1].y - points[i].y,
			                          points[i+1].x - points[i].x)
				*180 / Mathf.PI;


			GameObject instance;
			float instance_width, instance_height;

			if (i == platformIndex) {
				instance = Instantiate(platform, center, new Quaternion()) as GameObject;
				instance.name = "platform";	// necessary for win condition in rocketController.cs

				instance_width = instance.renderer.bounds.size.x;
				instance_height = instance.renderer.bounds.size.y;

				Vector3 p = instance.transform.position;
				instance.transform.position = new Vector3(p.x, p.y - (instance_height-b_instance_height)/2, p.z);

			} else {
				instance = Instantiate(boundary, center, new Quaternion()) as GameObject;
				instance_width = b_instance_width;
				instance_height = b_instance_height;
			}

			Vector3 s = instance.transform.localScale;
			instance.transform.localScale = new Vector3(length / instance_width, s.y, s.z);
			instance.transform.Rotate(new Vector3(0f,0f,angle));
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
