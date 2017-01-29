using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Lobby manager. 
	/// Handles the status "Ready" of the player, which is synced by Photon.
	/// </summary>
	public class LobbyManager : MonoBehaviour {

		#region Public Variables


		[Tooltip("The Panel used to display who is ready to go")]
		public Transform readyPanel;
		[Tooltip("The number of people you need to gather before starting to play")]
		public int nbReadyNeeded = 2;


		#endregion


		#region Private Variables


		static Text _whoReady;
		static int _nbReadyNeeded;


		#endregion


		#region MonoBehaviour CallBacks


		// Use this for initialization
		void Start () {
			_whoReady = readyPanel.GetChild(2).GetComponentInChildren<Text>();
			_nbReadyNeeded = nbReadyNeeded;

			Button readyButton = readyPanel.GetChild (1).GetComponent<Button> ();
			readyButton.onClick.AddListener (delegate { PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().SetReadyStatus (); });
		}

		// Update is called once per frame
		void Update () {
			if (LobbyManager.RefreshWho ()) {
				//We lock the room and start the game!
				//PhotonNetwork.room.ClearExpectedUsers ();
				PhotonNetwork.room.IsOpen = false;
				PhotonNetwork.room.IsVisible = false;
				PhotonNetwork.LoadLevel ("Main");
			}
		}


		#endregion


		#region Custom


		/// <summary>
		/// Searches through all the players, checks if they are ready or not.
		/// </summary>
		public static bool RefreshWho () {
			int nbReady = 0;
			_whoReady.text = "";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player.GetComponent<PlayerManager> ().isReady) {
					_whoReady.text += "~ " + PlayerManager.GetProperName(player.name) + ": YES!\n";
					nbReady++;
				}
				else
					_whoReady.text += "~ " + PlayerManager.GetProperName(player.name) + ": NO...\n";
			}

			return (nbReady >= _nbReadyNeeded)? true : false;
		}


		#endregion
	}
}
