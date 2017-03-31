using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Lobby manager. 
	/// Handles the tutorial canvas, and the number of Ready players before entering a game.
	/// </summary>
	public class LobbyManager : Photon.PunBehaviour {
		
		#region Public Variables


		[Tooltip("The Panel used to display who is ready to go")]
		public Transform readyPanel;
		[Tooltip("The center of the world around which players are spawned")]
		public Transform fireCamp;

		public int nbReadyNeeded;

		public bool isDebugging = false;


		#endregion


		#region Private Variables


		Text _howManyReady;


		#endregion


		#region MonoBehaviour CallBacks


		void Start () {
			if (isDebugging)
				nbReadyNeeded = 2;
			else
				nbReadyNeeded = 6;

			ChatManager.RoomName = PhotonNetwork.room.Name;

			_howManyReady = readyPanel.GetChild(2).GetComponent<Text>();

			Button[] swapGenderButtons = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild(4).GetChild(2).GetComponentsInChildren<Button>();
			swapGenderButtons [0].onClick.AddListener (delegate {
				PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().SwapGender ();
			});
			swapGenderButtons [1].onClick.AddListener (delegate {
				PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().SwapGender ();
			});

			if (PhotonNetwork.room.PlayerCount > 1)
				ResetPlayerPosition ();			
		}

		void Update () {
			_howManyReady.text = "";
			int nbReady = 0;

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				DontDestroyOnLoad (player);
				if (player.GetComponent<PlayerManager> ().role == "Ready")					
					nbReady++;
			}

			if (PhotonNetwork.inRoom) {
				_howManyReady.text = nbReady + " / ";
				if (PhotonNetwork.room.PlayerCount >= nbReadyNeeded)
					_howManyReady.text += PhotonNetwork.room.PlayerCount;
				else
					_howManyReady.text += nbReadyNeeded;
			}

			if(isDebugging) {
				if(PlayerManager.LocalPlayerInstance != null && PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().role != "Ready")
					SetReadyStatus ();
			}
	
			if (PhotonNetwork.isMasterClient && nbReady >= nbReadyNeeded && nbReady == PhotonNetwork.room.PlayerCount) {			
				//We lock the room and start the game!
				PhotonNetwork.room.IsOpen = false;
				PhotonNetwork.room.IsVisible = false;
				PhotonNetwork.LoadLevel ("Main");
			}
		}

		void LateUpdate () {
			if (PhotonNetwork.inRoom && PlayerManager.LocalPlayerInstance == null) {
				int playerRank = 0;
				float angle, x, z;
				do {
					playerRank++;
					angle = (playerRank * 2f * Mathf.PI / PhotonNetwork.room.MaxPlayers); // get the angle for this step (in radians, not degrees)
					x = Mathf.Cos (angle) * 6f;
					z = Mathf.Sin (angle) * 6f;
				} while(IsPlayerPositionAvailable(x, z));

				Vector3 positionOnCircle = new Vector3(x, 3f, z);
				positionOnCircle += fireCamp.position; 
				GameObject player = PhotonNetwork.Instantiate ("Player", positionOnCircle, Quaternion.identity, 0);
				player.transform.LookAt (-fireCamp.position);
				player.transform.rotation = new Quaternion (0, player.transform.rotation.y, 0, player.transform.rotation.w);	

				PlayerManager.LocalPlayerInstance = player;
			}
		}
			
		public override void OnLeftRoom()
		{
			SceneManager.LoadScene(0);
		}


		#endregion


		#region Custom


		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		/// <summary>
		/// This event is triggered when the local player clicks on the button "Ready".
		/// </summary>
		public void SetReadyStatus() {
			Text readyText = readyPanel.GetChild (1).GetComponentInChildren<Text> ();
			PlayerManager pM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();

			if (pM.role == "Ready") {
				readyText.text = "No !";
				readyText.color = Color.red;
				readyPanel.transform.parent.GetChild (0).gameObject.SetActive (true);
				pM.role = "";
			} else {
				readyText.text = "Yes !";
				readyText.color = Color.green;
				readyPanel.transform.parent.GetChild (0).gameObject.SetActive (false);
				pM.role = "Ready";
			}
		}

		/// <summary>
		/// This function is called when the master client decided to return to lobby after the game finished.
		/// </summary>
		void ResetPlayerPosition() {
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			int playerRank = 0;
			foreach (GameObject player in players) {
				if (player == PlayerManager.LocalPlayerInstance) {
					float angle = (playerRank * 2f * Mathf.PI / PhotonNetwork.room.MaxPlayers); // get the angle for this step (in radians, not degrees)
					float x = Mathf.Cos (angle) * 6f;
					float z = Mathf.Sin (angle) * 6f;

					Vector3 positionOnCircle = new Vector3 (x, 3f, z);
					player.transform.position = positionOnCircle + fireCamp.position;
				}
				playerRank++;
			}
		}

		/// <summary>
		/// This little function compares the position of the local player to any player that can be in the same spot.
		/// </summary>
		bool IsPlayerPositionAvailable(float x, float z) {
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (Mathf.Approximately (player.transform.position.x, x) && Mathf.Approximately (player.transform.position.z, z))
					return true;
			}
			return false;
		}


		#endregion
	}
}
