using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour {

	public string playerPrefabName = "Player";
	public string roomName = "LoupGarou";

	const string version = "v0.0.1";

	void Start () {
		PhotonNetwork.ConnectUsingSettings (version);
	}

	void OnJoinedLobby () {
		RoomOptions roomOptions = new RoomOptions () { IsVisible = false, MaxPlayers = 6 };
		PhotonNetwork.JoinOrCreateRoom (roomName, roomOptions, TypedLobby.Default);
	}

	void OnJoinedRoom () {
		PhotonNetwork.Instantiate(playerPrefabName, new Vector3(Random.Range(-20, 20), 1, Random.Range(-20, 20)), Quaternion.identity, 0);
	}
}
