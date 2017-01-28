using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

		[Tooltip("The current Status of our player")]
		public bool isAlive = true;

		[Tooltip("The ID player you voted against")]
		public int votedPlayerID;

		[Tooltip("The ID of the owner of the player")]
		public int playerID;


		#endregion

		#region Private Variables


		int numberOfVote = 0;


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
			gameObject.name = photonView.owner.NickName;
			playerID = photonView.ownerId;
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
			if ((DayNightCycle.currentTime <= 0.25f || DayNightCycle.currentTime > 0.375f) && votedPlayerID != 0) {
				votedPlayerID = 0;
			}

			if (isAlive == false)
				GameManager.Instance.LeaveRoom ();
			else {
				numberOfVote = VoteManager.RefreshWho ();
				if (numberOfVote > PhotonNetwork.room.PlayerCount / 2)
					isAlive = false;
			}
		}


		#endregion


		#region IPunObservable implementation

		void IPunObservable.OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				// We own this player: send the others our data
				stream.SendNext(isAlive);
				stream.SendNext(votedPlayerID);
			}else{
				// Network player, receive data
				this.isAlive = (bool)stream.ReceiveNext();
				this.votedPlayerID = (int)stream.ReceiveNext ();
			}
		}

		#endregion
	}
}
