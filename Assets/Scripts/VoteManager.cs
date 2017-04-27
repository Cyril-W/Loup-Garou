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

		public string mostVotedPlayer;
		public List<SingleVoteManager> singleVotes = new List<SingleVoteManager>();
		/// <summary>
		/// The button used by the local Player to reset his vote only during the voting phases.
		/// </summary>
		public GameObject resetVoteButton;

		public static VoteManager Instance;


		#endregion


		#region Private Variables


		Text _infoText;
		PlayerManager _localPM;
		GameObject _minimapHidingSeal;
		GameObject _spyButton;
		static List<string> _votedPlayers;
		int _formerState = 1;
		AudioSource _audioSource;


		#endregion


		#region MonoBehaviour CallBacks


		void Awake () {
			Instance = this;
			_infoText = transform.GetChild (1).GetChild (2).GetComponentInChildren<Text> ();
			resetVoteButton = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (3).GetComponentInChildren<Button> ().gameObject;
			resetVoteButton.SetActive (false);
			_minimapHidingSeal = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (0).GetChild (2).gameObject;
			_minimapHidingSeal.SetActive (false);
			_spyButton = _minimapHidingSeal.transform.parent.GetChild(4).gameObject;
			_spyButton.SetActive(false);
			_localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			_votedPlayers = new List<string> ();
			_audioSource = GetComponent<AudioSource> ();
		}

		void Update () {
			if (RoleManager.gameFinished == true)
				return;
			
			RefreshVotedPlayers ();

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

		void OnTriggerStay(Collider other) {
			if (other.gameObject == PlayerManager.LocalPlayerInstance) {
				Transform worldCanvas = transform.GetChild (1);
				worldCanvas.GetChild (2).gameObject.SetActive (true);
				if (worldCanvas.localScale.x <= 0.015f) 
					worldCanvas.localScale += 0.0005f * Vector3.one;
				if (worldCanvas.localPosition.y < 0.5f)
					worldCanvas.localPosition += 0.1f * Vector3.up;

			}
		}

		void OnTriggerExit(Collider other) {
			if (other.gameObject == PlayerManager.LocalPlayerInstance) {
				transform.GetChild (1).GetChild (2).gameObject.SetActive (false);
				transform.GetChild (1).localScale = 0.005f * Vector3.one;
				transform.GetChild (1).localPosition = -0.51f * Vector3.forward;
			}
		}


		#endregion


		#region Custom


		/// <summary>
		/// When the Leave button is clicked, this function erase the player from the all the single votes and disconnect him.
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
			_votedPlayers.Clear ();

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			_infoText.text = "";
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.votedPlayer != "" && pM.isAlive) {
					_votedPlayers.Add (pM.votedPlayer);
					// Double vote of the Mayor doesn't count during the night if he's a Werewolf!
					if (DayNightCycle.GetCurrentState() < 5 && player.name == mayorName)
						_votedPlayers.Add (pM.votedPlayer);
				}
				if (player.name == mayorName)
					_infoText.text += "[M] ";
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
		/// This function handles the nomination of the Mayor, the killing of the Villager or Werewolf vote and the switching of Chat canals
		/// </summary>
		void VoteSystem() {
			if (_formerState != DayNightCycle.GetCurrentState ()) {
				_formerState = DayNightCycle.GetCurrentState ();
				DayNightCycle.Instance.UpdateClockText (true);

				if (_formerState == 1) {
					// This prevents the Witch from being detected because she's outside before anyone else
					if (_localPM.role == "Witch")
						_localPM.GoHome ();
					if (mostVotedPlayer != "") {
						if (PhotonNetwork.isMasterClient) {
							GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
							foreach (GameObject player in players) {
								if (player.name == mostVotedPlayer)
									player.GetComponent<PhotonView> ().RPC ("KillPlayer", PhotonTargets.All, new object[] { });
								else
									player.GetComponent<PhotonView> ().RPC ("ResetPlayerVote", PhotonTargets.All, new object[] { });
							}
						}
					} else
						_localPM.votedPlayer = "";
					_localPM.gameObject.GetComponent<Light> ().enabled = false;
					_minimapHidingSeal.SetActive (false);
					countDay++;
					if (!_localPM.isAlive || _localPM.role == "Werewolf")
						ChatManager.Instance.SwitchVillagerToWerewolf (true);
					else
						ChatManager.Instance.SwitchVillagerToWerewolf (false);
				} else if (_formerState == 2) {
					if (countDay > 1 && singleVotes.Find (v => v.reason == "Mayor") == null)
						CheckOrSetMayor ();

					if (_localPM.isAlive)
						resetVoteButton.SetActive (true);

					_formerState = DayNightCycle.GetCurrentState ();
				} else if (_formerState == 4) {
					if (countDay == 1 && PhotonNetwork.isMasterClient)
						mayorName = mostVotedPlayer;
					else if (countDay > 1 && _localPM.gameObject.name == mostVotedPlayer)
						_localPM.isAlive = false;						

					resetVoteButton.SetActive (false);
				} else if (_formerState == 5) {
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
				} else if (_formerState == 6) {
					if (_localPM.isAlive && _localPM.role != "Seer")
						_minimapHidingSeal.SetActive (true);
				} else if (_formerState == 7) {
					_audioSource.PlayOneShot (_audioSource.clip); // wolf howling

					// This prevents the Seer from being detected because she's outside when the Werewolves go out
					if (_localPM.role == "Seer") {
						_localPM.GoHome ();
						_minimapHidingSeal.SetActive (true);
					} else if (_localPM.role == "Werewolf") {
						_minimapHidingSeal.SetActive (false);
						if (_localPM.isAlive)
							resetVoteButton.SetActive (true);						
					}
					
					// During this phase, the Little Girl can spy the Werewolves by clicking the button "Spy"
					if (_localPM.role == "LittleGirl" && _localPM.isAlive)
						_spyButton.SetActive (true);					
				} else if (_formerState == 8) {
					// This prevents Werewolves from being detected because they are outside when the Witch go out
					if (_localPM.role == "Werewolf") {
						_localPM.GoHome ();
						_minimapHidingSeal.SetActive (true);
						resetVoteButton.SetActive (false);
					}

					if (_localPM.role == "LittleGirl") {
						_minimapHidingSeal.SetActive (true);
						_localPM.littleGirlSpying = false;
						_spyButton.SetActive(false);
					}

					if (_localPM.role == "Witch")
						_minimapHidingSeal.SetActive (false);	
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
