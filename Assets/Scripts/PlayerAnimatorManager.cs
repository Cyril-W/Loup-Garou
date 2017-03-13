using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Player Animator manager. 
	/// Handles movements of the local player.
	/// </summary>
	public class PlayerAnimatorManager : Photon.MonoBehaviour {
		
		#region Public Variables


		[System.Serializable]
		public class MoveSettings
		{
			public float forwardVel = 12f;
			public float strafVel = 10f;
			public float rotateVel = 100f;
			public float jumpVel = 10f;
			public float distToGrounded = 1.1f;
			public LayerMask ground;
		}

		[System.Serializable]
		public class PhysSettings
		{
			public float downAccel = 0.5f;
		}

		[System.Serializable]
		public class InputSettings
		{
			public float inputDelay = 0.1f;
		}

		public MoveSettings moveSetting = new MoveSettings();
		public PhysSettings physSetting = new PhysSettings();
		public InputSettings inputSetting = new InputSettings();

		public Quaternion TargetRotation {
			get { return targetRotation; }
		}

		public static bool isBlocked = false;
		public static Transform compas;


		#endregion


		#region Private Variables


		Vector3 velocity = Vector3.zero;
		Quaternion targetRotation;
		Rigidbody rBody;
		Animator anim;
		float forwardInput, turnInput, strafInput, jumpInput;


		#endregion


		#region MonoBehavior CallBacks


		void Start () 
		{
			anim = GetComponent<Animator>();
			if (!anim)
				Debug.LogWarning("PlayerAnimatorManager is Missing Animator Component",this);
			targetRotation = transform.rotation;
			if (GetComponent<Rigidbody> ())
				rBody = GetComponent<Rigidbody> ();
			else
				Debug.Log ("No rigidbody attached to the player!");

			forwardInput = turnInput = strafInput = jumpInput = 0;
		}
			
		void Update ()
		{
			if ((photonView.isMine == false && PhotonNetwork.connected == true) || isBlocked)
				return;
			
			GetInput ();
			Turn ();
		}
					
		void FixedUpdate () 
		{
			if ((photonView.isMine == false && PhotonNetwork.connected == true) || isBlocked)
				return;
			
			Run ();
			Jump ();

			rBody.velocity = transform.TransformDirection (velocity);
		}


		#endregion


		#region Custom


		void GetInput () {
			forwardInput = Input.GetAxis ("Vertical");
			turnInput = Input.GetAxis ("Horizontal");
			strafInput = Input.GetAxis ("Straf");
			jumpInput = Input.GetAxisRaw ("Jump"); //non-interpolated
		}

		void Run () {
			if (Mathf.Abs (forwardInput) > inputSetting.inputDelay) {
				velocity.z = moveSetting.forwardVel * forwardInput;
				if(!anim)
					anim.SetBool ("isWalking", true);
			} else {
				velocity.z = 0;
				if(!anim)
					anim.SetBool ("isWalking", false);
			}

			if (Mathf.Abs (strafInput) > inputSetting.inputDelay)
				velocity.x = moveSetting.strafVel * strafInput;
			else
				velocity.x = 0;
		}

		void Turn () {
			if (Mathf.Abs (turnInput) > inputSetting.inputDelay) 
				targetRotation *= Quaternion.AngleAxis (moveSetting.rotateVel * turnInput * Time.deltaTime, Vector3.up);
			transform.rotation = targetRotation;
			if(!compas)
				compas.rotation = targetRotation;
		}

		void Jump () {
			if (jumpInput > 0 && Grounded ())
				velocity.y = moveSetting.jumpVel;
			else if (jumpInput == 0 && Grounded ())
				velocity.y = 0;
			else
				velocity.y -= physSetting.downAccel;
		}

		bool Grounded () {
			return Physics.Raycast (transform.position, Vector3.down, moveSetting.distToGrounded, moveSetting.ground);
		}


		#endregion
	}
}
