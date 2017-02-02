using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// PlayerButton. 
	/// Is used to link a button with a PlayerID.
	/// </summary>
	public class PlayerButton {
		public Button Button { get; set; }
		public string PlayerName { get; set; }
	}

	/// <summary>
	/// Vote manager. 
	/// Handles voting time (when to show the panel), and vote from the localPlayer, which is synced by Photon.
	/// </summary>
	public class VoteManager : Photon.PunBehaviour {

		#region Public Variables


		[Tooltip("The panel used for voting against player")]
		public GameObject votePanel;
		[Tooltip("The Prefab used to populate the player list")]
		public Button voteButton;
		[Tooltip("The Text used to display who voted against you")]
		public Text whoVoted;


		#endregion


		#region Private Variables


		static GameObject _votePanel;
		static Button _voteButton;
		static Text _whoVoted;
		static List<PlayerButton> playerButtons = new List<PlayerButton> ();
		static bool _hasVoted = false;


		#endregion


		#region MonoBehaviour CallBacks


		// Use this for initialization
		void Start () {
			_voteButton = voteButton;
			_votePanel = votePanel;
			_whoVoted = whoVoted;

			_whoVoted.text = "";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if(player.name != PhotonNetwork.player.NickName)
					RegisterPlayerForVote (player.name);
			}
			
			_votePanel.SetActive (false);
		}
		
		// Update is called once per frame
		void Update () {	
			PlayerManager player = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();

			if (DayNightCycle.currentTime >= 0.25f && DayNightCycle.currentTime < 0.375f) {
				if (VoteManager.RefreshWho () > PhotonNetwork.room.PlayerCount / 2) {
					player.isAlive = false;
					_votePanel.SetActive (false);
					GetComponent<VoteManager> ().enabled = false;
				} else
					_votePanel.SetActive (!_hasVoted);
			} else {
				RefreshPlayerList ();
				_votePanel.SetActive (false);
				_hasVoted = false;
				player.votedPlayer = "";

			}
		}

		public void OnApplicationQuit() 
		{
			VoteManager.RemovePlayerForVote (PlayerManager.LocalPlayerInstance.name);
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
			VoteManager.RemovePlayerForVote (PhotonNetwork.player.NickName);
			PhotonNetwork.LeaveRoom();
		}

		/// <summary>
		/// Searches through all the players, checks if they voted against you and updates your numberOfVote.
		/// </summary>
		public static int RefreshWho () {
			int numberOfVote = 0;
			_whoVoted.text = "";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player.GetComponent<PlayerManager>().votedPlayer == PlayerManager.LocalPlayerInstance.name) {
					_whoVoted.text += "~ " + PlayerManager.GetProperName(player.name) + "\n";
					numberOfVote++;
				}
			}

			return numberOfVote;
		}

		public static void OnClicked(string playerClicked) {
			PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().votedPlayer = playerClicked;

			_hasVoted = true;
		}

		public void OnSkipVote() {
			_hasVoted = true;
		}

		/// <summary>
		/// Add a button linked to the player photonID and add listener on it.
		/// </summary>
		public static void RegisterPlayerForVote (string playerName) {
			Button btn = Instantiate (_voteButton);
			btn.GetComponentInChildren<Text> ().text = PlayerManager.GetProperName(playerName);
			btn.transform.SetParent (_votePanel.transform.GetChild(1).GetChild(0));
			btn.onClick.AddListener (delegate { VoteManager.OnClicked (playerName); });

			playerButtons.Add (new PlayerButton() {Button = btn, PlayerName = playerName});
		}

		/// <summary>
		/// Remove the button linked to the player photonID.
		/// </summary>
		public static void RemovePlayerForVote (string playerName) {
			PlayerButton playerButton = playerButtons.Find (p => p.PlayerName == playerName);
			if(playerButton != null)
				Destroy(playerButton.Button.gameObject);

			playerButtons.Remove (playerButton);
		}

		/// <summary>
		/// Updates the players that still can be voted.
		/// </summary>
		public static void RefreshPlayerList () {
			foreach (PlayerButton pB in playerButtons) {
				if(pB.Button != null)
					Destroy (pB.Button.gameObject);
			}
			playerButtons.Clear ();

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player.name != PhotonNetwork.player.NickName && player.GetComponent<PlayerManager> ().isAlive)
					RegisterPlayerForVote (player.name);
			}
				
			_votePanel.GetComponentInChildren<ScrollRect> ().verticalNormalizedPosition = 1;
		}


		#endregion
	}
}
