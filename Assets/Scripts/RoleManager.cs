using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Role manager. 
	/// Handles roles of players.
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
		int _nbWerewolfAlive = 0;


		#endregion


		#region MonoBehaviour CallBacks


		void Start () {
			nbPlayerAlive = PhotonNetwork.room.PlayerCount;

			_endgamePanel = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (3).gameObject;
			_endgamePanel.SetActive (false);
			_nbWerewolfAlive = Mathf.RoundToInt (PhotonNetwork.room.PlayerCount / 3);

			if (PhotonNetwork.isMasterClient) {
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				for (int i = 0; i < players.Length; i++) {
					GameObject temp = players[i];
					int randomIndex = Random.Range(i, players.Length);
					players[i] = players[randomIndex];
					players[randomIndex] = temp;
				}

				for (int i = 0; i < players.Length; i++) {
					string role;
					if (i <= _nbWerewolfAlive - 1)
						role = "Werewolf";
					else if (i == _nbWerewolfAlive)
						role = "Seer";
					else
						role = "Villager";
					players [i].GetComponent<PhotonView> ().RPC("SetPlayerRoleAndTent", PhotonTargets.All, new object[] { role, tents.GetChild(i).name });
				}
			}
		}
		
		// Update is called once per frame
		void Update () {
			Text infoText = GetComponentInChildren<Text> ();
			infoText.text = "~ Day" + VoteManager.Instance.countDay + " / Night" + VoteManager.Instance.countNight + " ~\nHere are all the roles:\n~~~~~~~~~~~~~~\n";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			nbPlayerAlive = players.Length;
			_nbWerewolfAlive = 0;
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				infoText.text += PlayerManager.GetProperName (player.name);
				if (pM.isAlive) {
					if (pM.role == "Werewolf")
						_nbWerewolfAlive++;
					if (!pM.isDiscovered)
						infoText.text += " [Alive] > Unknown\n";
					else
						infoText.text += " [Alive] > " + pM.role + "\n";
				} else {					
					nbPlayerAlive--;
					infoText.text += " [Dead] > " + pM.role + "\n";
				}
			}
			
			gameFinished = CheckIfGameFinished ();
			_endgamePanel.SetActive (gameFinished);
		}


		#endregion


		#region Custom


		bool CheckIfGameFinished () {
			string winnerRole = "";
			if (nbPlayerAlive == _nbWerewolfAlive)
				winnerRole = "Werewolf";
			else if (_nbWerewolfAlive == 0)
				winnerRole = "Villager";
			else
				return false;

			string pMrole = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().role;
			string victoryText;
			if (winnerRole == "Werewolf" && pMrole == winnerRole || winnerRole == "Villager" && pMrole != "Werewolf")
				victoryText = "Victory!";
			else
				victoryText = "Defeat...";
			victoryText += "\n\nTo leave the game, click the button above.";
			_endgamePanel.transform.GetChild(1).GetComponent<Text> ().text = victoryText;

			Sprite winnerSprite = Resources.Load ("Cards/" + winnerRole, typeof(Sprite)) as Sprite;
			if (winnerSprite != null)
				_endgamePanel.transform.GetChild (0).GetComponent<Image> ().sprite = winnerSprite;
			else
				Debug.Log ("No image was found for " + winnerRole);

			return true;
		}


		#endregion
	}
}
