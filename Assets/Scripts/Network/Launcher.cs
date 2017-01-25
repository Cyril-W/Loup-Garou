﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class Launcher : Photon.PunBehaviour
	{
		#region Public Variables

		[Tooltip("The Ui Panel to let the user select whether he wants to join existing or random room")]
		public GameObject controlPanel;
		[Tooltip("The UI Label to inform the user that the connection is in progress")]
		public GameObject progressLabel;

		/// <summary>
		/// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
		/// </summary>   
		[Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
		public byte MaxPlayersPerRoom = 6;

		/// <summary>
		/// The PUN loglevel. 
		/// </summary>
		public PhotonLogLevel Loglevel = PhotonLogLevel.ErrorsOnly;


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
		string _gameVersion = "1";


		#endregion


		#region MonoBehaviour CallBacks

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
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


		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start()
		{
			Connect ();
		}


		#endregion


		#region Public Methods

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

			controlPanel.SetActive(true);
			progressLabel.SetActive(false);
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
				controlPanel.SetActive (false);
				progressLabel.SetActive (true);

				// we don't want to do anything if we are not attempting to join a room. 
				// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
				// we don't want to do anything.
				if (isConnecting) {
					// #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
					PhotonNetwork.JoinRandomRoom ();
				}
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
				string roomName = controlPanel.GetComponentInChildren<InputField>().text;

				controlPanel.SetActive (false);
				progressLabel.SetActive (true);

				// we don't want to do anything if we are not attempting to join a room. 
				// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
				// we don't want to do anything.
				if (isConnecting) {
					// #Critical we need at this point to attempt joining an existing and named Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
					RoomOptions roomOptions = new RoomOptions ();
					roomOptions.IsVisible = false;
					PhotonNetwork.JoinOrCreateRoom (roomName, roomOptions, TypedLobby.Default);
				}
			}
		}


		#endregion

		#region Photon.PunBehaviour CallBacks


		public override void OnConnectedToMaster()
		{
			// we don't want to do anything if we are not attempting to join a room. 
			// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
			// we don't want to do anything.
			if (isConnecting)
			{
				controlPanel.SetActive(true);
				progressLabel.SetActive(false);
			}

			Debug.Log("/Launcher: OnConnectedToMaster() was called by PUN");
		}


		public override void OnDisconnectedFromPhoton()
		{
			controlPanel.SetActive(true);
			progressLabel.SetActive(false);

			Debug.LogWarning("/Launcher: OnDisconnectedFromPhoton() was called by PUN");        
		}

		public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
		{
			Debug.Log("/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");

			// #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
			PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
		}

		public override void OnJoinedRoom()
		{
			// #Critical
			// Load the Room Level. 
			PhotonNetwork.LoadLevel("Main");

			Debug.Log("/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
		}

		#endregion
	}
}
