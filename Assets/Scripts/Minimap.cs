using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class MinimapObject {
		public Image icon { get; set; }
		public GameObject owner { get; set; }
	}

	public class Minimap : MonoBehaviour {

		public static Transform playerPos;
		public float mapScale = 2.0f;

		public static List<MinimapObject> objects = new List<MinimapObject> ();

		public static void RegisterMinimapObject (GameObject o, Image i) {
			Image image = Instantiate (i);
			objects.Add (new MinimapObject () { owner = o, icon = image });
		}

		public static void RemoveMinimapObject (GameObject o) {
			List<MinimapObject> newList = new List<MinimapObject> ();
			for (int i = 0; i < objects.Count; i++) {
				if (objects [i].owner == o) {
					Destroy (objects [i].icon);
					continue;
				} else
					newList.Add (objects [i]);
			}

			objects.Clear (); //RemoveRange (0, objects.Count);
			objects.AddRange (newList);
		}

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
		
		// Update is called once per frame
		void Update () {
			DrawMinimapDots ();
		}
	}
}
