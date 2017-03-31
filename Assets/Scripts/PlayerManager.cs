using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Player manager. 
	/// Handles the two different version of the Player: its spirit, when it is in the lobby, and its incarnation when it is in game.
	/// </summary>
	public class PlayerManager : Photon.PunBehaviour, IPunObservable {

		#region Public Variables


		public static GameObject LocalPlayerInstance;
		public bool isAlive = true;
		/// <summary>
		/// This variable is synced accross the Network and helps determine which 3D model should be used.
		/// </summary>
		public bool isMale = true;
		public string votedPlayer;
		/// <summary>
		/// This variable has two different roles: it indicates if the player is Ready in the lobby, and then it represents the secret role of the player.
		/// </summary>
		public string role;
		/// <summary>
		/// This variable is not synced, as it depends: each client knows if this player is discovered by the local player or not.
		/// </summary>
		public bool isDiscovered = false;
		public GameObject house;

		public bool deathPotionAvailable = false;
		public bool lifePotionAvailable = false;
		public bool seerRevealingAvailable = false;
		public bool hunterBulletAvailable = false;
		public bool littleGirlSpying = false;

		public static int sizeOfID = 4;


		#endregion


		#region Private Variables


		/// <summary>
		/// This serves to display the role of the local player and if he's the Mayor or not.
		/// </summary>
		Transform _rolePanel;
		Text _votedPlayerText;


		#endregion


		#region MonoBehaviour CallBacks


		void Awake()
		{		
			gameObject.name = photonView.owner.NickName;
			GetComponentInChildren<TextMesh> ().text = PlayerManager.GetProperName(gameObject.name);

			if (photonView.isMine)
				isDiscovered = true;
							
			OnSceneLoaded (SceneManager.GetActiveScene(), LoadSceneMode.Single);
		}
			
		void Update()
		{
			if (SceneManagerHelper.ActiveSceneBuildIndex == 1) {
				if (role == "Ready")
					transform.position = new Vector3 (transform.position.x, Mathf.PingPong (Time.time, 1f) + 3, transform.position.z);
			} else {
				if (isAlive) {
					transform.GetChild (0).gameObject.SetActive (isMale);
					transform.GetChild (1).gameObject.SetActive (!isMale);
					transform.GetChild (2).gameObject.SetActive (false);
					if (photonView.isMine && role == "LittleGirl")
						PlayerAnimatorManager.compas.GetComponent<Image> ().color = (littleGirlSpying) ? Color.yellow : Color.red;
				} else {
					if (photonView.isMine) {	
						if (VoteManager.Instance.mayorName == gameObject.name) {
							VoteManager.Instance.mayorName = "";
							VoteManager.Instance.StartSingleVote ("Mayor");
						}
						if (hunterBulletAvailable)
							VoteManager.Instance.StartSingleVote ("Hunter");
						if (VoteManager.Instance.singleVotes.Count == 0) {
							GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
							foreach (GameObject player in players)
								player.GetComponent<PlayerManager> ().isDiscovered = true;
						}
					}
					if (!isDiscovered)
						isDiscovered = true;
					transform.GetChild (0).gameObject.SetActive (false);
					transform.GetChild (1).gameObject.SetActive (false);
					transform.GetChild (2).gameObject.SetActive (true);
				}

				if (photonView.isMine) {
					DisplayRole ();
					if(_votedPlayerText != null)
						_votedPlayerText.text = PlayerManager.GetProperName(votedPlayer);
				}
			}
		}

		void OnEnable() {
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void OnDisable() {
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}


		#endregion


		#region Custom


		/// <summary>
		/// This switches from the ghost form to the incarnated form, depending on the scene loaded.
		/// </summary>
		void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			if (scene.buildIndex == 1) {	
				gameObject.GetComponent<Rigidbody>().isKinematic = true;
				transform.GetChild (0).gameObject.SetActive (false);
				transform.GetChild (1).gameObject.SetActive (false);
				transform.GetChild (2).gameObject.SetActive (true);

				// To dinstinguish whether the 3D model is our or not from a glance: red is our, blue is other's
				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				if(photonView.isMine) {
					rends[0].material.SetColor("_TintColor", Color.red);
					rends[1].material.SetColor("_TintColor", Color.red);
					rends[2].material.color = Color.red;
				} else {
					rends[0].material.SetColor("_TintColor", Color.blue);
					rends[1].material.SetColor("_TintColor", Color.blue);
					rends[2].material.color = Color.blue;
				}

				GetComponent<MinimapObjectID> ().enabled = false;
				GetComponent<PlayerAnimatorManager> ().enabled = false;

				GetComponentInChildren<TextMesh> ().text = PlayerManager.GetProperName(gameObject.name);
			} else if (scene.buildIndex == 2) {
				_rolePanel = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (1);
				_votedPlayerText = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (3).GetComponentInChildren<Text> ();

				gameObject.GetComponent<Rigidbody>().isKinematic = false;
				if(isMale)
					transform.GetChild (0).gameObject.SetActive (true);
				else
					transform.GetChild (1).gameObject.SetActive (true);
				transform.GetChild (2).gameObject.SetActive (false);

				// To dinstinguish whether the 3D model is our or not from a glance: red is our, blue is other's
				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				if(photonView.isMine) {
					rends[1].material.color = Color.red;
					rends[2].material.color = Color.red;
					VoteManager.Instance.resetVoteButton.GetComponent<Button> ().onClick.AddListener (delegate {
						ClearVotedPlayer ();
					});
					// clear the name of the local player to display it at the bottom of the screen 
					_rolePanel.GetChild(4).GetComponentInChildren<Text> ().text = PlayerManager.GetProperName(gameObject.name);
					GetComponentInChildren<TextMesh> ().text = "";
				} else {
					rends[1].material.color = Color.blue;
					rends[2].material.color = Color.blue;
					GetComponent<MinimapObjectID> ().enabled = true;
				}
				GetComponent<PlayerAnimatorManager> ().enabled = true;
			}
		}

		/// <summary>
		/// This eliminates the Numbers at the end of the player name.
		/// </summary>
		public static string GetProperName(string name) {
			if (name == "")
				return name;
			else {
				int isThereSpace = (name [name.Length - 1] == ' ') ? 1 : 0;
				return name.Substring (0, name.Length - sizeOfID - isThereSpace);
			}
		}
			
		[PunRPC]
		public void SetPlayerRoleAndTent (string role, string house) {
			GameObject[] houses = GameObject.FindGameObjectsWithTag ("House");
			foreach (GameObject h in houses) {
				if (h.name == house)
					this.house = h;
			}
			this.role = role;

			if (photonView.isMine) {
				if (role == "Witch") {
					lifePotionAvailable = true;
					deathPotionAvailable = true;
				} else if (role == "Hunter")
					hunterBulletAvailable = true;
				else if (role == "LittleGirl") {					
					EventTrigger trigger = GameObject.FindGameObjectWithTag("Canvas").transform.GetChild(0).GetChild(4).GetComponent<EventTrigger>( );
					EventTrigger.Entry entryDown = new EventTrigger.Entry( );
					entryDown.eventID = EventTriggerType.PointerDown;
					entryDown.callback.AddListener( ( data ) => { OnPointerDownDelegate( (PointerEventData)data ); } );
					trigger.triggers.Add( entryDown );
					EventTrigger.Entry entryUp = new EventTrigger.Entry( );
					entryUp.eventID = EventTriggerType.PointerUp;
					entryUp.callback.AddListener( ( data ) => { OnPointerUpDelegate( (PointerEventData)data ); } );
					trigger.triggers.Add( entryUp );
				}
			}
		}

		public void OnPointerDownDelegate( PointerEventData data )
		{
			PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().littleGirlSpying = true;
		}

		public void OnPointerUpDelegate( PointerEventData data )
		{
			PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().littleGirlSpying = false;
		}

		[PunRPC]
		public void KillPlayer () {
			if (photonView.isMine)
				this.isAlive = false;
		}

		[PunRPC]
		public void ResetPlayerVote () {
			if (photonView.isMine)
				this.votedPlayer = "";
		}

		/// <summary>
		/// This is usefull to avoid the players from being detected because they didn't come home at night when their turn is over.
		/// /!\ If you stay out when the night falls, this will not be triggered /!\
		/// </summary>
		public void GoHome() {
			if (house.GetComponent<HouseManager> ().ownerInside == false)
				gameObject.transform.position = house.transform.position + Vector3.up * 2f;
		}

		/// <summary>
		/// This updates the role card of the player on its GUI and also shows him if he's the Mayor or not.
		/// </summary>
		void DisplayRole() {
			string roleText = "";
			string roleName = role;
			if (!isAlive) {
				roleText = "Dead\nYou can still hear but not talk to the others. You can leave the game or spy other player.";
				roleName = "Dead";
			} else {
				if (role == "Villager")
					roleText = role + "\nYour aim is to eliminate all Werewolves from the game. The only way to achieve that is to vote against a player during the day.";
				else if (role == "Werewolf")
					roleText = role + "\nYour aim is to eliminate all Villagers from the game. To achieve this, you can vote one more time against a player, at night.";
				else if (role == "Seer")
					roleText = role + "\nYour aim is to eliminate all Werewolves from the game. To achieve this, you can discover the role of someone each night.";
				else if (role == "Witch") {
					roleText = role + "\nYour aim is to eliminate all Werewolves from the game. To achieve this, you can kill one person, and save one person per game.\n";
					if(!lifePotionAvailable || !deathPotionAvailable)
						roleName += (lifePotionAvailable ? "1" : "0") + (deathPotionAvailable ? "1" : "0");
				} else if (role == "Hunter")
					roleText = role + "\nYour aim is to eliminate all Werewolves from the game. To achieve this, you can shot dead someone when you get killed.";
				else if (role == "LittleGirl")
					roleText = role + "\nYour aim is to eliminate all Werewolves from the game. To achieve this, you can spy them at night (but get revealed to them !).";
			}
			Sprite roleSprite = Resources.Load ("Cards/" + roleName, typeof(Sprite)) as Sprite;
			_rolePanel.GetChild(0).GetComponentInChildren<Text> ().text = roleText;
			if (roleSprite != null)
				_rolePanel.GetChild (1).GetComponent<Image> ().sprite = roleSprite;
			else
				Debug.Log ("No image was found for " + role);

			if (VoteManager.Instance != null && gameObject.name == VoteManager.Instance.mayorName) {
				roleText = "Mayor\nAs the representant of your people, your vote counts for double. If you die, you will have to chose your successor.";
				roleSprite = Resources.Load ("Cards/MayorDay", typeof(Sprite)) as Sprite;
			} else {
				roleText = "You're not the Mayor\nAs the representant of you all, his vote counts for double. If he dies, he will chose his successor.";
				roleSprite = Resources.Load ("Cards/MayorNight", typeof(Sprite)) as Sprite;
			}
			_rolePanel.GetChild(2).GetComponentInChildren<Text> ().text = roleText;
			if (roleSprite != null)
				_rolePanel.GetChild (3).GetComponent<Image> ().sprite = roleSprite;
			else
				Debug.Log ("No image was found for the Mayor (Day/Night)");
		}

		/// <summary>
		/// This event is triggered in the lobby, when the local player clicks the button to change its gender.
		/// </summary>
		public void SwapGender() {
			PlayerManager pM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			pM.isMale = !(pM.isMale);
		}

		/// <summary>
		/// This event is triggered when the local player clicks the button to clear its vote.
		/// </summary>
		public void ClearVotedPlayer () {
			votedPlayer = "";
		}


		#endregion


		#region IPunObservable implementation


		void IPunObservable.OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				// We own this player: send the others our data
				stream.SendNext(isAlive);
				stream.SendNext(isMale);
				stream.SendNext(votedPlayer);
				stream.SendNext(role);
				stream.SendNext (littleGirlSpying);
			}else{
				// Network player, receive data
				this.isAlive = (bool)stream.ReceiveNext();
				this.isMale = (bool)stream.ReceiveNext();
				this.votedPlayer = (string)stream.ReceiveNext ();
				this.role = (string)stream.ReceiveNext ();
				this.littleGirlSpying = (bool)stream.ReceiveNext ();
			}
		}


		#endregion
	}
}
