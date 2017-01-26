using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class FillVotedList : MonoBehaviour {

		static Text text;

		// Use this for initialization
		void Start () {
			text = GetComponent<Text> ();

			text.text = "";
		}

		public static int RefreshWho () {
			int numberOfVote = 0;
			text.text = "";

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player.GetComponent<PlayerManager> ().votedPlayerID == PhotonNetwork.player.ID) {
					text.text += player.name;
					numberOfVote++;
				}
			}

			return numberOfVote;
		}
	}
}
