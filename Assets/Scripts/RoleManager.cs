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


		void Start () {
			if (PhotonNetwork.isMasterClient) {
				List<int> indexOfWerewolves = new List<int>();
				for(int j = 0; j <= Mathf.RoundToInt(PhotonNetwork.room.PlayerCount / 3) - 1; j++) {
					int randInt = Random.Range (0, PhotonNetwork.room.PlayerCount);
					while(indexOfWerewolves.Contains (randInt))
						randInt = Random.Range (0, PhotonNetwork.room.PlayerCount);
					indexOfWerewolves.Add (randInt);
				}

				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				for (int i = 0; i < players.Length; i++) {
					string role;
					if (indexOfWerewolves.Contains (i))
						role = "Werewolf";
					else
						role = "Villager";					
					players [i].GetComponent<PhotonView> ().RPC("SetPlayerRoleAndTent", PhotonTargets.All, new object[] { role, tents.GetChild(i).name });
				}
			}
		}


		
		// Update is called once per frame
		void Update () {
			Text infoText = GetComponentInChildren<Text> ();
			infoText.text = "Here's the state of the village:\n~~~~~~~~~~~~~~~~\n";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				infoText.text += PlayerManager.GetProperName (player.name);
				if (pM.isAlive)
					infoText.text += "[Alive] > Unknown\n";
				else
					infoText.text += "[Dead] > " + pM.role + "\n";
			}
		}


		#endregion
	}
}
