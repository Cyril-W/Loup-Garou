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
		public Image icon { get; set; }
		public GameObject owner { get; set; }
	}

	/// <summary>
	/// Minimap. 
	/// Handles the coordinates of other players on your minimap, using trigonometric position.
	/// </summary>
	public class Minimap : MonoBehaviour {

		#region Public Variables


		public static Transform playerPos;
		public float mapScale = 2.0f;

		static List<MinimapObject> objects = new List<MinimapObject> ();


		#endregion


		#region MonoBehaviour CallBacks


		// Update is called once per frame
		void Update () {
			DrawMinimapDots ();
		}

		#endregion


		#region Custom


		/// <summary>
		/// Create the image one the minimapr and register it on the list of minimap objects.
		/// </summary>
		public static void RegisterMinimapObject (GameObject o, Image i) {
			Image image = Instantiate (i);
			objects.Add (new MinimapObject () { owner = o, icon = image });
		}

		/// <summary>
		/// Remove the image from the minimap and the list of minimap objects.
		/// </summary>
		public static void RemoveMinimapObject (GameObject o) {
			for (int i = 0; i < objects.Count; i++) {
				if (objects [i].owner == o) {
					Destroy (objects [i].icon);
					objects.RemoveAt(i);
					return;
				}
			}
		}

		/// <summary>
		/// Remove all the images from the minimap to start a new minimap.
		/// </summary>
		public static void FlushMap () {
			for (int i = 0; i < objects.Count; i++) {
				Destroy (objects [i].icon);
				objects.RemoveAt(i);
			}
		}

		/// <summary>
		/// Calculate position of other players from your localPlayer and put the dot in the right place. If the player turn it simply pivot around the center of the minimap
		/// </summary>
		void DrawMinimapDots () {
			foreach (MinimapObject obj in objects) {
				Vector3 minimapPos = (obj.owner.transform.position - playerPos.position);
				float distToObject = Vector3.Distance (playerPos.position, obj.owner.transform.position) * mapScale;
				float deltaY = Mathf.Atan2 (minimapPos.x, minimapPos.z) * Mathf.Rad2Deg - 270 - playerPos.eulerAngles.y;
				minimapPos.x = distToObject * Mathf.Cos (deltaY * Mathf.Deg2Rad) * -1;
				minimapPos.z = distToObject * Mathf.Sin (deltaY * Mathf.Deg2Rad);

				obj.icon.transform.SetParent (transform);
				obj.icon.transform.position = new Vector3 (minimapPos.x, minimapPos.z, 0) + transform.position;
			}
		}


		#endregion
	}
}
