using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Single Vote manager. 
	/// Handles the vote from the localPlayer only, when he dies as a Mayor or Hunter.
	/// </summary>
	public class SingleVoteManager : MonoBehaviour {
		
		#region Public Variables


		[Tooltip("The Prefab used to populate the player list")]
		public GameObject voteButton;
		[Tooltip("The Transform to which player buttons are added")]
		public Transform voteButtonGrid;
		public List<PlayerButton> playerButtons;
		public string reason;
		public float secondsToVote = 30f;


		#endregion


		#region Private Variables


		PlayerManager _localPM;
		Text _title;
		Text _description;
		Text _counter;


		#endregion


		#region MonoBehaviour CallBacks


		void Awake () {
			_title = transform.GetChild (0).GetChild (0).GetComponent<Text> ();
			_description = transform.GetChild (1).GetChild (0).GetComponent<Text> ();
			_counter = transform.GetChild (0).GetChild (1).GetComponent<Text> ();

			_localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();

			playerButtons = new List<PlayerButton> ();

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player != PlayerManager.LocalPlayerInstance) {
					PlayerManager pM = player.GetComponent<PlayerManager> ();
					if (pM.isAlive)
						RegisterPlayerForVote (pM);
				}
			}

			voteButtonGrid.transform.parent.GetComponent<ScrollRect> ().verticalNormalizedPosition = 1;

			// The vote is instantly canceled if no player is alive
			if (playerButtons.Count == 0)
				secondsToVote = 0;
		}

		void Update () {
			if (reason == "Mayor") {
				_title.text = "Successor vote";
				_description.text = "Elect your successor:\n\nYou have few seconds to name the next Mayor of the Village. If you don't seize this opportunity, a new Mayor will be randomly elected.";
			} else if (reason == "Hunter") {
				_title.text = "Vendetta vote";
				_description.text = "One name, one dead:\n\nYou have been killed. You now have few seconds to name the victime of your riffle. If you don't seize this occasion, nobody will die.";
			} else {
				_title.text = "Unknown reason";
				_description.text = "No reason!\n\nThis vote has been started without any particular reason to it...";
			}

			_counter.text = Mathf.RoundToInt(secondsToVote).ToString();
			secondsToVote -= Time.deltaTime;
			if (secondsToVote <= 0f) {
				secondsToVote = 0f;

				AnalyzeOneShotResult ("");
			} 
		}


		#endregion


		#region Custom


		/// <summary>
		/// Depending on the reason, elects a new mayor or kills a player ... or do nothing if no one has been selected.
		/// </summary>
		void AnalyzeOneShotResult(string voted) {
			gameObject.SetActive (false);

			if (voted != "") {
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				GameObject votedPlayer = null;
				foreach (GameObject player in players) {
					if (player.name == voted)
						votedPlayer = player;					
				}
				if (votedPlayer != null) {
					if (reason == "Mayor")
						VoteManager.Instance.gameObject.GetComponent<PhotonView> ().RPC ("SetNewMayor", PhotonTargets.MasterClient, new object[] { voted });
					else if (reason == "Hunter")
						votedPlayer.GetComponent<PhotonView> ().RPC ("KillPlayer", PhotonTargets.All, new object[] { });
				}
			} 

			if (reason == "Hunter")
				_localPM.hunterBulletAvailable = false;

			VoteManager.singleVotes.Remove (this);
			Destroy(gameObject);
		}

		/// <summary>
		/// This is the event triggered by the player vote button.
		/// </summary>
		public void OnClicked(string playerClicked) {
			AnalyzeOneShotResult (playerClicked);			
		}

		/// <summary>
		/// This is the event triggered by the cancel button.
		/// </summary>
		public void OnSkipVote() {
			AnalyzeOneShotResult ("");	
		}

		/// <summary>
		/// Add a button linked to the player photonID and add listener on it.
		/// </summary>
		void RegisterPlayerForVote (PlayerManager pM) {
			string playerName = pM.gameObject.name;
			Button btn = Instantiate (voteButton).GetComponent<Button>();

			btn.GetComponentInChildren<Text> ().text = PlayerManager.GetProperName(playerName);

			Image[] images = btn.GetComponentsInChildren<Image> ();
			if (pM.isDiscovered) {
				Sprite roleSprite = Resources.Load ("Cards/" + pM.role, typeof(Sprite)) as Sprite;
				if (roleSprite != null)
					images[1].sprite = roleSprite;
				else
					Debug.Log ("No image was found for " + pM.role);
			}
			if (VoteManager.Instance.mayorName == playerName) {
				Sprite mayorSprite = Resources.Load ("Cards/MayorDay", typeof(Sprite)) as Sprite;
				if (mayorSprite != null)
					images[2].sprite = mayorSprite;
				else
					Debug.Log ("No image was found for Mayor");
			}

			btn.transform.SetParent (voteButtonGrid);
			btn.onClick.AddListener (delegate {
				OnClicked (playerName);
			});

			playerButtons.Add (new PlayerButton () { Button = btn, PlayerName = playerName });
		}

		/// <summary>
		/// Remove the button linked to the player photonID.
		/// </summary>
		public void RemovePlayerForVote (string playerName) {
			PlayerButton playerButton = playerButtons.Find (p => p.PlayerName == playerName);
			if(playerButton != null)
				Destroy(playerButton.Button.gameObject);

			playerButtons.Remove (playerButton);
		}


		#endregion
	}
}
