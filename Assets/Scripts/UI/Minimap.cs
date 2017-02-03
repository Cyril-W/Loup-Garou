using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Minimap Object. 
	/// Is used to link a dot on the minimap with its owner.
	/// </summary>
	public class MinimapObject {
		public Image Icon { get; set; }
		public GameObject Owner { get; set; }
	}

	/// <summary>
	/// Minimap. 
	/// Handles the coordinates of other players on your minimap, using trigonometric position.
	/// </summary>
	public class Minimap : MonoBehaviour {
		#region Public Variables


		[Tooltip("Prefab of the image used to show the position of the gameObject on the minimap")]
		public GameObject minimapDotPrefab;
		public float mapScale = 1.5f;

		public static Minimap Instance;


		#endregion


		#region Private Variables


		static List<MinimapObject> objects = new List<MinimapObject> ();


		#endregion


		#region MonoBehaviour CallBacks


		void Awake () {
			Instance = this;

			PlayerAnimatorManager.compas = transform.GetChild(0);
		}

		// Update is called once per frame
		void Update () {
			DrawMinimapDots ();
		}

		#endregion


		#region Custom


		/// <summary>
		/// Create the image on the minimap and register it on the list of minimap objects.
		/// </summary>
		public void RegisterMinimapObject (GameObject owner, Sprite sprite, Color color) {
			Image image = (Instantiate (minimapDotPrefab)).GetComponent<Image>();
			image.sprite = sprite;
			image.color = color;
			image.transform.SetParent (transform);

			objects.Add (new MinimapObject () { Owner = owner, Icon = image });
		}

		/// <summary>
		/// Remove the image from the minimap and from the list of minimap objects.
		/// </summary>
		public void RemoveMinimapObject (MinimapObject mO) {
			Destroy (mO.Icon);
			objects.Remove(mO);
		}

		/// <summary>
		/// Recolor the image in the minimap.
		/// </summary>
		public void RecolorMinimapObject (GameObject owner) {
			MinimapObject mO = objects.Find (o => o.Owner == owner);
			if(mO != null)
				mO.Icon.color = owner.GetComponent<MinimapObjectID>().color;
			else
				Debug.Log ("Error: the minimap object for " + owner.name + " has not been found!");
		}

		/// <summary>
		/// Calculate position of other players from your localPlayer and put the dot in the right place. If the player turn it simply pivot around the center of the minimap
		/// </summary>
		void DrawMinimapDots () {
			for (int i = 0; i < objects.Count; i++) {
				MinimapObject obj = objects [i];
				if (obj.Owner != null) {
					Transform playerPos = PlayerManager.LocalPlayerInstance.transform;
					Vector3 minimapPos = (obj.Owner.transform.position - playerPos.position);
					float distToObject = Vector3.Distance (playerPos.position, obj.Owner.transform.position) * mapScale;
					float deltaY = Mathf.Atan2 (minimapPos.x, minimapPos.z) * Mathf.Rad2Deg - 270 - playerPos.eulerAngles.y;
					minimapPos.x = distToObject * Mathf.Cos (deltaY * Mathf.Deg2Rad) * -1;
					minimapPos.z = distToObject * Mathf.Sin (deltaY * Mathf.Deg2Rad);

					obj.Icon.transform.position = new Vector3 (minimapPos.x, minimapPos.z, 0) + transform.position;
				} else
					Minimap.Instance.RemoveMinimapObject (obj);
			}
		}


		#endregion
	}
}
