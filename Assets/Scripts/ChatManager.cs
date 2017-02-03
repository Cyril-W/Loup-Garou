using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon.Chat;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Chat manager. 
	/// Handles chatting system.
	/// </summary>
	public class ChatManager : MonoBehaviour, IChatClientListener {

		public static ChatClient chatClient;
		public static string UserName { get; set; }
		public static string ChannelName { get; set; }
		public static string RoomName { get; set; }

		public static List<string> playersConnected;

		[Tooltip("Up to a certain degree, previously sent messages can be fetched for context")]
		public int HistoryLengthToFetch = 0;

		[Tooltip("Text used to store all received messages")]
		public Text chatMessages;
		[Tooltip("Field containing the message to be sent")]
		public InputField InputFieldMessage;
	
		static int _previousScene;

		#region MonoBehaviour Callback


		// Use this for initialization
		void Start () {
			_previousScene = 0;

			DontDestroyOnLoad (gameObject);

			UserName = PhotonNetwork.player.NickName;
			ChannelName = "global#";
			RoomName = "";
			
			chatClient = new ChatClient(this);

			chatClient.ChatRegion = "EU";
			ExitGames.Client.Photon.Chat.AuthenticationValues authValues = new ExitGames.Client.Photon.Chat.AuthenticationValues ();
			authValues.UserId = UserName;
			authValues.AuthType = ExitGames.Client.Photon.Chat.CustomAuthenticationType.None;
			chatClient.Connect (PhotonNetwork.PhotonServerSettings.ChatAppID, "1", authValues);
		}
		
		// Update is called once per frame
		void Update () {
			if (chatClient != null)
				chatClient.Service ();

			PlayerAnimatorManager.isBlocked = InputFieldMessage.isFocused;
				
			if (Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return))
				OnEnterSend ();
		}

		void OnEnable() {
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void OnDisable() {
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		#endregion


		#region Custom


		void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			ChangeChannel (_previousScene, scene.buildIndex);
			_previousScene = scene.buildIndex;
		}

		//Be careful: the # between the name of the channel and the RoomName is very important to avoid displaying the long RoomName!
		void ChangeChannel(int formerSceneIndex, int newSceneIndex) {
			if(PhotonNetwork.inRoom)
				RoomName = PhotonNetwork.room.Name;

			if (newSceneIndex == 0) { // to Launcher scene
				ChannelName = "global#";
				chatClient.Subscribe (new string[] { ChannelName }, HistoryLengthToFetch);

				if (formerSceneIndex == 1) { //from Lobby	
					chatClient.PublishMessage ("lobby#" + RoomName, "left the lobby channel");
					chatClient.Unsubscribe (new string[] { "lobby#" + RoomName });
				} 
				else if (formerSceneIndex == 2) { //from Main
					chatClient.PublishMessage ("villager#" + RoomName, "left the villager channel");
					chatClient.Unsubscribe (new string[] { "villager#" + RoomName });
				} 
			} 
			else if (newSceneIndex == 1) { // to Lobby scene
				ChannelName = "lobby#" + RoomName;
				chatClient.Subscribe (new string[] { ChannelName }, HistoryLengthToFetch);

				if (formerSceneIndex == 0) { //from Launcher	
					chatClient.PublishMessage ("global#", "left the global channel");
					chatClient.Unsubscribe (new string[] { "global#" });
				} 
				else if (formerSceneIndex == 2) { //from Main
					Debug.Log("Error: you cannot go from Main to Lobby");
				} 
			} 
			else if (newSceneIndex == 2) { // to Main scene
				ChannelName = "villager#" + RoomName;
				chatClient.Subscribe (new string[] { ChannelName }, HistoryLengthToFetch);

				if (formerSceneIndex == 1) { //from Lobby	
					chatClient.PublishMessage ("lobby#" + RoomName, "left the lobby channel");
					chatClient.Unsubscribe (new string[] { "lobby#" + RoomName });
				} 
				else if (formerSceneIndex == 0) { //from Launcher
					Debug.Log("Error: you cannot go from Launcher to Main");
				} 
			}
		}

		/// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
		public void OnApplicationQuit()
		{
			if (chatClient != null) {
				if (SceneManagerHelper.ActiveSceneBuildIndex == 0) {
					Debug.Log ("You left the game on global channel");
					chatClient.PublishMessage ("global#", "left the game");
				} else if (SceneManagerHelper.ActiveSceneBuildIndex == 1) {
					Debug.Log ("You left the game on lobby channel");
					chatClient.PublishMessage ("lobby#" + RoomName, "left the game");
				} else if (SceneManagerHelper.ActiveSceneBuildIndex == 2) {
					Debug.Log ("You left the game on villager channel");
					chatClient.PublishMessage ("villager#" + RoomName, "left the game");
				}

				chatClient.Disconnect ();
			}
		}

		public void OnEnterSend()
		{
			if (InputFieldMessage != null && InputFieldMessage.text != "") {
				if (PlayerManager.LocalPlayerInstance != null && PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().isAlive == false)
					chatMessages.text += "[You are dead]\n";
				else
					SendChatMessage (InputFieldMessage.text);
				InputFieldMessage.text = "";
			}
		}

		public void OnClickSend()
		{
			if (InputFieldMessage != null && InputFieldMessage.text != "") {
				if (PlayerManager.LocalPlayerInstance != null && PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().isAlive == false)
					chatMessages.text += "[You are dead]\n";
				else
					SendChatMessage(InputFieldMessage.text);
				InputFieldMessage.text = "";
			}
		}

		void SendChatMessage(string inputLine)
		{
			if (string.IsNullOrEmpty(inputLine))
				return;

			chatClient.PublishMessage(ChannelName, inputLine);
		}

		static string GetChannelName(string channelName)
		{
			if (channelName == "")
				return "";
			else {
				int nbToKeep = 0;
				int i = 0;
				while (channelName [i] != '#') {
					nbToKeep++;
					i++;
				}
				return channelName.Substring (0, nbToKeep);
			}
		}


		#endregion


		#region IChatClientListener Interface Implementation


		void IChatClientListener.OnSubscribed(string[] channels, bool[] results) {
			if (channels.Length == 1)
				chatClient.PublishMessage (channels [0], "joined the " + GetChannelName(channels [0]) + " channel");
			else
				Debug.Log ("Oops, seems there is more than one channel!");
		}

		void IChatClientListener.OnUnsubscribed (string[] channels)
		{
			if (channels.Length == 1) {
				chatMessages.text = "Welcome to the in-game Chat. Have fun playing!\n------------------------------------------\n";
			} else
				Debug.Log ("Oops, seems there is more than one channel!");
		}

		void IChatClientListener.OnGetMessages(string channelName, string[] senders, object[] messages) {
			for (int i = 0; i < messages.Length; i++) {
				if (chatMessages != null)
					chatMessages.text += "[" + GetChannelName (channelName) + "] " + PlayerManager.GetProperName(senders [i]) + " > " + messages [i] + "\n";
				else
					Debug.Log ("Oops, seems there is nothing to contain the message!");
			}

			if (chatMessages != null)
				chatMessages.transform.parent.GetComponent<ScrollRect> ().verticalNormalizedPosition = 1;
		}

		void IChatClientListener.OnDisconnected ()
		{
			Debug.Log ("Disconnected: " + chatClient.DisconnectedCause);
		}

		void IChatClientListener.OnConnected ()
		{
			chatClient.Subscribe (new string[] { ChannelName }, HistoryLengthToFetch);
			// chatClient.SetOnlineStatus (ChatUserStatus.Online);
		}

		void IChatClientListener.OnChatStateChange (ChatState state)
		{
			// Debug.Log ("Chat client state changed to :" + state);
		}

		void IChatClientListener.DebugReturn (ExitGames.Client.Photon.DebugLevel level, string message)
		{
			/*
			if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
			{
				UnityEngine.Debug.LogError(message);
			}
			else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
			{
				UnityEngine.Debug.LogWarning(message);
			}
			else
			{
				UnityEngine.Debug.Log(message);
			}
			*/
		}

		void IChatClientListener.OnPrivateMessage (string sender, object message, string channelName)
		{
			Debug.Log ("Warning: private messages not handled!");
		}

		void IChatClientListener.OnStatusUpdate (string user, int status, bool gotMessage, object message)
		{
			Debug.Log ("Warning: friends not handled!");
		}


		#endregion
	}
}
