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

		[Tooltip("The Sprites used to display the roles of the players")]
		public List<Sprite> roleSprites;

		[Tooltip("Is our local player alive?")]
		public bool isAlive = true;

		[Tooltip("Is our local player ready?")]
		public bool isReady = false;

		[Tooltip("The player you voted against")]
		public string votedPlayer;

		[Tooltip("The role you were attributed")]
		public string role;

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
			gameObject.name = photonView.owner.NickName;

			isReady = false;
			isAlive = true;

			if (photonView.isMine) {
				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				rends[0].material.SetColor("_TintColor", Color.red);
				rends[1].material.SetColor("_TintColor", Color.red);
				rends[2].material.color = Color.red;
			} else {
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
				if (isReady)
					transform.position = new Vector3 (transform.position.x, Mathf.PingPong (Time.time, 1f) + 3, transform.position.z);
			} else {
				transform.GetChild (0).gameObject.SetActive (isAlive);
				transform.GetChild (2).gameObject.SetActive (!isAlive);
				if (photonView.isMine) {
					SetTextForRole (role);
					_rolePanel.GetComponentInChildren<Button> ().interactable = isAlive;
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


		void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			if (scene.buildIndex == 2) {
				_rolePanel = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (1);

				gameObject.GetComponent<Rigidbody>().isKinematic = false;
				transform.GetChild (0).gameObject.SetActive (true);
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

			if (photonView.isMine) {
				this.role = role;
				Button roleButton = _rolePanel.GetComponentInChildren<Button> ();
				SpriteState buttonState = roleButton.spriteState;
				Sprite roleSprite = roleSprites.Find (s => s.name == this.role);
				if (roleSprite != null) {
					buttonState.pressedSprite = roleSprite;
					roleButton.spriteState = buttonState;
					SetTextForRole (role);
				} else
					Debug.Log ("No image was found for " + this.role);
			}
		}

		void SetTextForRole(string role) {
			string roleText = "";
			if (!isAlive) {
				role = "Dead";
				roleText = "\nYou can still hear but not talk to the others. Either leave the game or wait until a Witch revives you.";
			} else if (role == "Villager")
				roleText = "\nYour aim is to eliminate all Werewolves from the game. The only way to achieve that is to vote against a player during the day.";
			else if (role == "Werewolf")
				roleText = "\nYour aim is to eliminate all Villagers from the game. To achieve this, you can vote one more time against a player, at night.";
			
			_rolePanel.GetChild(0).GetComponentInChildren<Text> ().text = role + roleText;
		}


		#endregion


		#region IPunObservable implementation


		void IPunObservable.OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				// We own this player: send the others our data
				stream.SendNext(isAlive);
				stream.SendNext(isReady);
				stream.SendNext(votedPlayer);
				stream.SendNext (role);
			}else{
				// Network player, receive data
				this.isAlive = (bool)stream.ReceiveNext();
				this.isReady = (bool)stream.ReceiveNext();
				this.votedPlayer = (string)stream.ReceiveNext ();
				this.role = (string)stream.ReceiveNext ();
			}
		}


		#endregion
	}
}
