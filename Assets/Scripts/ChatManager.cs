using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon.Chat;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Chat manager. 
	/// Handles chatting system.
	/// </summary>
	public class ChatManager : MonoBehaviour, IChatClientListener {

		public static ChatClient chatClient;
		public static string UserName { get; set; }
		public static string RoomName { get; set; }

		[Tooltip("Up to a certain degree, previously sent messages can be fetched for context")]
		public int HistoryLengthToFetch;

		[Tooltip("Text used to store all received messages")]
		public Text chatMessages;
		[Tooltip("Field containing the message to be sent")]
		public InputField InputFieldMessage;
	

		#region MonoBehaviour Callback


		// Use this for initialization
		void Start () {
			UserName = PhotonNetwork.player.NickName + PhotonNetwork.player.ID;
			RoomName = PhotonNetwork.room.Name;

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


		#endregion


		#region Custom


		/// <summary>To avoid that the Editor becomes unresponsive, disconnect all Photon connections in OnApplicationQuit.</summary>
		public void OnApplicationQuit()
		{
			if (chatClient != null)
				chatClient.Disconnect ();
		}

		public static void LeaveChat() {
			if (chatClient != null) {
				chatClient.PublishMessage (RoomName, UserName + " left the room");
				chatClient.Disconnect ();
			}
		}

		public void OnEnterSend()
		{
			if (InputFieldMessage != null && InputFieldMessage.text != "") {
				SendChatMessage (InputFieldMessage.text);
				InputFieldMessage.text = "";
			}
		}

		public void OnClickSend()
		{
			if (InputFieldMessage != null && InputFieldMessage.text != "")
			{
				SendChatMessage(InputFieldMessage.text);
				InputFieldMessage.text = "";
			}
		}

		private void SendChatMessage(string inputLine)
		{
			if (string.IsNullOrEmpty(inputLine))
				return;

			chatClient.PublishMessage(RoomName, inputLine);
		}


		#endregion


		#region IChatClientListener Interface Implementation


		void IChatClientListener.OnSubscribed(string[] channels, bool[] results) {
			if(channels.Length == 1 && channels[0] == PhotonNetwork.room.Name)
				chatClient.PublishMessage (RoomName, "joined the room");
			else
				Debug.Log ("Oops, seems there is more than one channel!");
			
			//Debug.Log ("Subscribed to " + channels.Length + " new channels: " + channelNames);
		}

		void IChatClientListener.OnGetMessages(string channelName, string[] senders, object[] messages) {
			for (int i = 0; i < messages.Length; i++) {
				if (chatMessages != null) {
					chatMessages.text += senders [i] + ": " + messages [i] + "\n";
				}
				else
					Debug.Log ("Oops, seems there is nothing to contain the message!");
			}

			if (chatMessages != null)
				chatMessages.transform.parent.parent.GetChild (1).GetComponent<Scrollbar> ().value = 0;
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

		void IChatClientListener.OnDisconnected ()
		{
			Debug.Log ("Disconnected: " + chatClient.DisconnectedCause);
		}

		void IChatClientListener.OnConnected ()
		{
			chatClient.Subscribe (new string[] { PhotonNetwork.room.Name }, HistoryLengthToFetch);
			// chatClient.SetOnlineStatus (ChatUserStatus.Online);
		}

		void IChatClientListener.OnChatStateChange (ChatState state)
		{
			// Debug.Log ("Chat client state changed to :" + state);
		}

		void IChatClientListener.OnPrivateMessage (string sender, object message, string channelName)
		{
			Debug.Log ("Warning: private messages not handled!");
		}

		void IChatClientListener.OnUnsubscribed (string[] channels)
		{
			if (channels.Length == 1 && channels [0] == RoomName)
				chatClient.PublishMessage (RoomName, "left the room");
			else
				Debug.Log ("Oops, seems there is more than one channel!");
		}

		void IChatClientListener.OnStatusUpdate (string user, int status, bool gotMessage, object message)
		{
			Debug.Log ("Warning: friends not handled!");
		}


		#endregion
	}
}
