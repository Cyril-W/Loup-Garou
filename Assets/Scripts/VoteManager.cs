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

		public static VoteManager Instance;


		#endregion


		#region Private Variables


		static List<PlayerButton> playerButtons = new List<PlayerButton> ();
		static List<string> _votedPlayers;
		static bool _hasVoted = false;
		static bool _voteChecked = false;


		#endregion


		#region MonoBehaviour CallBacks


		// Use this for initialization
		void Start () {
			Instance = this;
			whoVoted.text = "";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if(player.name != PhotonNetwork.player.NickName)
					RegisterPlayerForVote (player.name);
			}
			_votedPlayers = new List<string> ();
			votePanel.SetActive (false);
		}
		
		// Update is called once per frame
		void Update () {	
			PlayerManager player = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			RefreshVotedPlayer (ref _votedPlayers);

			if (DayNightCycle.currentTime >= 0.25f && DayNightCycle.currentTime < 0.375f) {
				whoVoted.transform.parent.gameObject.SetActive (true);
				_voteChecked = false;
				if(player.isAlive)
					votePanel.SetActive (!_hasVoted);
			} else if (DayNightCycle.currentTime >= 0.375f && DayNightCycle.currentTime < 0.75f) {
				RefreshPlayerList ();
				votePanel.SetActive (false);
				_hasVoted = false;
				player.votedPlayer = "";

				// we just do this once, using a flag
				if (!_voteChecked) {
					bool isAlive = CheckVote ();
					_voteChecked = true;
					_hasVoted = false;
					if (!isAlive) {
						player.isAlive = false;
						votePanel.SetActive (false);
						GetComponent<VoteManager> ().enabled = false;
					}
				}
			} else if (DayNightCycle.currentTime >= 0.75f && DayNightCycle.currentTime < 0.875f) {
				whoVoted.transform.parent.gameObject.SetActive (false);
				_voteChecked = false;
				if(player.role == "Werewolf" && player.isAlive)
					votePanel.SetActive (!_hasVoted);
			} else {
				RefreshPlayerList ();
				votePanel.SetActive (false);
				_hasVoted = false;
				player.votedPlayer = "";

				// we just do this once, using a flag
				if (!_voteChecked) {
					bool isAlive = CheckVote ();
					_voteChecked = true;
					_hasVoted = false;
					if (!isAlive) {
						player.isAlive = false;
						votePanel.SetActive (false);
						whoVoted.transform.parent.gameObject.SetActive (true);
						GetComponent<VoteManager> ().enabled = false;
					}
				}
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
		/// Searches through all the players, checks if they voted against you and updates the people who were voted against.
		/// </summary>
		void RefreshVotedPlayer(ref List<string> votedPlayers) {
			VoteManager.Instance.whoVoted.text = "";
			votedPlayers.Clear ();

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.votedPlayer != "") {
					votedPlayers.Add (pM.votedPlayer);
					if (pM.votedPlayer == PlayerManager.LocalPlayerInstance.name)
						VoteManager.Instance.whoVoted.text += "~ " + PlayerManager.GetProperName(player.name) + "\n";
				}
			}
		}

		/// <summary>
		/// Searches through all the voted player list, if one has stricly more votes than the other, he's eliminated.
		/// If 2 or more players have the same number of vote, no one is eliminated.
		/// </summary>
		bool CheckVote() {
			if (_votedPlayers.Count == 0)
				return true;
			else {
				int previousCount = _votedPlayers.FindAll (p => p == _votedPlayers [0]).Count;
				List<string> mostVotedPlayers = new List<string> ();
				mostVotedPlayers.Add (_votedPlayers [0]);

				for (int i = 1; i < _votedPlayers.Count; i++) {
					int newCount = _votedPlayers.FindAll (p => p == _votedPlayers [i]).Count;
					if (newCount > previousCount) {
						mostVotedPlayers.Remove (_votedPlayers [i - 1]);
						mostVotedPlayers.Add (_votedPlayers [i]);
						previousCount = newCount;
					} else if (newCount == previousCount && _votedPlayers [i - 1] != _votedPlayers [i])
						mostVotedPlayers.Add (_votedPlayers [i]);
				}
					
				if (mostVotedPlayers.Count > 1)
					return true;
				else if (mostVotedPlayers [0] != PlayerManager.LocalPlayerInstance.name)
					return true;
				else 
					return false;
			}
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
			Button btn = Instantiate (VoteManager.Instance.voteButton);
			btn.GetComponentInChildren<Text> ().text = PlayerManager.GetProperName(playerName);
			btn.transform.SetParent (VoteManager.Instance.votePanel.transform.GetChild(1).GetChild(0));
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
				
			VoteManager.Instance.votePanel.GetComponentInChildren<ScrollRect> ().verticalNormalizedPosition = 1;
		}


		#endregion
	}
}
