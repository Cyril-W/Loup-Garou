using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Lobby manager. 
	/// Handles the status number of ready, which is synced by Photon.
	/// </summary>
	public class LobbyManager : Photon.PunBehaviour {
		#region Public Variables


		[Tooltip("The Panel used to display who is ready to go")]
		public Transform readyPanel;
		[Tooltip("The Sprite used to display when you're ready to go")]
		public Sprite readySprite;
		[Tooltip("The Panel used to display when you're not ready to go")]
		public Sprite notReadySprite;
		[Tooltip("The center of the world around which players are spawned")]
		public Transform fireCamp;
		[Tooltip("The number of people you need to gather before starting to play")]
		public int nbReadyNeeded = 3;


		#endregion


		#region Private Variables


		Text _whoReady;


		#endregion


		#region MonoBehaviour CallBacks


		// Use this for initialization
		void Start () {
			ChatManager.RoomName = PhotonNetwork.room.Name;

			_whoReady = readyPanel.GetChild(2).GetComponentInChildren<Text>();

			int numberOfSpawn = PhotonNetwork.room.MaxPlayers;
			int rankOfPlayer = PhotonNetwork.player.ID;
			float angle = (rankOfPlayer * 2f * Mathf.PI / numberOfSpawn); // get the angle for this step (in radians, not degrees)
			float x = Mathf.Cos(angle) * rankOfPlayer* 6f;
			float z = Mathf.Sin(angle) * rankOfPlayer * 6f;
			Vector3 positionOnCircle = new Vector3(x, 3f, z);
			positionOnCircle += fireCamp.position; 
			GameObject player = PhotonNetwork.Instantiate ("Player", positionOnCircle, Quaternion.identity, 0);
			player.transform.LookAt (fireCamp.position);
			player.transform.rotation = new Quaternion (0, player.transform.rotation.y, 0, player.transform.rotation.z);	
		}

		// Update is called once per frame
		void Update () {
			_whoReady.text = "";
			int nbReady = 0;

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player.GetComponent<PlayerManager> ().isReady) {
					DontDestroyOnLoad (player);
					nbReady++;	
				}
			}

			if (PhotonNetwork.inRoom)
				_whoReady.text = nbReady + " / " + PhotonNetwork.room.PlayerCount;
	
			if (PhotonNetwork.isMasterClient && nbReady >= nbReadyNeeded && nbReady == PhotonNetwork.room.PlayerCount) {			
				//We lock the room and start the game!
				//PhotonNetwork.room.ClearExpectedUsers ();
				PhotonNetwork.room.IsOpen = false;
				PhotonNetwork.room.IsVisible = false;
				PhotonNetwork.LoadLevel ("Main");
			}
		}

		/// <summary>
		/// Called when the local player left the room. We need to load the launcher scene.
		/// </summary>
		public override void OnLeftRoom()
		{
			SceneManager.LoadScene(0);
		}


		#endregion


		#region Custom


		public void LeaveRoom()
		{
			PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().isReady = false;
			PhotonNetwork.LeaveRoom();
		}

		public void SetReadyStatus() {
			Button readyButton = readyPanel.GetChild (1).GetComponentInChildren<Button> ();
			PlayerManager pM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();

			if (pM.isReady) {
				readyButton.image.sprite = notReadySprite;
				readyButton.image.color = Color.red;
				readyButton.GetComponentInChildren<Text> ().text = "No > Yes";
			} else {
				readyButton.image.sprite = readySprite;
				readyButton.image.color = Color.green;
				readyButton.GetComponentInChildren<Text> ().text = "Yes > No";
			}

			pM.isReady = !(pM.isReady);
		}


		#endregion



	}
}
