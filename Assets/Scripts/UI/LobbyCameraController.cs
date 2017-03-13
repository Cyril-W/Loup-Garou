using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Lobby camera controller. 
	/// Handles the position of the camera during all the phases of the lobby tutorial.
	/// </summary>
	public class LobbyCameraController : MonoBehaviour {

		#region Public Variables


		[Tooltip("Prefab of the image used to show the position of the gameObject on the minimap")]
		public Transform currentTarget;
		[Tooltip("Prefab of the image used to show the position of the gameObject on the minimap")]
		public float speedFactor = 0.1f;
		[Tooltip("Prefab of the image used to show the position of the gameObject on the minimap")]
		public float zoomFactor = 1.0f;


		#endregion


		#region Private Variables


		Vector3 _lastPosition;


		#endregion


		#region MonoBehaviour CallBacks


		void Start () {
			_lastPosition = transform.position;
		}
		
		void Update () {
			transform.position = Vector3.Lerp (transform.position, currentTarget.position, speedFactor);
			transform.rotation = Quaternion.Slerp (transform.rotation, currentTarget.rotation, speedFactor);

			float velocity = Vector3.Magnitude (transform.position - _lastPosition);
			Camera.main.fieldOfView = 60 + velocity * zoomFactor;

			_lastPosition = transform.position;
		}


		#endregion


		#region Custom


		public void SetTarget(Transform target) {
			currentTarget = target;
		}


		#endregion
	}
}
