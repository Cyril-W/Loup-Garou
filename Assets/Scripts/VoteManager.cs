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
	/// Handles results of votes, for the Mayor or for the victim of Villagers and Werewolves.
	/// </summary>
	public class VoteManager : Photon.PunBehaviour, IPunObservable {
		
		#region Public Variables


		[Tooltip("The Prefab used to set a new single vote")]
		public GameObject voteCanvas;
		public string mayorName;

		public int countDay = 1;
		public int countNight = 0;

		public static string mostVotedPlayer;
		public static List<SingleVoteManager> singleVotes = new List<SingleVoteManager>();
		public static VoteManager Instance;


		#endregion


		#region Private Variables


		Text _clockDescriptionText;
		Text _infoText;
		Text _mayorNameText;
		PlayerManager _localPM;
		/// <summary>
		/// The button used by the local Player to reset his vote only during the voting phases.
		/// </summary>
		GameObject _resetVoteButton;
		static List<string> _votedPlayers;
		int _formerState = 1;


		#endregion


		#region MonoBehaviour CallBacks


		void Start () {
			Instance = this;
			_clockDescriptionText = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (2).GetChild(0).GetComponentInChildren<Text> ();
			_infoText = transform.GetChild (1).GetChild (0).GetComponentInChildren<Text> ();
			_mayorNameText = transform.GetChild (1).GetChild (2).GetComponentInChildren<Text> ();
			_resetVoteButton = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (3).GetComponentInChildren<Button> ().gameObject;
			_localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			_votedPlayers = new List<string> ();
		}

		void Update () {
			if (RoleManager.gameFinished == true)
				return;
			
			RefreshVotedPlayers ();

			UpdateClockText ();

			VoteSystem ();
		}

		public void OnApplicationQuit() 
		{
			foreach (SingleVoteManager svM in singleVotes)
				svM.RemovePlayerForVote (_localPM.gameObject.name);
		}


		public override void OnLeftRoom()
		{
			SceneManager.LoadScene(0);
		}


		#endregion


		#region Custom


		/// <summary>
		/// When the Leave button is clicked, this function disconnect erase the player from the game and disconnect him.
		/// </summary>
		public void LeaveRoom()
		{
			foreach (SingleVoteManager svM in singleVotes)
				svM.RemovePlayerForVote (PhotonNetwork.player.NickName);
			PhotonNetwork.LeaveRoom();
		}

		/// <summary>
		/// Starts a vote specific to the local player, when the Mayor or the Hunter dies.
		/// </summary>
		public void StartSingleVote (string reason)
		{			
			if (RoleManager.gameFinished == false && singleVotes.Count == 0 && singleVotes.Find (v => v.reason == reason ) == null) {
				SingleVoteManager svM = Instantiate (voteCanvas).GetComponent<SingleVoteManager> ();
				svM.reason = reason;
				singleVotes.Add (svM);
			}
		}

		[PunRPC]
		public void SetNewMayor (string newMayorName) {
			if (PhotonNetwork.isMasterClient)
				this.mayorName = newMayorName;
		}

		/// <summary>
		/// Searches through all the players, updates the voted player list and check if there is a most voted player.
		/// </summary>
		void RefreshVotedPlayers() {
			_mayorNameText.text = "Mayor:\n" + PlayerManager.GetProperName(mayorName);
			_votedPlayers.Clear ();

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			_infoText.text = "";
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.votedPlayer != "") {
					_votedPlayers.Add (pM.votedPlayer);
					// Double vote of the mayor doesn't count during the night if he's a Werewolf!
					if (DayNightCycle.GetCurrentState() < 5 && player.name == mayorName)
						_votedPlayers.Add (pM.votedPlayer);
				}
				_infoText.text += PlayerManager.GetProperName (player.name) + " > " + PlayerManager.GetProperName(pM.votedPlayer) + "\n";
			}

			// This part checks if there is a most voted player
			if (_votedPlayers.Count == 0)
				mostVotedPlayer = "";
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
					mostVotedPlayer = "";
				else 
					mostVotedPlayer = mostVotedPlayers [0];				
			}
		}

		/// <summary>
		/// Searches through all the player list: if no one has been elected, a random mayor is elected.
		/// </summary>
		void CheckOrSetMayor () {
			// We ensure only one player checks for this, to avoid mutliple Mayor nomination
			if (!PhotonNetwork.isMasterClient)
				return;

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			if (mayorName != "") {
				foreach (GameObject player in players) {
					if (mayorName == player.name)
						return;
				}
				mayorName = "";
			} 

			// We only nominate a new Mayor if it's meaningfull: if there is at least 2 players
			if (mayorName == "" && RoleManager.nbPlayerAlive > 1) {
				int randomMayor;
				do {
					randomMayor = Random.Range (0, players.Length);
				} while (players [randomMayor].GetComponent<PlayerManager> ().isAlive == false);
				mayorName = players [randomMayor].name;
			} else if (RoleManager.nbPlayerAlive <= 1)
				mayorName = "";			
		}

		/// <summary>
		/// This function indicates to all player what to do depending on the time of the day. It also (in)activate the resetVoteButton
		/// </summary>
		void UpdateClockText () {
			bool resetVoteButtonActive = false;

			if (DayNightCycle.GetCurrentState () == 1) {
				if (countDay == 1)
					_clockDescriptionText.text = "Welcome! You just woke up in the middle of a Village! You have time to explore it, for now.";
				else
					_clockDescriptionText.text = "Wake up! Look all around you, there may be few people who died during the night!";
			} else if (DayNightCycle.GetCurrentState () == 2) {
				if (countDay == 1)
					_clockDescriptionText.text = "Time to discuss and vote for the new Mayor by clicking on the button in front of the player Tent!";
				else
					_clockDescriptionText.text = "Time to discuss and vote for the next victim by clicking on the button in front of the player Tent!";
				if(_localPM.isAlive)
					resetVoteButtonActive = true;
			} else if (DayNightCycle.GetCurrentState () == 3) {
				_clockDescriptionText.text = "Last chance to give your vote by clicking on the button located in front of player's tent! If you already voted, patience.";
				if(_localPM.isAlive)
					resetVoteButtonActive = true;
			} else if (DayNightCycle.GetCurrentState () == 4)
				_clockDescriptionText.text = "Votes have all been counted! Now finish your discussions and go back to your tent, night is falling upon the Village!";
			else if (DayNightCycle.GetCurrentState () == 5)
				_clockDescriptionText.text = "The night has just fallen, keep your eyes closed and wait until dawn!";
			else if (DayNightCycle.GetCurrentState () == 6)
				_clockDescriptionText.text = "Ô Seer, Ô All-Knowing, time for you to come out and discover someone's role!";
			else if (DayNightCycle.GetCurrentState () == 7) {
				_clockDescriptionText.text = "Time for the Werewolf to strike down their opponent! They are silently agreeing on who to devour...";
				if (_localPM.isAlive && _localPM.role == "Werewolf")
					resetVoteButtonActive = true;
			} else if (DayNightCycle.GetCurrentState() == 8)
				_clockDescriptionText.text = "Votes have all been counted! The victim's fate is now in the hands on the Witch!";

			_resetVoteButton.SetActive (resetVoteButtonActive);
		}

		/// <summary>
		/// This function handles the nomination of the Mayor, the killing of the Villager or Werewolf vote and the switching of Chat canals
		/// </summary>
		void VoteSystem() {
			if (DayNightCycle.GetCurrentState () == 1) {
				if (_formerState != DayNightCycle.GetCurrentState ()) {
					// This prevents the Witch from being detected because she's outside before anyone else
					if (_localPM.role == "Witch")
						_localPM.GoHome ();
					if (mostVotedPlayer != "") {
						if (_localPM.gameObject.name == mostVotedPlayer)
							_localPM.isAlive = false;
						GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
						foreach (GameObject player in players) {
							if (player.name == mostVotedPlayer && player.GetComponent<PlayerManager> ().isAlive == false)
								_localPM.votedPlayer = "";
						}
					} else
						_localPM.votedPlayer = "";
					_localPM.gameObject.GetComponent<Light> ().enabled = false;
					countDay++;
					if (!_localPM.isAlive || _localPM.role == "Werewolf")
						ChatManager.Instance.SwitchVillagerToWerewolf (true);
					else
						ChatManager.Instance.SwitchVillagerToWerewolf (false);	
					
					_formerState = DayNightCycle.GetCurrentState ();
				}
			} else if (DayNightCycle.GetCurrentState () == 2) {
				// We make sure that the future mayor is being elected by a single vote before checking
				if (countDay > 1 && _formerState != DayNightCycle.GetCurrentState () && singleVotes.Find (v => v.reason == "Mayor" ) == null) {
					CheckOrSetMayor ();

					_formerState = DayNightCycle.GetCurrentState ();
				}
			}else if (DayNightCycle.GetCurrentState () == 4) {
				if (_formerState != DayNightCycle.GetCurrentState ()) {
					if (countDay == 1 && PhotonNetwork.isMasterClient)
						mayorName = mostVotedPlayer;
					else if (countDay > 1 && _localPM.gameObject.name == mostVotedPlayer)
						_localPM.isAlive = false;						

					_formerState = DayNightCycle.GetCurrentState ();
				}
			} else if (DayNightCycle.GetCurrentState () == 5) {
				if (_formerState != DayNightCycle.GetCurrentState ()) {
					_localPM.votedPlayer = "";
					_localPM.gameObject.GetComponent<Light> ().enabled = true;
					countNight++;
					if (!_localPM.isAlive || _localPM.role == "Werewolf")
						ChatManager.Instance.SwitchVillagerToWerewolf (true);
					else
						ChatManager.Instance.SwitchVillagerToWerewolf (false);

					// Each night, the Seer can reveal someone's role
					if (_localPM.role == "Seer")
						_localPM.seerRevealingAvailable = true;
					
					// The first night, all Werewolves discover each other
					if (countNight == 1 && _localPM.role == "Werewolf") {
						GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
						foreach (GameObject player in players) {
							PlayerManager pM = player.GetComponent<PlayerManager> ();
							if (pM.role == "Werewolf")
								pM.isDiscovered = true;
						}
					}						

					_formerState = DayNightCycle.GetCurrentState ();
				}
			} else if (DayNightCycle.GetCurrentState () == 7) {
				if (_formerState != DayNightCycle.GetCurrentState ()) {
					// This prevents the Seer from being detected because she's outside when the Werewolves go out
					if (_localPM.role == "Seer")
						_localPM.GoHome ();

					_formerState = DayNightCycle.GetCurrentState ();
				}
			} else if (DayNightCycle.GetCurrentState () == 8) {
				if (_formerState != DayNightCycle.GetCurrentState ()) {
					// This prevents Werewolves from being detected because they are outside when the Witch go out
					if (_localPM.role == "Werewolf")
						_localPM.GoHome ();

					_formerState = DayNightCycle.GetCurrentState ();
				}
			}
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
