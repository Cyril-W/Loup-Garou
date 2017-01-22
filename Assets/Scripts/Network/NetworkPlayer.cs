using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : Photon.MonoBehaviour {

	bool isAlive = true;
	Vector3 position;
	Quaternion rotation;
	float lerpSmoothing = 10.0f;

	// Use this for initialization
	void Start () {
		if (photonView.isMine) {
			GetComponent<MovePlayer> ().enabled = true;
			SmoothCameraFollow.target = transform;
			Minimap.playerPos = transform;
			Renderer[] rends = GetComponentsInChildren<Renderer> ();
			rends[1].material.color = Color.red;
			rends[2].material.color = Color.red;
			GetComponent<MinimapObjectID> ().image.color = Color.red;
		} else {
			StartCoroutine ("Alive");
			Renderer[] rends = GetComponentsInChildren<Renderer> ();
			rends[1].material.color = Color.blue;
			rends[2].material.color = Color.blue;
			GetComponent<MinimapObjectID> ().image.color = Color.blue;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext (transform.position);
			stream.SendNext (transform.rotation);
		} else {
			position = (Vector3)stream.ReceiveNext ();
			rotation = (Quaternion)stream.ReceiveNext ();
		}
	}

	IEnumerator Alive () {
		while (isAlive) {
			transform.position = Vector3.Lerp (transform.position, position, Time.deltaTime * lerpSmoothing);
			transform.rotation = Quaternion.Lerp (transform.rotation, rotation, Time.deltaTime * lerpSmoothing);
		
			yield return null;
		}
	}
}
