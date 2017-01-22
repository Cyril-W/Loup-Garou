using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {
	
	void LateUpdate () {
		transform.LookAt (Camera.main.transform.position);
		transform.Rotate (new Vector3 (0, 180, 0));
	}
}
