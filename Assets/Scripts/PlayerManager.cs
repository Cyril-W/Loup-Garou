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

		#region MonoBehaviour CallBacks


		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start()
		{		
			if (photonView.isMine) {
				SmoothCameraFollow.target = transform;
				Minimap.playerPos = transform;

				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				rends[1].material.color = Color.red;
				rends[2].material.color = Color.red;
			} else {
				Renderer[] rends = GetComponentsInChildren<Renderer> ();
				rends[1].material.color = Color.blue;
				rends[2].material.color = Color.blue;
			}
			GetComponentInChildren<TextMesh> ().text = photonView.owner.NickName;
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		void Awake()
		{
			// #Important
			// used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
			if (photonView.isMine)
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
