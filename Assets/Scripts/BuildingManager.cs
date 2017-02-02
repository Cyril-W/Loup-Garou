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
	public class BuildingManager : MonoBehaviour {
		#region Public Variables


		[Tooltip("The owner of the building. When he's inside, other can also enter")]
		public bool hasOwner = false;
		[Tooltip("The owner of the building is displayed on this world canvas")]
		public Transform displayCanvas;
		[Tooltip("The image used to display when the owner is not inside")]
		public Sprite exitDoor;
		[Tooltip("The image used to display when the owner is inside")]
		public Sprite enterDoor;


		#endregion


		#region Private Variables


		bool _ownerInside = false;
		GameObject _owner;


		#endregion


		#region MonoBehaviour CallBacks


		void Update () {
			bool isOwner;
			bool newHasOwner = CheckOwner (out isOwner);
			if (newHasOwner != hasOwner) {
				hasOwner = newHasOwner;
				ChangeColor (hasOwner, isOwner);
			}

			if (isOwner && !_ownerInside) {
				if (DayNightCycle.currentTime > 0.5f && DayNightCycle.currentTime <= 1f)
					_owner.GetComponent<PlayerManager> ().isAlive = false;
			}
		}

		bool CheckOwner (out bool isOwner) {
			isOwner = false;
			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.tent != "" && pM.tent == gameObject.name) {
					if (player == PlayerManager.LocalPlayerInstance) {
						isOwner = true;
						_owner = player;
					}
					displayCanvas.GetComponentInChildren<Text> ().text = PlayerManager.GetProperName (pM.gameObject.name);
					return true;
				}
			}
			displayCanvas.GetComponentInChildren<Text> ().text = "No owner";
			return false;
		}

		void ChangeColor(bool hasOwner, bool isOwner) {
			Renderer fabrik1 = transform.GetChild (0).GetChild (0).GetComponent<Renderer> ();
			Renderer fabrik2 = transform.GetChild (0).GetChild (1).GetComponent<Renderer> ();
			MinimapObjectID mID = GetComponent<MinimapObjectID> ();
			Image i = displayCanvas.GetChild (1).GetComponent<Image> ();

			if (hasOwner) {
				if (isOwner) {
					fabrik1.material.color = Color.red;
					fabrik2.material.color = Color.red;
					mID.color = Color.red;
					i.sprite = enterDoor;
					i.color = Color.green;
				} else {
					fabrik1.material.color = Color.blue;
					fabrik2.material.color = Color.blue;
					mID.color = Color.blue;
					i.sprite = exitDoor;
					i.color = Color.red;
				}
			} else {
				fabrik1.material.color = Color.white;
				fabrik2.material.color = Color.white;
				mID.color = Color.white;
				i.sprite = enterDoor;
				i.color = Color.white;
			}

			Minimap.Instance.RecolorMinimapObject (gameObject);
		}

		void OnTriggerStay(Collider other) {
			/*
			 * This code served when tents weren't attributed right from the start!
			if (other.gameObject.CompareTag("Player")) {
				PlayerManager pM = other.gameObject.GetComponent<PlayerManager> ();
				if (!hasOwner && pM.tent == "") {
					hasOwner = true;
					pM.tent = gameObject.name;
					displayCanvas.GetComponentInChildren<Text> ().text = PlayerManager.GetProperName(pM.gameObject.name);
					ChangeColor (true, true);
				} else if (pM.tent != gameObject.name) {
					if (!_ownerInside) {
						other.gameObject.transform.position = transform.GetChild (5).position;
						other.gameObject.transform.rotation = transform.GetChild (5).rotation;
					}
				} else if (pM.tent == gameObject.name)
					_ownerInside = true;
			}
			*/

			if (hasOwner && other.CompareTag("Player")) {
				PlayerManager pM = other.GetComponent<PlayerManager> ();
				if (pM.tent != gameObject.name && !_ownerInside) {
					other.transform.position = transform.GetChild (5).position;
					other.transform.rotation = transform.GetChild (5).rotation;
				} else if (pM.tent == gameObject.name) {
					_ownerInside = true;
					Image i = displayCanvas.GetChild (1).GetComponent<Image> ();
					i.sprite = enterDoor;
					i.color = Color.green;
				}
			}
		}

		void OnTriggerExit(Collider other) {
			if (other.CompareTag ("Player") && other.GetComponent<PlayerManager> ().tent == gameObject.name) {
				_ownerInside = false;
				if (other.gameObject != PlayerManager.LocalPlayerInstance) {
					Image i = displayCanvas.GetChild (1).GetComponent<Image> ();
					i.sprite = exitDoor;
					i.color = Color.red;
				}
			}
		}


		#endregion
	}
}
