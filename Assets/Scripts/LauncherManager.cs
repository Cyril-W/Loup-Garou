using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Launcher manager. 
	/// Handles the setting of the player name, the manual and the random connection.
	/// </summary>
	public class LauncherManager : Photon.PunBehaviour {
		
		#region Public Variables


		[Tooltip("The UI Canvas to handle the chat system")]
		public GameObject chatCanvas;

		/// <summary>
		/// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
		/// </summary>   
		public byte MaxPlayersPerRoom = 20;

		/// <summary>
		/// The PUN loglevel. 
		/// </summary>
		public PhotonLogLevel Loglevel = PhotonLogLevel.ErrorsOnly;

		public bool isDebugging = false;


		#endregion


		#region Private Variables
	

		/// <summary>
		/// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
		/// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
		/// Typically this is used for the OnConnectedToMaster() callback.
		/// </summary>
		bool isConnecting;

		/// <summary>
		/// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
		/// </summary>
		string _gameVersion = "2";


		#endregion


		#region MonoBehaviour CallBacks


		void Awake()
		{
			// #NotImportant
			// Force LogLevel
			PhotonNetwork.logLevel = Loglevel;

			// #Critical
			// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
			PhotonNetwork.autoJoinLobby = false;


			// #Critical
			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			PhotonNetwork.automaticallySyncScene = true;
		}
			
		void Start()
		{
			if (!isDebugging) {
				if (GameObject.FindGameObjectWithTag ("Chat") != null)
					OnOkPressed ();
			}
			
			Connect ();
		}
			
		void Update() 
		{
			if (isDebugging) {
				if (PhotonNetwork.connectedAndReady && !isConnecting) {
					OnOkPressed ();
					RandomConnect ();
				}
			}
		}


		#endregion


		#region Custom


		/// <summary>
		/// Used the set the chat connection process with a proper username. 
		/// </summary>
		public void OnOkPressed()
		{
			transform.GetChild(0).GetChild (1).gameObject.SetActive (true);
			transform.GetChild(0).GetChild (2).gameObject.SetActive (true);
			if (transform.GetChild (0).GetChild (0).GetChild (2).gameObject.activeSelf == true) {
				InputField inputField = transform.GetChild (0).GetChild (0).GetComponentInChildren<InputField> ();
				inputField.text += Random.Range (1000, 9999).ToString();
				inputField.DeactivateInputField ();
				inputField.interactable = false;
				if (GameObject.FindGameObjectWithTag ("Chat") == null)
					Instantiate (chatCanvas);
				else
					inputField.text = ChatManager.UserName;
				transform.GetChild (0).GetChild (0).GetChild (2).gameObject.SetActive (false);
			}
		}

		/// <summary>
		/// Start the connection process. 
		/// - If already connected, we wait for the choice of the room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect()
		{
			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (!PhotonNetwork.connected) {
				// #Critical, we must first and foremost connect to Photon Online Server.
				PhotonNetwork.ConnectUsingSettings (_gameVersion);
			}

			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(false);
		}

		/// <summary>
		/// Start the random connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void RandomConnect()
		{
			// keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
			isConnecting = true;

			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (!PhotonNetwork.connected) {
				Connect ();
			} else {
				transform.GetChild(0).gameObject.SetActive (false);
				transform.GetChild(1).gameObject.SetActive (true);

				// #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
				PhotonNetwork.JoinRandomRoom ();
			}
		}

		/// <summary>
		/// Start the manual connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void ManualConnect()
		{
			// keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
			isConnecting = true;

			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (!PhotonNetwork.connected)
			{
				Connect ();
			} else {
				string roomName = transform.GetChild(0).GetChild(1).GetComponentInChildren<InputField>().text;

				transform.GetChild(0).gameObject.SetActive (false);
				transform.GetChild(1).gameObject.SetActive (true);

				// #Critical we need at this point to attempt joining an existing and named Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
				PhotonNetwork.JoinOrCreateRoom (roomName, new RoomOptions() { IsVisible = false, MaxPlayers = MaxPlayersPerRoom }, null);
			}
		}

		public override void OnConnectedToMaster()
		{
			// we don't want to do anything if we are not attempting to join a room. 
			// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
			// we don't want to do anything.
			if (isConnecting)
			{
				transform.GetChild(0).gameObject.SetActive(true);
				transform.GetChild(1).gameObject.SetActive(false);
			}
		}


		public override void OnDisconnectedFromPhoton()
		{
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(false);      
		}

		public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
		{
			// #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
			PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
		}

		public override void OnJoinedRoom()
		{
			// #Critical
			// Load the Lobby Level. 
			PhotonNetwork.LoadLevel("Lobby");
		}


		#endregion
	}
}
