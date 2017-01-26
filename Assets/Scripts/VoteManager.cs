using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class PlayerButton {
		public Button Button { get; set; }
		public int PlayerID { get; set; }
	}

	public class VoteManager : MonoBehaviour {

		#region Public Variables

		[Tooltip("The local player who votes on this client")]
		public static GameObject localPlayer;
		[Tooltip("The panel used for voting against player")]
		public GameObject votePanel;
		[Tooltip("The Prefab used to populate the player list")]
		public Button voteButton;


		#endregion


		#region Private Variables


		static GameObject _votePanel;
		static Button _voteButton;
		static List<PlayerButton> playerButtons = new List<PlayerButton> ();
		static bool hasVoted = false;


		#endregion


		#region MonoBehaviour CallBacks


		// Use this for initialization
		void Start () {
			_voteButton = voteButton;
			_votePanel = votePanel;

			PhotonPlayer[] players = PhotonNetwork.otherPlayers;
			foreach (PhotonPlayer player in players)
				RegisterPlayerForVote (player.ID);
			
			_votePanel.SetActive (false);
		}
		
		// Update is called once per frame
		void Update () {
			if (SceneManagerHelper.ActiveSceneBuildIndex == 1) {
				if(!votePanel)
					votePanel = GameObject.FindGameObjectWithTag ("VotingPanel");
				
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
		}


		#endregion


		#region Custom


		public static void OnClicked(int playerIDClicked) {
			localPlayer.GetComponent<PlayerManager> ().votedPlayerID = playerIDClicked;

			hasVoted = true;
		}

		public static void RegisterPlayerForVote (int playerID) {
			Button btn = Instantiate (_voteButton);
			btn.GetComponentInChildren<Text> ().text = PhotonPlayer.Find(playerID).NickName;
			btn.transform.SetParent (_votePanel.transform.GetChild(1).GetChild(0));
			btn.onClick.AddListener (delegate { OnClicked (playerID); });

			playerButtons.Add (new PlayerButton() {Button = btn, PlayerID = playerID});
		}

		public static void RemovePlayerForVote (int playerID) {
			PlayerButton playerButton = playerButtons.Find (p => p.PlayerID == playerID);
			Destroy(playerButton.Button.gameObject);

			playerButtons.Remove (playerButton);
		}


		#endregion
	}
}
