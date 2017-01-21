using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		transform.LookAt (Camera.main.transform.position);
		transform.Rotate (new Vector3 (0, 180, 0));
	}
}
