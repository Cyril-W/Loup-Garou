using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Role manager. 
	/// Handles roles of players.
	/// </summary>
	public class RoleManager : Photon.PunBehaviour {
		#region Public Variables


		[Tooltip("The Transform containing all the tents")]
		public Transform tents;


		#endregion


		#region MonoBehaviour CallBacks


		void Awake () {	
		}

		void Start () {
			if (PhotonNetwork.isMasterClient) {
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				for (int i = 0; i < players.Length; i++) {
					
					players [i].GetComponent<PhotonView>().RPC("SetPlayerRoleAndTent", PhotonTargets.All, new object[] { "villager", tents.GetChild(i).name });
				}
			}
		}


		
		// Update is called once per frame
		void Update () {
			Text text = GetComponentInChildren<Text> ();
			text.text = "Here are all the roles:\n~~~~~~~~~~~~~~~~\n";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				pM.role = "villager";
				text.text += PlayerManager.GetProperName(player.name) + ": " + pM.role + "\n";
			}
		}


		#endregion
	}
}
