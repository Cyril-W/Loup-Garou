#if UNITY_5 && (!UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2 && ! UNITY_5_3) || UNITY_6
#define UNITY_MIN_5_4
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Player manager. 
	/// Handles Status of player.
	/// </summary>
	public class PlayerManager : Photon.PunBehaviour, IPunObservable
	{

		#region Public Variables

		[Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
		public static GameObject LocalPlayerInstance;

		[Tooltip("The current Status of our player")]
		public bool IsAlive = true;

		#endregion

		#region Private Variables



		#endregion

		#region MonoBehaviour CallBacks


		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start()
		{
			#if UNITY_MIN_5_4
			// Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
			{
				this.CalledOnLevelWasLoaded(scene.buildIndex);
			};
			#endif

			if (photonView.isMine) {
				SmoothCameraFollow.target = transform;
				Minimap.playerPos = transform;

				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				rends[1].material.color = Color.red;
				rends[2].material.color = Color.red;
				GetComponent<MinimapObjectID> ().image.color = Color.red;
			} else {
				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				rends[1].material.color = Color.blue;
				rends[2].material.color = Color.blue;
				GetComponent<MinimapObjectID> ().image.color = Color.blue;
			}
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		void Awake()
		{
			// #Important
			// used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
			if ( photonView.isMine)
			{
				PlayerManager.LocalPlayerInstance = this.gameObject;
			}
			// #Critical
			// we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
			DontDestroyOnLoad(this.gameObject);
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// </summary>
		void Update()
		{
			if (IsAlive == false)
				GameManager.Instance.LeaveRoom ();
		}

		#if !UNITY_MIN_5_4
		/// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
		void OnLevelWasLoaded(int level)
		{
			this.CalledOnLevelWasLoaded(level);
		}
		#endif

		#endregion

		#region Custom

		void CalledOnLevelWasLoaded(int level)
		{
			// check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
			if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
			{
				transform.position = new Vector3(0f, 5f, 0f);
			}
		}

		#endregion

		#region IPunObservable implementation

		void IPunObservable.OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				// We own this player: send the others our data
				stream.SendNext(IsAlive);
			}else{
				// Network player, receive data
				this.IsAlive = (bool)stream.ReceiveNext();
			}
		}

		#endregion
	}
}
