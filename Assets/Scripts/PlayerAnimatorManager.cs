using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class PlayerAnimatorManager : Photon.MonoBehaviour 
	{
		#region Public Variables

		[Tooltip("The current Speed of our player")]
		public float speed = 10.0f;
		public float rotationSpeed = 100.0f;
		public float verticalSpeed = 5.0f;

		[Tooltip("The current state of the player")]
		public static bool isBlocked = false;

		#endregion

		#region Private Variables

		private float distToGround;
		private Rigidbody rb;
		private float walkingSpeed; //used to store the values of different speeds
		private float runningSpeed; //in order to switch to walk/run easily
		private Animator anim;

		#endregion

		#region MonoBehavior CallBacks


		// Use this for initialization
		void Start () 
		{
			anim = GetComponent<Animator>();
			if (!anim)
				Debug.LogWarning("PlayerAnimatorManager is Missing Animator Component",this);
			walkingSpeed = speed;
			runningSpeed = 2 * speed;
			rb = GetComponent<Rigidbody>();
			distToGround = GetComponent<Collider>().bounds.extents.y;
		}

		// Update is called once per frame
		void Update ()
		{
			if ((photonView.isMine == false && PhotonNetwork.connected == true) || isBlocked)
				return;
			
			//Jump
			if(Input.GetKeyDown(KeyCode.Space))
			{
				Jump();
			}

			//Run
			if (Input.GetKey (KeyCode.LeftShift)) 
			{
				if(IsGrounded())
					speed = runningSpeed;
			}
			else
				speed = walkingSpeed;
		}
					
		void FixedUpdate () 
		{
			if ((photonView.isMine == false && PhotonNetwork.connected == true) || isBlocked)
				return;
			
			float translation = Input.GetAxis ("Vertical") * speed;
			float rotation = Input.GetAxis ("Horizontal") * rotationSpeed;

			if (translation != 0 || rotation != 0) 
			{
				translation *= Time.deltaTime;
				rotation *= Time.deltaTime;
				transform.Translate (0, 0, translation);
				transform.Rotate (0, rotation, 0);
				if(!anim)
					anim.SetBool ("isWalking", true);
			} else 
			{
				if(!anim)
					anim.SetBool ("isWalking", false);
			}
		}


		#endregion

		#region Custom

		/// <summary>
		/// Checks if player touches the ground. Used as a flag representing when the player is on the ground.
		/// </summary>
		bool IsGrounded() 
		{
			return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.05f);
		}

		/// <summary>
		/// Tries to jump. Uses the flag IsGrounded.
		/// </summary>
		void Jump()	
		{
			if(IsGrounded())
			{				
				rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
				rb.AddForce(Vector3.up * verticalSpeed, ForceMode.Impulse);
				if(!anim)
					anim.SetTrigger("jump");
			}
		}

		#endregion
	}
}
