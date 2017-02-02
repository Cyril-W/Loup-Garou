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

		[Tooltip("Is our local player ready?")]
		public bool isReady = false;

		[Tooltip("The player you voted against")]
		public string votedPlayer;

		[Tooltip("The role you were attributed")]
		public string role;

		[Tooltip("The tent you were attributed")]
		public string tent;


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
				PlayerManager.LocalPlayerInstance = gameObject;

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
			}else {
				Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer> ();
				renderers[0].enabled = isAlive;
				renderers[1].enabled = isAlive;
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
				// transform.position = new Vector3 (transform.position.x, 2f, transform.position.y);
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
			this.role = role;
			this.tent = tent;
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
