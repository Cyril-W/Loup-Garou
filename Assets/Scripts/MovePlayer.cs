using UnityEngine;
using System.Collections;

public class MovePlayer : MonoBehaviour {

	public float speed = 10.0f;
	public float rotationSpeed = 100.0f;
	public float verticalSpeed = 5.0f;
	float distToGround;

	Rigidbody rb;

	private float walkingSpeed;
	private float runningSpeed;
	//private Animator anim;

	void Start()
	{
		// anim = GetComponentInChildren<Animator>();
		walkingSpeed = speed;
		runningSpeed = 2 * speed;
		rb = GetComponent<Rigidbody>();
		distToGround = GetComponent<Collider>().bounds.extents.y;
	}

	// Update is called once per frame
	void FixedUpdate () {
		float translation = Input.GetAxis ("Vertical") * speed;
		float rotation = Input.GetAxis ("Horizontal") * rotationSpeed;

		if (translation != 0 || rotation != 0) {
			translation *= Time.deltaTime;
			rotation *= Time.deltaTime;
			transform.Translate (0, 0, translation);
			transform.Rotate (0, rotation, 0);
			// anim.SetBool ("isWalking", true);
		} else {
			// anim.SetBool ("isWalking", false);
		}
	}

	void Update ()
	{
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

	bool IsGrounded() {
		return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.05f);
	}

	void Jump()	{
		if(IsGrounded())
		{
			// anim.SetTrigger("jump");
			rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			rb.AddForce(Vector3.up * verticalSpeed, ForceMode.Impulse);
		}
	}
}
