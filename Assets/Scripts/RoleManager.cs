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

			_infoText = transform.GetChild (1).GetChild (0).GetComponentInChildren<Text> ();
			_dayText = transform.GetChild (1).GetChild (1).GetComponentInChildren<Text> ();
			_nightText = transform.GetChild (1).GetChild (2).GetComponentInChildren<Text> ();

			_endgamePanel = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (4).gameObject;
			_endgamePanel.SetActive (false);
			// The average number of Werewolf per game is 1/3 of the total number of player
			_nbWerewolfAlive = Mathf.RoundToInt (PhotonNetwork.room.PlayerCount / 3);

			if (PhotonNetwork.isMasterClient) {
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				// We need to shuffle both:
				// - the player list, otherwise the role are attributed in the order of connection
				int randomIndex;
				for (int i = 0; i < players.Length; i++) {
					GameObject temp = players [i];
					randomIndex = Random.Range(i, players.Length);
					players [i] = players [randomIndex];
					players [randomIndex] = temp;
				}
				// - the player house, otherwise the roles are detected by the position in the village
				List<int> houseIndexes = new List<int> ();
				for (int i = 0; i < players.Length; i++) {
					randomIndex = Random.Range(0, players.Length);
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
						role = "Hunter";
					else
						role = "Villager";
					players [i].GetComponent<PhotonView> ().RPC("SetPlayerRoleAndTent", PhotonTargets.All, new object[] { role, tents.GetChild(houseIndexes[i]).name });
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
				_infoText.text += PlayerManager.GetProperName (player.name);
				if (pM.isAlive) {
					if (pM.role == "Werewolf")
						_nbWerewolfAlive++;
					if (!pM.isDiscovered)
						_infoText.text += " > ( ? )\n";
					else
						_infoText.text += " > " + pM.role + "\n";
				} else {					
					nbPlayerAlive--;
					_infoText.text += " [┼] > " + pM.role + "\n";
				}
			}
			
			gameFinished = CheckIfGameFinished ();
			_endgamePanel.SetActive (gameFinished);
		}


		#endregion


		#region Custom


		/// <summary>
		/// The conditions of victory depends on your being a Werewolf or not: if there is only Werewolves left,they win! If they are all dead, everyone else win!
		/// </summary>
		bool CheckIfGameFinished () {
			if (DayNightCycle.isDebugging)
				return false;
			
			string winnerRole = "";
			if (nbPlayerAlive == _nbWerewolfAlive)
				winnerRole = "Werewolf";
			else if (_nbWerewolfAlive == 0)
				winnerRole = "Villager";
			else
				return false;

			PlayerManager localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			string cardToDisplay;
			string victoryText;
			if (localPM.isAlive) {
				cardToDisplay = winnerRole;
				string localPlayerRole = localPM.role;
				if (winnerRole == "Werewolf" && localPlayerRole == winnerRole || winnerRole == "Villager" && localPlayerRole != "Werewolf")
					victoryText = "Victory!";
				else
					victoryText = "Defeat...";
			} else {
				cardToDisplay = "Dead";
				victoryText = "You died during the game...";
			}
			victoryText += "\n\nTo leave the game, click the button above.";
			_endgamePanel.transform.GetChild(1).GetComponent<Text> ().text = victoryText;

			Sprite displayedSprite = Resources.Load ("Cards/" + cardToDisplay, typeof(Sprite)) as Sprite;
			if (displayedSprite != null)
				_endgamePanel.transform.GetChild (0).GetComponent<Image> ().sprite = displayedSprite;
			else
				Debug.Log ("No image was found for " + cardToDisplay);

			return true;
		}


		#endregion
	}
}
