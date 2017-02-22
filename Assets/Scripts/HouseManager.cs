using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Player tent. 
	/// Player who doesn't own the tent cannot enter.
	/// Name of the player is displayed on a sign
	/// </summary>
	public class HouseManager : MonoBehaviour {
		#region Public Variables


		[Tooltip("The owner of the building. When he's inside, other can also enter")]
		public bool hasOwner = false;
		[Tooltip("The owner of the building is displayed on this world canvas")]
		public Transform displayCanvas;
		[Tooltip("The images used to display when the owner is inside")]
		public Sprite[] doors = new Sprite[2];
		[Tooltip("The images used to display when the owner is the Mayor")]
		public Sprite[] isMayor = new Sprite[2];
		[Tooltip("False if the owner is has a role during night and has not played his role during the current night")]
		public bool alreadyPlayed = true;


		#endregion


		#region Private Variables


		Renderer[] _fabriks;
		Image[] _displayImages;
		bool _ownerInside = false;
		GameObject _owner;


		#endregion


		#region MonoBehaviour CallBacks


		void Start () {
			_fabriks = transform.GetChild (0).GetComponentsInChildren<Renderer> ();
			_displayImages = new Image[] { displayCanvas.GetChild(1).GetComponent<Image> (), displayCanvas.GetChild(2).GetComponent<Image> (), displayCanvas.GetChild(3).GetComponent<Image> () };
			if (_displayImages.Length != 3)
				Debug.Log ("Oops, seems there is something missing to display all the player's informations!");
		}

		void Update () {
			bool isOwner;
			bool newHasOwner = CheckOwner (out isOwner);
			if (newHasOwner != hasOwner) {
				hasOwner = newHasOwner;
				ChangeColor (hasOwner, isOwner);
			}

			if (hasOwner && _owner != null) {
				if (_owner.name == VoteManager.Instance.mayorName)
					_displayImages [0].sprite = isMayor [0];
				else
					_displayImages [0].sprite = isMayor [1];

				PlayerManager pM = _owner.GetComponent<PlayerManager> ();
				string displayedRole;
				if (!pM.isDiscovered)
					displayedRole = "Card";
				else
					displayedRole = pM.role;
				Sprite roleSprite = Resources.Load ("Cards/" + displayedRole, typeof(Sprite)) as Sprite;
				if (roleSprite != null)
					_displayImages [2].sprite = roleSprite;
				else
					Debug.Log ("No image was found for " + displayedRole);

				bool frontFabricActive = false;
				if (pM.isAlive) {
					if (0.5f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.625f) {
						frontFabricActive = true;
						if (isOwner) {
							if (!_ownerInside) {
								pM.isAlive = false;
							} else if (pM.role == "Seer" || pM.role == "Witch" || pM.role == "Werewolf")
								alreadyPlayed = false;
						}
					} else if (0.625f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.75f) {
						if (pM.role != "Seer" || alreadyPlayed == true)
							frontFabricActive = true;
					} else if (0.75f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.875f) {
						if (pM.role == "Seer" && !_ownerInside)
							pM.isAlive = false;
						else if (pM.role != "Werewolf" || alreadyPlayed == true)
							frontFabricActive = true;
					} else if (0.875f < DayNightCycle.currentTime && DayNightCycle.currentTime < 0.1f) {
						if (pM.role == "Werewolf" && !_ownerInside)
							pM.isAlive = false;
						else if (pM.role != "Witch" || alreadyPlayed == true)
							frontFabricActive = true;
					}
				}
				_fabriks[3].gameObject.SetActive(frontFabricActive);
			}
		}

		void OnTriggerStay(Collider other) {
			if (hasOwner && other.CompareTag("Player")) {
				PlayerManager pM = other.GetComponent<PlayerManager> ();
				if (pM.tent != gameObject.name && !_ownerInside) {
					other.transform.position = transform.GetChild (5).position;
					other.transform.rotation = transform.GetChild (5).rotation;
				} else if (pM.tent == gameObject.name) {
					_ownerInside = true;
					_displayImages [1].sprite = doors [0];
					_displayImages [1].color = Color.green;
				}
			}
		}

		void OnTriggerExit(Collider other) {
			if (other.CompareTag ("Player") && other.GetComponent<PlayerManager> ().tent == gameObject.name) {
				_ownerInside = false;
				if (other.gameObject != PlayerManager.LocalPlayerInstance) {
					_displayImages [1].sprite = doors [1];
					_displayImages [1].color = Color.red;
				}
			}
		}


		#endregion


		#region Custom


		bool CheckOwner (out bool isOwner) {
			isOwner = false;
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.tent != "" && pM.tent == gameObject.name) {
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
				if (isOwner) {
					foreach (Renderer rend in _fabriks)
						rend.material.color = Color.red;
					mID.color = Color.red;
					_displayImages [1].sprite = doors [0];
					_displayImages [1].color = Color.green;
				} else {
					foreach (Renderer rend in _fabriks)
						rend.material.color = Color.blue;
					mID.color = Color.blue;
					_displayImages [1].sprite = doors [1];
					_displayImages [1].color = Color.red;
				}
			} else {
				foreach (Renderer rend in _fabriks)
					rend.material.color = Color.white;
				mID.color = Color.white;
				_displayImages [1].sprite = doors [0];
				_displayImages [1].color = Color.white;
				_displayImages [0].sprite = isMayor [1];
				Sprite roleSprite = Resources.Load("Cards/Card", typeof(Sprite)) as Sprite;
				if (roleSprite != null)
					_displayImages [2].sprite = roleSprite;
				else
					Debug.Log ("No image was found for Card");
			}

			Minimap.Instance.RecolorMinimapObject (gameObject);
		}

		public void DiscoverRole() {
			PlayerManager pM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
			if (pM.role == "Seer") {
				_owner.GetComponent<PlayerManager> ().isDiscovered = true;

				GameObject[] houses = GameObject.FindGameObjectsWithTag ("House");
				foreach (GameObject house in houses) {
					if (house.name == pM.tent) {
						PlayerManager.LocalPlayerInstance.transform.position = house.transform.position;
						house.GetComponent<HouseManager> ().alreadyPlayed = true;
					}
				}
			}
		}


		#endregion
	}
}
