using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SetupLocalPlayer : NetworkBehaviour {

	[SyncVar]
	public string pname = "Player";

	[SyncVar]
	public Color playerColor = Color.white;

	// Use this for initialization
	void Start () {
		if (isLocalPlayer) {
			GetComponent<MovePlayer> ().enabled = true;
			SmoothCameraFollow.target = transform;
		}

		Renderer[] rends = GetComponentsInChildren<Renderer> ();
		rends[1].material.color = playerColor;
		rends[2].material.color = playerColor;

		transform.position = new Vector3 (Random.Range (-20, 20), 1, Random.Range (-20, 20));
	}

	void Update () {
		GetComponentInChildren<TextMesh>().text = pname;
	}
}
