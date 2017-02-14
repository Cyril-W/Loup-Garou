using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// PlayerButton. 
	/// Is used to link a button with a Player Name.
	/// </summary>
	public class PlayerButton {
		public Button Button { get; set; }
		public string PlayerName { get; set; }
	}

	/// <summary>
	/// Vote manager. 
	/// Handles voting time (when to instantiante a vote canvas).
	/// </summary>
	public class VoteManager : Photon.PunBehaviour, IPunObservable {
		#region Public Variables


		[Tooltip("The Prefab used to set a new single vote")]
		public GameObject voteCanvas;
		[Tooltip("The name of the current Mayor of the Village")]
		public string mayorName;

		public int countDay = 1;
		public int countNight = 0;

		public static List<SingleVoteManager> votes = new List<SingleVoteManager>();
		public static VoteManager Instance;


		#endregion


		#region Private Variables


		Text _descriptionText;
		PlayerManager _localPM;
		bool _isOnVillagerChannel = true;
		static List<string> _votedPlayers;


		#endregion


		#region MonoBehaviour CallBacks


		// Use this for initialization
		void Start () {
			Instance = this;
			_descriptionText = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (2).GetChild(1).GetComponentInChildren<Text> ();
			_localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			_votedPlayers = new List<string> ();
		}
		
		// Update is called once per frame
		void Update () {
			if (RoleManager.gameFinished == true)
				return;
			
			RefreshVotedPlayers (ref _votedPlayers);

			if (0f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.25f) {
				if(countDay == 1)
					_descriptionText.text = "Wake up! Time to discuss with other villagers, the vote for the new Mayor is coming!";
				else
					_descriptionText.text = "Wake up! Time to discuss with other villagers, the vote for the next victim is coming!";

				if (!_isOnVillagerChannel) {
					_localPM.votedPlayer = "";
					countDay++;
					_isOnVillagerChannel = !_isOnVillagerChannel;
					if(!_localPM.isAlive || _localPM.role == "Werewolf")
						ChatManager.Instance.SwitchVillagerToWerewolf (true);		
					else
						ChatManager.Instance.SwitchVillagerToWerewolf (false);			
				}
			} else if (0.25f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.375f) {
				_descriptionText.text = "If you already voted, patience! Once the vote are all collected, results will be announced!";

				if (votes.Find (v => v.isOneShot == false) == null) {
					SingleVoteManager svM = Instantiate (voteCanvas).GetComponent<SingleVoteManager> ();
					svM.secondsToVote = DayNightCycle.secondsInDay / 4;
					if (countDay == 1) {
						svM.title.text = "Mayor election";
						svM.description.text = "Day 1:\nIt's high noon! Elect your new Mayor by simply clicking on the player you think the most apt to represents the Village.";
					} else {
						CheckOrSetMayor ();
						svM.title.text = "Villager vote";
						svM.description.text = "Day " + countDay + ":\nThis usual vote is set to select the next victim to be executed. Let us hope it is a Werewolf...";
					}
					votes.Add (svM);
				}
			} else if (0.375f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.5f) {
				_descriptionText.text = "Votes have all been counted! Now finish your discussions and go back to your tent, night is falling upon the Village!";

				SingleVoteManager svM = votes.Find (v => v.isOneShot == false);
				if (svM != null) {
					if (countDay == 1) {
						if (PhotonNetwork.isMasterClient)
							mayorName = CheckPlayerMostVoted ();
						if(mayorName == "")
							CheckOrSetMayor ();
					}
					else if (_localPM.gameObject.name == CheckPlayerMostVoted())		
						_localPM.isAlive = false;					
					Destroy (svM.gameObject);
					votes.Remove (svM);
				}
			} else if (0.5f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.75f) {
				_descriptionText.text = "If there is a Seer still alive, time for her to come out and spy on other's role!";

				if (_isOnVillagerChannel) {
					_localPM.votedPlayer = "";
					countNight++;
					_isOnVillagerChannel = !_isOnVillagerChannel;
					if( !_localPM.isAlive || _localPM.role == "Werewolf")
						ChatManager.Instance.SwitchVillagerToWerewolf (true);		
					else
						ChatManager.Instance.SwitchVillagerToWerewolf (false);	
				}
			}else if (0.75f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.875f) {
				_descriptionText.text = "Time for the Werewolf to strike down their opponent! They are silently agreeing on who to devour...";

				if (votes.Find (v => v.isOneShot == false) == null) {
					SingleVoteManager svM = Instantiate (voteCanvas).GetComponent<SingleVoteManager> ();
					if (_localPM.role != "Werewolf")
						svM.secondsToVote = 0;
					else
						svM.secondsToVote = DayNightCycle.secondsInNight/4;
					svM.title.text = "Werewolf vote";
					svM.description.text = "Night " + countNight + ":\nThis usual vote is set to select the next victim to be executed. Kill them all!";
					votes.Add (svM);
				}
			}else if (0.875f < DayNightCycle.currentTime && DayNightCycle.currentTime < 1f) {
				_descriptionText.text = "Votes have all been counted! The victim of the Werewolf now lies on the ground, covered in blood!";

				SingleVoteManager svM = votes.Find (v => v.isOneShot == false);
				if (svM != null) {
					if (_localPM.gameObject.name == CheckPlayerMostVoted())	
						_localPM.isAlive = false;
					Destroy (svM.gameObject);
					votes.Remove (svM);
				}
			}
		}

		public void OnApplicationQuit() 
		{
			foreach (SingleVoteManager svM in votes)
				svM.RemovePlayerForVote (PlayerManager.LocalPlayerInstance.name);
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
			foreach (SingleVoteManager svM in votes)
				svM.RemovePlayerForVote (PhotonNetwork.player.NickName);
			PhotonNetwork.LeaveRoom();
		}

		/// <summary>
		/// Starts a vote specific to the local player, for example when the Mayor or the Hunter dies.
		/// </summary>
		public void StartOneShotVote (string reason)
		{			
			if (reason == "Mayor") {
				if (votes.Find (v => (v.isOneShot == true && v.title.text == "Successor vote")) == null) {
					SingleVoteManager svM = Instantiate (voteCanvas).GetComponent<SingleVoteManager> ();
					svM.isOneShot = true;
					svM.title.text = "Successor vote";
					svM.description.text = "Elect your successor:\nYou have few seconds to name the next Mayor of the Village. If you don't seize this opportunity, a new Mayor will be randomly elected.";
					votes.Add (svM);

					if (RoleManager.nbPlayerAlive <= 1)
						svM.secondsToVote = 0;
				} 
			}else if (reason == "Hunter") {
				if (votes.Find (v => (v.isOneShot == true && v.title.text == "Vendetta vote")) == null) {
					SingleVoteManager svM = Instantiate (voteCanvas).GetComponent<SingleVoteManager> ();
					svM.isOneShot = true;
					svM.title.text = "Vendetta vote";
					svM.description.text = "One name, one dead:\nYou have been killed. You now have few seconds to name the victime of your riffle. If you don't seize this occasion, nobody will die.";
					votes.Add (svM);

					if (RoleManager.nbPlayerAlive <= 1)
						svM.secondsToVote = 0;
				}
			} else
				Debug.Log ("Reason unknown, or vote already started!");
		}

		/// <summary>
		/// Seeks the vote of the local player, checks if he voted against and acts accordingly.
		/// </summary>
		[PunRPC]
		public void AnalyzeOneShotResult (SingleVoteManager svM, string voter, string voted) {
			PlayerManager voterPM = null;
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player.name == voter)
					voterPM = player.GetComponent<PlayerManager> ();
			}
			if (voterPM != null) {			
				GameObject votedPlayer = null;
				foreach (GameObject player in players) {
					if (player.name == voted)
						votedPlayer = player;					
				}
				if (votedPlayer != null) {
					if (voter == mayorName) {
						if (voted == "")
							CheckOrSetMayor ();
						else
							this.mayorName = voted;						
					} else if (voterPM.role == "Hunter") {
						// votedPM.isAlive = false;
					}
				}
			}

			Destroy(svM.gameObject);
			votes.Remove (svM);
		}

		/// <summary>
		/// Searches through all the players, checks if they voted against you and updates the people who were voted against.
		/// </summary>
		void RefreshVotedPlayers(ref List<string> votedPlayers) {
			Text displayText = GetComponentInChildren<Text> ();
			displayText.text = "~ Mayor: " + PlayerManager.GetProperName(mayorName) + " ~\nHere are all the vote:\n~~~~~~~~~~~~~~\n";
			votedPlayers.Clear ();

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.votedPlayer != "") {
					votedPlayers.Add (pM.votedPlayer);
					if (DayNightCycle.currentTime >= 0f && DayNightCycle.currentTime < 0.5f && player.name == mayorName)
						votedPlayers.Add (pM.votedPlayer);
				}
				displayText.text += PlayerManager.GetProperName (player.name) + " > " + PlayerManager.GetProperName(pM.votedPlayer) + "\n";
			}
		}

		/// <summary>
		/// Searches through all the voted player list: if the local player has stricly more votes than the others, he's eliminated.
		/// If 2 or more players have the same number of vote, no one is eliminated (but the mayor makes it impossible).
		/// </summary>
		string CheckPlayerMostVoted() {
			if (_votedPlayers.Count == 0)
				return "";
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
					} else if (newCount == previousCount && !mostVotedPlayers.Contains(_votedPlayers [i]))
						mostVotedPlayers.Add (_votedPlayers [i]);
				}
					
				if (mostVotedPlayers.Count > 1)
					return "";
				else
					return mostVotedPlayers [0];
			}
		}

		/// <summary>
		/// Searches through all the player list: if no one has has been elected, a random mayor is elected.
		/// </summary>
		void CheckOrSetMayor () {
			if (!PhotonNetwork.isMasterClient)
				return;

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			if (mayorName != "") {
				foreach (GameObject player in players) {
					if (mayorName == player.name)
						return;
				}
				Debug.Log ("The Mayor name is attributed to a player that does not exist anymore!");
				mayorName = "";
			} 

			if (mayorName == "" && RoleManager.nbPlayerAlive > 1) {
				int randomMayor;
				do {
					randomMayor = Random.Range (0, players.Length);
				} while (players [randomMayor].GetComponent<PlayerManager> ().isAlive == false);
				Debug.Log ("No Mayor found: player n°" + randomMayor + " has been randomly elected");
				mayorName = players [randomMayor].name;
			} else if (RoleManager.nbPlayerAlive <= 1)
				mayorName = "";			
		}


		#endregion


		#region IPunObservable implementation


		void IPunObservable.OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				// We own this player: send the others our data
				stream.SendNext(mayorName);
			}else{
				// Network player, receive data
				this.mayorName = (string)stream.ReceiveNext ();
			}
		}


		#endregion
	}
}
