using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Player manager. 
	/// Handles Status of player and who he voted against.
	/// </summary>
	public class PlayerManager : Photon.PunBehaviour, IPunObservable
	{
		#region Public Variables


		[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
		public static GameObject LocalPlayerInstance;

		[Tooltip("Is our local player alive?")]
		public bool isAlive = true;

		[Tooltip("Is our local player a male?")]
		public bool isMale = true;

		[Tooltip("The player you voted against")]
		public string votedPlayer;

		[Tooltip("The role you were attributed")]
		public string role;

		[Tooltip("Is your role discovered (not synced on Photon)")]
		public bool isDiscovered = false;

		[Tooltip("The tent you were attributed")]
		public string tent;


		#endregion


		#region Private Variables


		Transform _rolePanel;


		#endregion


		#region MonoBehaviour CallBacks


		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		void Awake()
		{		
			Button[] swapButtons = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild(4).GetChild(2).GetComponentsInChildren<Button>();
			swapButtons[0].onClick.AddListener (delegate { SwapGender (); });
			swapButtons[1].onClick.AddListener (delegate { SwapGender (); });
			gameObject.name = photonView.owner.NickName;

			isAlive = true;

			if (photonView.isMine) {
				isDiscovered = true;
				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				rends[0].material.SetColor("_TintColor", Color.red);
				rends[1].material.SetColor("_TintColor", Color.red);
				rends[2].material.color = Color.red;
			} else {
				isDiscovered = false;
				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				rends[0].material.SetColor("_TintColor", Color.blue);
				rends[1].material.SetColor("_TintColor", Color.blue);
				rends[2].material.color = Color.blue;
			}

			GetComponentInChildren<TextMesh> ().text = PlayerManager.GetProperName(gameObject.name);
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// </summary>
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
				} else {
					if (photonView.isMine) {
						GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
						foreach (GameObject player in players)
							player.GetComponent<PlayerManager> ().isDiscovered = true;
						votedPlayer = "";
						if (VoteManager.Instance.mayorName == gameObject.name)
							VoteManager.Instance.StartOneShotVote ("Mayor");
					}
					if (!isDiscovered)
						isDiscovered = true;
					transform.GetChild (0).gameObject.SetActive (false);
					transform.GetChild (1).gameObject.SetActive (false);
					transform.GetChild (2).gameObject.SetActive (true);
				}

				if (photonView.isMine)
					DisplayRole ();
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


		void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			if (scene.buildIndex == 2) {
				_rolePanel = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (1);

				gameObject.GetComponent<Rigidbody>().isKinematic = false;
				if(isMale)
					transform.GetChild (0).gameObject.SetActive (true);
				else
					transform.GetChild (1).gameObject.SetActive (true);
				transform.GetChild (2).gameObject.SetActive (false);

				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				if(photonView.isMine) {
					rends[1].material.color = Color.red;
					rends[2].material.color = Color.red;
				} else {
					rends[1].material.color = Color.blue;
					rends[2].material.color = Color.blue;
				}

				if(!photonView.isMine)
					GetComponent<MinimapObjectID> ().enabled = true;
				GetComponent<PlayerAnimatorManager> ().enabled = true;
			}
		}

		public static string GetProperName(string name) {
			int nbToKeep = 0;
			foreach (char c in name) {
				if (c != ' ' && !char.IsNumber (c))
					nbToKeep++;
				else
					return name.Substring (0, nbToKeep);
			}
			return name;
		}
			
		[PunRPC]
		public void SetPlayerRoleAndTent (string role, string tent) {
			// outside of the if because it's not synced by the network! Whereas the role must (because it can change!)
			this.tent = tent;

			if (photonView.isMine)
				this.role = role;
		}

		void DisplayRole() {
			string roleText = "";
			Sprite roleSprite;
			if (!isAlive) {
				roleText = "Dead\nYou can still hear but not talk to the others. Either leave the game or wait until a Witch revives you.";
				roleSprite = Resources.Load ("Cards/Card", typeof(Sprite)) as Sprite;
			} else {
				if (role == "Villager")
					roleText = role + "\nYour aim is to eliminate all Werewolves from the game. The only way to achieve that is to vote against a player during the day.";
				else if (role == "Werewolf")
					roleText = role + "\nYour aim is to eliminate all Villagers from the game. To achieve this, you can vote one more time against a player, at night.";
				roleSprite = Resources.Load ("Cards/" + role, typeof(Sprite)) as Sprite;
			}
			_rolePanel.GetChild(0).GetComponentInChildren<Text> ().text = roleText;
			if (roleSprite != null)
				_rolePanel.GetChild (1).GetComponent<Image> ().sprite = roleSprite;
			else
				Debug.Log ("No image was found for " + role);

			if (gameObject.name == VoteManager.Instance.mayorName) {
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

		public void SwapGender() {
			PlayerManager pM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			pM.isMale = !(pM.isMale);
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
				stream.SendNext (role);
			}else{
				// Network player, receive data
				this.isAlive = (bool)stream.ReceiveNext();
				this.isMale = (bool)stream.ReceiveNext();
				this.votedPlayer = (string)stream.ReceiveNext ();
				this.role = (string)stream.ReceiveNext ();
			}
		}


		#endregion
	}
}
