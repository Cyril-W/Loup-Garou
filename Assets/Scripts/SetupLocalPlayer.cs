using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class SetupLocalPlayer : NetworkBehaviour {

	[SyncVar]
	public string pname = "Player";

	void OnGUI () {
		if(isLocalPlayer) {
			pname = GUI.TextField(new Rect(25, Screen.height - 40, 100, 30), pname);
		
			if(GUI.Button(new Rect(130, Screen.height - 40, 80, 30), "Change")) {
				CmdChangeName(pname);
			}
		}
	}

	[Command]
	public void CmdChangeName (string newName) {
		pname = newName;
	}

	// Use this for initialization
	void Start () {
		if(isLocalPlayer) {
			GetComponent<MovePlayer>().enabled = true;
			SmoothCameraFollow.target = transform;
		}			
	}

	void Update () {
		GetComponentInChildren<TextMesh>().text = pname;
	}
}
