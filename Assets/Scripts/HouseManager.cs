using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Player House manager. 
	/// Player who doesn't own the house cannot enter.
	/// Name, role and vote button for the player is displayed on a panel.
	/// </summary>
	public class HouseManager : MonoBehaviour {
		
		#region Public Variables


		public bool hasOwner = false;
		[Tooltip("The information about the owner of the building is displayed on this world canvas")]
		public Transform displayCanvas;
		[Tooltip("The images used to display when the owner is inside")]
		public Sprite[] doors = new Sprite[2];
		[Tooltip("The images used to display when the owner is the Mayor")]
		public Sprite[] isMayor = new Sprite[2];

		public bool ownerInside = false;


		#endregion


		#region Private Variables


		/// <summary>
		/// The fabriks are color changing, depending on the owner of the tent. 
		/// </summary>
		Renderer[] _fabriks;
		Image _mayorImage;
		Image _ownerInsideImage;
		Image _roleImage;
		GameObject _mostVotedImage;
		GameObject _deadImage;
		/// <summary>
		/// The actions possible are: 0/vote, 1/save, 2/kill, 3/reveal. 
		/// </summary>
		GameObject[] _actionButtons;

		GameObject _owner;
		GameObject _mainCamera;
		GameObject _houseCamera;


		#endregion


		#region MonoBehaviour CallBacks


		void Start () {
			_fabriks = transform.GetChild (0).GetComponentsInChildren<Renderer> ();

			_mayorImage = displayCanvas.GetChild (2).GetComponent<Image> ();
			_ownerInsideImage = displayCanvas.GetChild (3).GetComponent<Image> ();
			_roleImage = displayCanvas.GetChild (4).GetComponent<Image> ();
			_actionButtons = new GameObject[] { displayCanvas.GetChild (5).gameObject, displayCanvas.GetChild (6).gameObject, displayCanvas.GetChild (7).gameObject, displayCanvas.GetChild (8).gameObject };
			_mostVotedImage = displayCanvas.GetChild (9).gameObject;
			_deadImage = displayCanvas.GetChild (10).gameObject;

			_mainCamera = Camera.main.gameObject;
			_houseCamera = transform.GetChild (5).gameObject;
		}

		void Update () {
			bool isOwner;
			bool newHasOwner = CheckOwner (out isOwner);
			if (newHasOwner != hasOwner) {
				hasOwner = newHasOwner;
				ChangeColor (hasOwner, isOwner);
			}

			if (hasOwner && _owner != null) {
				_fabriks [3].gameObject.SetActive (FrontFabrikActive ());
				UpdateOwnerInfo ();
			} else {
				_fabriks [3].gameObject.SetActive (false);
				_deadImage.SetActive (false);
			}
		}

		void OnTriggerStay(Collider other) {
			if (other.CompareTag("Player")) {
				if (hasOwner) {
					PlayerManager pM = other.GetComponent<PlayerManager> ();
					// The player is not authorised to go inside if the owner is not already inside
					if (pM.house != gameObject && !ownerInside)
						other.transform.position = transform.TransformPoint(new Vector3(-2f, 2, 7));
					else {
						// To avoid the player from looking outside at night, the camera is switched to a local mode
						if (other.gameObject == PlayerManager.LocalPlayerInstance) {
							_mainCamera.SetActive (false);
							_houseCamera.SetActive (true);
						}
						if (pM.house == gameObject) {
							ownerInside = true;
							_ownerInsideImage.sprite = doors [0];
							_ownerInsideImage.color = Color.green;
						}
					}
				}
			}
		}

		void OnTriggerExit(Collider other) {
			if (other.CompareTag ("Player")) {
				// To avoid the player from looking outside at night, the camera is switched to a local mode
				if (other.gameObject == PlayerManager.LocalPlayerInstance) {
					_mainCamera.SetActive (true);
					_houseCamera.SetActive (false);
				}

				if (other.GetComponent<PlayerManager> ().house == gameObject) {
					ownerInside = false;
					if (other.gameObject != PlayerManager.LocalPlayerInstance) {
						_ownerInsideImage.sprite = doors [1];
						_ownerInsideImage.color = Color.red;
					}
				}
			}
		}


		#endregion


		#region Custom


		/// <summary>
		/// This function makes a double check: if the house has a owner and if the local player is the owner. 
		/// </summary>
		bool CheckOwner (out bool isOwner) {
			isOwner = false;
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.house != null && pM.house == gameObject) {
					_owner = player;
					if (player == PlayerManager.LocalPlayerInstance)
						isOwner = true;					
					displayCanvas.GetComponentInChildren<Text> ().text = PlayerManager.GetProperName (_owner.name);
					return true;
				}
			}
			displayCanvas.GetComponentInChildren<Text> ().text = "No Owner";
			return false;
		}

		void ChangeColor(bool hasOwner, bool isOwner) {
			MinimapObjectID mID = GetComponent<MinimapObjectID> ();

			if (hasOwner) {
				_mayorImage.gameObject.SetActive (true);
				_roleImage.gameObject.SetActive (true);
				if (isOwner) {
					foreach (Renderer rend in _fabriks)
						rend.material.color = Color.red;
					mID.color = Color.red;
					_ownerInsideImage.sprite = doors [0];
					_ownerInsideImage.color = Color.green;
				} else {
					foreach (Renderer rend in _fabriks)
						rend.material.color = Color.blue;
					mID.color = Color.blue;
					_ownerInsideImage.sprite = doors [1];
					_ownerInsideImage.color = Color.red;
				}
			} else {
				_mayorImage.gameObject.SetActive (false);
				_roleImage.gameObject.SetActive (false);
				foreach (Renderer rend in _fabriks)
					rend.material.color = Color.white;
				mID.color = Color.white;
				_ownerInsideImage.sprite = doors [0];
				_ownerInsideImage.color = Color.white;
			}

			Minimap.Instance.RecolorMinimapObject (gameObject);
		}

		/// <summary>
		/// The front fabrik prevents player from going out at night, and conceals the player that plays a role during night. 
		/// </summary>
		bool FrontFabrikActive () {
			PlayerManager pM = _owner.GetComponent<PlayerManager> ();
			if (!pM.isAlive)
				return false;

			bool frontFabricActive = false;
			if (DayNightCycle.GetCurrentState () == 5) {
				if (!ownerInside && _owner == PlayerManager.LocalPlayerInstance)
					pM.isAlive = false;
				else
					frontFabricActive = true;
			} else if (DayNightCycle.GetCurrentState () == 6 && pM.role != "Seer")
				frontFabricActive = true;
			else if (DayNightCycle.GetCurrentState () == 7 && pM.role != "Werewolf")
				frontFabricActive = true;
			else if (DayNightCycle.GetCurrentState () == 8 && pM.role != "Witch")
				frontFabricActive = true;
				
			return frontFabricActive;
		}

		/// <summary>
		/// All the buttons and images on the front panel are udpated here. 
		/// </summary>
		void UpdateOwnerInfo () {
			PlayerManager ownerPM = _owner.GetComponent<PlayerManager> ();
			string displayedRole = "";
			if (ownerPM.isDiscovered)
				displayedRole = ownerPM.role;
			else
				displayedRole = "Card";
			Sprite roleSprite = Resources.Load("Cards/" + displayedRole, typeof(Sprite)) as Sprite;
			if (roleSprite != null)
				_roleImage.sprite = roleSprite;
			else
				Debug.Log ("No image was found for " + roleSprite.name);

			foreach (GameObject btn in _actionButtons)
				btn.SetActive (false);
		
			if (ownerPM.isAlive) {	
				_deadImage.SetActive (false);
				_mayorImage.gameObject.SetActive (true);
				if (VoteManager.Instance != null && _owner.name == VoteManager.Instance.mayorName)
					_mayorImage.sprite = isMayor [0];
				else
					_mayorImage.sprite = isMayor [1];

				PlayerManager localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
				if (localPM.isAlive) {
					if (DayNightCycle.GetCurrentState () == 2 || DayNightCycle.GetCurrentState () == 3 || (DayNightCycle.GetCurrentState () == 7 && localPM.role == "Werewolf")) {
						if (_owner.name != localPM.votedPlayer)
							_actionButtons [0].SetActive (true);
					} 
					if (DayNightCycle.GetCurrentState () == 6 && localPM.seerRevealingAvailable && ownerPM.isDiscovered == false)
						_actionButtons [3].SetActive (true);
				
					if (_owner.name == VoteManager.Instance.mostVotedPlayer) {
						_mostVotedImage.SetActive (true);
						if (DayNightCycle.GetCurrentState () == 8 && localPM.lifePotionAvailable)
							_actionButtons [1].SetActive (true);					
					} else {
						_mostVotedImage.SetActive (false);	
						if (DayNightCycle.GetCurrentState () == 8 && localPM.deathPotionAvailable)
							_actionButtons [2].SetActive (true);
					}
				}
			} else {
				_deadImage.SetActive (true);
				_mayorImage.gameObject.SetActive (false);
				_mostVotedImage.SetActive (false);
			}
		}

		/// <summary>
		/// This event is triggered when the player clicks the "Vote" button on the front panel. 
		/// </summary>
		public void VoteForPlayer() {
			PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().votedPlayer = _owner.name;
		}

		/// <summary>
		/// This event is triggered when the Witch clicks the "Save" button on the front panel, only if the player is the most voted. 
		/// </summary>
		public void SavePlayer() {
			PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().lifePotionAvailable = false;

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) 
				player.GetComponent<PhotonView> ().RPC ("ResetPlayerVote", PhotonTargets.All, new object[] { });
		}

		/// <summary>
		/// This event is triggered when the Witch clicks the "Kill" button on the front panel. 
		/// </summary>
		public void KillPlayer() {
			PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().deathPotionAvailable = false;

			_owner.GetComponent<PhotonView>().RPC ("KillPlayer", PhotonTargets.All, new object[] { });
		}

		/// <summary>
		/// This event is triggered when the Seer clicks the "Reveal" button on the front panel, only for one player. 
		/// </summary>
		public void RevealPlayer() {
			PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ().seerRevealingAvailable = false;

			_owner.GetComponent<PlayerManager> ().isDiscovered = true;
		}


		#endregion
	}
}
