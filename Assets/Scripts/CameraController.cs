using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Camera controller. 
	/// Handles the rotation of the camera following the local player.
	/// </summary>
	public class CameraController : MonoBehaviour {

		#region Public Variables 

		/// <summary>
		/// The local player the camera should follow.
		/// </summary>
		public Transform target;

		/// <summary>
		/// Helps to structure the variables, especially in Unity's Inspector.
		/// </summary>
		[System.Serializable]
		public class PositionSettings {
			public Vector3 targetPosOffset = new Vector3 (0, 1f, 0);
			public float lookSmooth = 100f;
			public float distanceFromTarget = -8f;
			public float zoomSmooth = 100f;
			public float maxZoom = -2f;
			public float minZoom = -15f;
		}

		/// <summary>
		/// Helps to structure the variables, especially in Unity's Inspector.
		/// </summary>
		[System.Serializable]
		public class OrbitSettings {
			public float xRotation = -20f;
			public float yRotation = -180f;
			public float maxXRotation = 25f;
			public float minXRotation = -85f;
			public float vOrbitSmooth = 150f;
			public float hOrbitSmooth = 150f;
		}

		/// <summary>
		/// Contains the variables concerning the position of the camera.
		/// </summary>
		public PositionSettings positionSetting = new PositionSettings ();
		/// <summary>
		/// Contains the variables concerning how the camera should rotate around the target.
		/// </summary>
		public OrbitSettings orbitSetting = new OrbitSettings ();


		#endregion


		#region Private Variables


		Vector3 _targetPos = Vector3.zero;
		Vector3 _destination = Vector3.zero;
		float _vOrbitInput, _hOrbitInput, _zoomInput, _hOrbitSnapInput;


		#endregion


		#region Monobehaviour Calls


		void Start () {
			SetCameraTarget (PlayerManager.LocalPlayerInstance.transform);

			MoveToTarget ();
		}

		void Update () {
			// if the player hold the right mouse button, orbit the camera with the mouse
			if (Input.GetMouseButton (1))
				GetInput (true);
			else // orbit the camera with the numpad
				GetInput (false);
			
			OrbitTarget ();
			ZoomInOnTarget ();
		}

		void LateUpdate () {
			MoveToTarget ();
			LookAtTarget ();
		}


		#endregion


		#region Custom 


		void GetInput (bool isManual) {
			if(isManual) {			
				_vOrbitInput = -Input.GetAxisRaw ("Mouse Y");
				_hOrbitInput = -Input.GetAxisRaw ("Mouse X");
			} else {
				_vOrbitInput = Input.GetAxisRaw ("OrbitVertical");
				_hOrbitInput = Input.GetAxisRaw ("OrbitHorizontal");
			}
			_hOrbitSnapInput = Input.GetAxisRaw ("Fire3");
			_zoomInput = Input.GetAxisRaw ("Mouse ScrollWheel");
		}

		void SetCameraTarget (Transform t) {
			target = t;

			if (target == null)
				Debug.Log ("No target assigned!");
		}

		void MoveToTarget () {
			if (target != null) {
				_targetPos = target.position + positionSetting.targetPosOffset;
				_destination = Quaternion.Euler (orbitSetting.xRotation, orbitSetting.yRotation + target.eulerAngles.y, 0f) * -Vector3.forward * positionSetting.distanceFromTarget;
				_destination += target.position;
				transform.position = _destination;
			}
		}

		void LookAtTarget () {
			Quaternion targetRotation = Quaternion.LookRotation (_targetPos - transform.position);
			transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, positionSetting.lookSmooth * Time.deltaTime);
		}

		void OrbitTarget () {
			if (_hOrbitSnapInput > 0)
				orbitSetting.yRotation = -180f;

			orbitSetting.xRotation += -_vOrbitInput * orbitSetting.vOrbitSmooth * Time.deltaTime;
			orbitSetting.yRotation += -_hOrbitInput * orbitSetting.hOrbitSmooth * Time.deltaTime;
		
			orbitSetting.xRotation = Mathf.Clamp (orbitSetting.xRotation, orbitSetting.minXRotation, orbitSetting.maxXRotation);
		}

		void ZoomInOnTarget () {
			positionSetting.distanceFromTarget += _zoomInput * positionSetting.zoomSmooth * Time.deltaTime;

			positionSetting.distanceFromTarget = Mathf.Clamp (positionSetting.distanceFromTarget, positionSetting.minZoom, positionSetting.maxZoom);
		}


		#endregion
	}
}
