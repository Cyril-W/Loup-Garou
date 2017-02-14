using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Single Vote manager. 
	/// Handles the vote from the localPlayer, which is synced by Photon.
	/// </summary>
	public class SingleVoteManager : MonoBehaviour {
		#region Public Variables


		[Tooltip("The Prefab used to populate the player list")]
		public GameObject voteButton;

		public Text title;
		public Text description;
		public float secondsToVote = 30f;
		public bool isOneShot = false;

		public List<PlayerButton> playerButtons;


		#endregion


		#region Private Variables


		Transform _votePanel;
		PlayerManager _localPM;


		#endregion


		#region MonoBehaviour CallBacks


		// Use this for initialization
		void Awake () {
			_votePanel = transform.GetChild (0).GetChild (2);

			_localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			_localPM.votedPlayer = "";

			playerButtons = new List<PlayerButton> ();

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				if (player != PlayerManager.LocalPlayerInstance) {
					PlayerManager pM = player.GetComponent<PlayerManager> ();
					if(pM.isAlive)
						RegisterPlayerForVote (pM);
				}
			}

			_votePanel.GetComponentInChildren<ScrollRect> ().verticalNormalizedPosition = 1;

			if (playerButtons.Count == 0)
				secondsToVote = 0;
		}

		// Update is called once per frame
		void Update () {
			_votePanel.GetChild(1).GetComponent<Text> ().text = Mathf.RoundToInt(secondsToVote).ToString();
			secondsToVote -= Time.deltaTime;
			if (secondsToVote <= 0f) {
				secondsToVote = 0f;

				gameObject.SetActive (false);
				if (isOneShot)
					VoteManager.Instance.gameObject.GetComponent<PhotonView> ().RPC ("AnalyzeOneShotResult", PhotonTargets.MasterClient, new object[] {
						this,
						_localPM.gameObject.name,
						""
					});
			} else {
				if(!isOneShot)
					gameObject.SetActive (_localPM.isAlive);
			}
		}


		#endregion


		#region Custom


		public void OnClicked(string playerClicked) {
			if (isOneShot) {
				gameObject.SetActive (false);
				VoteManager.Instance.gameObject.GetComponent<PhotonView> ().RPC ("AnalyzeOneShotResult", PhotonTargets.MasterClient, new object[] {
					this,
					_localPM.gameObject.name,
					playerClicked
				});
			} else {
				PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().votedPlayer = playerClicked;
				secondsToVote = 0;
			}
		}

		public void OnSkipVote() {
			if (isOneShot) {
				gameObject.SetActive (false);
				VoteManager.Instance.gameObject.GetComponent<PhotonView> ().RPC ("AnalyzeOneShotResult", PhotonTargets.MasterClient, new object[] {
					this,
					_localPM.gameObject.name,
					""
				});
			} else
				secondsToVote = 0;			
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

			btn.transform.SetParent (_votePanel.GetChild(0).GetChild(0));
			btn.onClick.AddListener (delegate { OnClicked (playerName); });

			playerButtons.Add (new PlayerButton() {Button = btn, PlayerName = playerName});
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
