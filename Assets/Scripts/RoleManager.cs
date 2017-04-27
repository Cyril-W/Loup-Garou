using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Role manager. 
	/// Handles roles of players and checks if the game is finished or not.
	/// </summary>
	public class RoleManager : Photon.PunBehaviour {
		
		#region Public Variables


		[Tooltip("The Transform containing all the tents")]
		public Transform tents;

		public static bool gameFinished = false;
		public static int nbPlayerAlive;


		#endregion


		#region Private Variables


		GameObject _endgamePanel;
		Text _infoText;
		Text _dayText;
		Text _nightText;
		int _nbWerewolfAlive = 0;


		#endregion


		#region MonoBehaviour CallBacks


		void Start () {
			nbPlayerAlive = PhotonNetwork.room.PlayerCount;

			_infoText = transform.GetChild (1).GetChild (4).GetComponentInChildren<Text> ();
			_dayText = transform.GetChild (1).GetChild (0).GetComponentInChildren<Text> ();
			_nightText = transform.GetChild (1).GetChild (1).GetComponentInChildren<Text> ();

			_endgamePanel = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (5).gameObject;
			_endgamePanel.SetActive (false);
			// The average number of Werewolf per game is 1/3 of the total number of player
			_nbWerewolfAlive = Mathf.RoundToInt (PhotonNetwork.room.PlayerCount / 3);

			if (PhotonNetwork.isMasterClient) {
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				if (!DayNightCycle.Instance.isDebugging) {
					// We need to shuffle both:
					// - the player list, otherwise the role are attributed in the order of connection
					int randomIndex;
					for (int i = 0; i < players.Length; i++) {
						GameObject temp = players [i];
						randomIndex = Random.Range (i, players.Length);
						players [i] = players [randomIndex];
						players [randomIndex] = temp;
					}
					// - the player house, otherwise the roles are detected by the position in the village
					List<int> houseIndexes = new List<int> ();
					for (int i = 0; i < players.Length; i++) {
						randomIndex = Random.Range (0, players.Length);
						while (houseIndexes.Contains (randomIndex))
							randomIndex = Random.Range (0, players.Length);
						houseIndexes.Add (randomIndex);

						string role;
						if (i <= _nbWerewolfAlive - 1)
							role = "Werewolf";
						else if (players.Length > 2 && i == _nbWerewolfAlive)
							role = "Seer";
						else if (players.Length > 3 && i == _nbWerewolfAlive + 1)
							role = "Witch";
						else if (players.Length > 4 && i == _nbWerewolfAlive + 2)
							role = "LittleGirl";
						else if (players.Length > 5 && i == _nbWerewolfAlive + 3)
							role = "Hunter";
						else
							role = "Villager";
						players [i].GetComponent<PhotonView> ().RPC ("SetPlayerRoleAndTent", PhotonTargets.All, new object[] {
							role,
							tents.GetChild (houseIndexes [i]).name
						});
					}
				} else if (players.Length == 1) {
					players [0].GetComponent<PhotonView> ().RPC ("SetPlayerRoleAndTent", PhotonTargets.All, new object[] {
						"Villager",
						tents.GetChild (0).name
					});
				} else if (players.Length == 2) {
					players [0].GetComponent<PhotonView> ().RPC ("SetPlayerRoleAndTent", PhotonTargets.All, new object[] {
						"Werewolf",
						tents.GetChild (0).name
					});
					players [1].GetComponent<PhotonView> ().RPC ("SetPlayerRoleAndTent", PhotonTargets.All, new object[] {
						"Witch",
						tents.GetChild (1).name
					});
				} else if (players.Length == 3) {
					players [0].GetComponent<PhotonView> ().RPC ("SetPlayerRoleAndTent", PhotonTargets.All, new object[] {
						"Werewolf",
						tents.GetChild (0).name
					});
					players [1].GetComponent<PhotonView> ().RPC ("SetPlayerRoleAndTent", PhotonTargets.All, new object[] {
						"Witch",
						tents.GetChild (1).name
					});
					players [2].GetComponent<PhotonView> ().RPC ("SetPlayerRoleAndTent", PhotonTargets.All, new object[] {
						"Hunter",
						tents.GetChild (2).name
					});
				} 
			}
		}
		
		void Update () {
			if (VoteManager.Instance != null) {
				_dayText.text = "Day" + VoteManager.Instance.countDay;
				_nightText.text = "Night" + VoteManager.Instance.countNight;
			} else {
				_dayText.text = "Day1";
				_nightText.text = "Night0";
			}
			
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			nbPlayerAlive = players.Length;
			_nbWerewolfAlive = 0;

			_infoText.text = "";
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.isAlive) {
					_infoText.text += PlayerManager.GetProperName (player.name);
					if (pM.role == "Werewolf")
						_nbWerewolfAlive++;
					if (!pM.isDiscovered)
						_infoText.text += " > ( ? )\n";
					else
						_infoText.text += " > " + pM.role + "\n";
				} else {					
					nbPlayerAlive--;
					_infoText.text += "[┼] " + PlayerManager.GetProperName (player.name) + " > " + pM.role + "\n";
				}
			}
			
			gameFinished = CheckIfGameFinished ();
			_endgamePanel.SetActive (gameFinished);
			if(gameFinished)
				_endgamePanel.transform.GetChild (0).gameObject.SetActive (PhotonNetwork.isMasterClient);
		}

		void OnTriggerStay(Collider other) {
			if (other.gameObject == PlayerManager.LocalPlayerInstance) {
				Transform worldCanvas = transform.GetChild (1);
				worldCanvas.GetChild (4).gameObject.SetActive (true);
				if (worldCanvas.localScale.x <= 0.015f) 
					worldCanvas.localScale += 0.0005f * Vector3.one;
				if (worldCanvas.localPosition.y < 0.5f)
					worldCanvas.localPosition += 0.1f * Vector3.up;
			}
		}

		void OnTriggerExit(Collider other) {
			if (other.gameObject == PlayerManager.LocalPlayerInstance) {
				transform.GetChild (1).GetChild (4).gameObject.SetActive (false);
				transform.GetChild (1).localScale = 0.005f * Vector3.one;
				transform.GetChild (1).localPosition = -0.51f * Vector3.forward;
			}
		}


		#endregion


		#region Custom


		/// <summary>
		/// When the Return to Lobby button is clicked, this function makes all the players return to the lobby of this locked room.
		/// </summary>
		public void ReturnToLobby()
		{
			if (PhotonNetwork.isMasterClient)
				PhotonNetwork.LoadLevel ("Lobby");			
		}

		/// <summary>
		/// The conditions of victory depends on your being a Werewolf or not: if there is only Werewolves left,they win! If they are all dead, everyone else win!
		/// </summary>
		bool CheckIfGameFinished () {
			bool isGameFinished = false;

			if (DayNightCycle.Instance.isDebugging == false) {			
				string winnerRole = "";
				if (nbPlayerAlive == _nbWerewolfAlive || _nbWerewolfAlive == 0) {
					if (nbPlayerAlive == _nbWerewolfAlive)
						winnerRole = "Werewolf";
					else if (_nbWerewolfAlive == 0)
						winnerRole = "Villager";
				
					string cardToDisplay;
					string victoryText;
					PlayerManager localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
					if (localPM.isAlive) {
						cardToDisplay = winnerRole;
						string localPlayerRole = localPM.role;
						if (winnerRole == "Werewolf" && localPlayerRole == winnerRole || winnerRole == "Villager" && localPlayerRole != "Werewolf")
							victoryText = "Victory!";
						else
							victoryText = "Defeat...";
					} else {
						cardToDisplay = "Dead";
						victoryText = "Defeat...\nYou died during the game...";
					}
					victoryText += "\n\nTo leave the game, click the button above.";
					_endgamePanel.transform.GetChild (2).GetComponent<Text> ().text = victoryText;

					Sprite displayedSprite = Resources.Load ("Cards/" + cardToDisplay, typeof(Sprite)) as Sprite;
					if (displayedSprite != null)
						_endgamePanel.transform.GetChild (1).GetComponent<Image> ().sprite = displayedSprite;
					else
						Debug.Log ("No image was found for " + cardToDisplay);
				
					isGameFinished = true;
				}
			}

			PlayerAnimatorManager.isBlocked = isGameFinished;
			return isGameFinished;
		}


		#endregion
	}
}
