using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com.Cyril_WIRTZ.Loup_Garou;
using UnityEngine.SceneManagement;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Placed on an object your want to track on your minimap. The image shown on the minimap is customable.
	/// </summary>
	public class MinimapObjectID : MonoBehaviour {

		[Tooltip("Custom image used to show the position of the gameObject on the minimap")]
		public Sprite image;
		[Tooltip("Custom color used to show the position of the gameObject on the minimap")]
		public Color color;

		void Start () {
			if (gameObject.CompareTag ("Player") == false || gameObject != PlayerManager.LocalPlayerInstance)
				Minimap.Instance.RegisterMinimapObject (gameObject, image, color);
		}

	}
}
