using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// PlayerButton. 
	/// Is used to link a button with a PlayerID.
	/// </summary>
	public class PlayerButton {
		public Button Button { get; set; }
		public int PlayerID { get; set; }
	}

	/// <summary>
	/// Vote manager. 
	/// Handles voting time (when to show the panel), and vote from the localPlayer, which is synced by Photon.
	/// </summary>
	public class VoteManager : MonoBehaviour {

		#region Public Variables


		[Tooltip("The local player who votes on this client")]
		public static GameObject localPlayer;
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
		static bool hasVoted = false;


		#endregion


		#region MonoBehaviour CallBacks


		// Use this for initialization
		void Start () {
			_voteButton = voteButton;
			_votePanel = votePanel;
			_whoVoted = whoVoted;

			PhotonPlayer[] players = PhotonNetwork.otherPlayers;
			foreach (PhotonPlayer player in players)
				RegisterPlayerForVote (player.ID);
			
			_votePanel.SetActive (false);
		}
		
		// Update is called once per frame
		void Update () {			
			if (DayNightCycle.currentTime >= 0.25f && DayNightCycle.currentTime < 0.375f) {
				if (!hasVoted)
					_votePanel.SetActive (true);
				else
					_votePanel.SetActive (false);
			} else {
				hasVoted = false;
				_votePanel.SetActive (false);
			}
		}


		#endregion


		#region Custom

		/// <summary>
		/// Searches through all the players, checks if they voted against you and updates your numberOfVote.
		/// </summary>
		public static int RefreshWho () {
			int numberOfVote = 0;
			_whoVoted.text = "";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player.GetComponent<PlayerManager> ().votedPlayerID == PhotonNetwork.player.ID) {
					_whoVoted.text += "~ " + player.name + "\n";
					numberOfVote++;
				}
			}

			return numberOfVote;
		}

		public static void OnClicked(int playerIDClicked) {
			localPlayer.GetComponent<PlayerManager> ().votedPlayerID = playerIDClicked;

			hasVoted = true;
		}

		/// <summary>
		/// Add a button linked to the player photonID and add listener on it.
		/// </summary>
		public static void RegisterPlayerForVote (int playerID) {
			Button btn = Instantiate (_voteButton);
			btn.GetComponentInChildren<Text> ().text = PhotonPlayer.Find(playerID).NickName;
			btn.transform.SetParent (_votePanel.transform.GetChild(1).GetChild(0));
			btn.onClick.AddListener (delegate { OnClicked (playerID); });

			playerButtons.Add (new PlayerButton() {Button = btn, PlayerID = playerID});
		}

		/// <summary>
		/// Remove the button linked to the player photonID.
		/// </summary>
		public static void RemovePlayerForVote (int playerID) {
			PlayerButton playerButton = playerButtons.Find (p => p.PlayerID == playerID);
			Destroy(playerButton.Button.gameObject);

			playerButtons.Remove (playerButton);
		}


		#endregion
	}
}
