using UnityEngine;
using System.Collections;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class FaceCamera : MonoBehaviour {
		void LateUpdate () {
			transform.LookAt (Camera.main.transform.position);
			transform.Rotate(new Vector3(0, 180, 0));
		}
	}
}
