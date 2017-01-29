using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class GameManager : Photon.PunBehaviour {

		#region Public Variables


		[Tooltip("The prefab to use for representing the player")]
		public GameObject playerPrefab;


		#endregion

		#region Photon Messages


		/// <summary>
		/// Called when the local player left the room. We need to load the launcher scene.
		/// </summary>
		public override void OnLeftRoom()
		{
			SceneManager.LoadScene(0);
		}

		public override void OnPhotonPlayerConnected( PhotonPlayer other  )
		{
			//if(SceneManagerHelper.ActiveSceneBuildIndex == 2)
			//	VoteManager.RegisterPlayerForVote (other.NickName);
		}


		public override void OnPhotonPlayerDisconnected( PhotonPlayer other  )
		{
			if(SceneManagerHelper.ActiveSceneBuildIndex == 2)
				VoteManager.RemovePlayerForVote (other.NickName);
		}


		#endregion


		#region Public Methods


		public void Start()
		{
			if (playerPrefab == null)
				Debug.LogError("Missing playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
			else {
				if (PlayerManager.LocalPlayerInstance == null) //spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
					PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f,3f,0f), Quaternion.identity, 0);
			}

			ChatManager.RoomName = PhotonNetwork.room.Name;
		}

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}


		#endregion  

		#region Private Methods


		void OnGUI ()
		{
			if (SceneManagerHelper.ActiveSceneBuildIndex == 2 && PhotonNetwork.isMasterClient) {
				int BoxWidth = 100;
				int BoxHeight = 30;
				DayNightCycle.currentTime = GUI.HorizontalSlider (new Rect ((Screen.width - BoxWidth - 2), 90, BoxWidth, BoxHeight), DayNightCycle.currentTime, 0.0f, 1.0f);
			}
		}


		#endregion
	}
}
