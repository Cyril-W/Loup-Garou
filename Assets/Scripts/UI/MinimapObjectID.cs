using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com.Cyril_WIRTZ.Loup_Garou;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Placed on an object your want to track the position on your minimap. The image shown on the minimap is customable.
	/// </summary>
	public class MinimapObjectID : Photon.PunBehaviour {

		[Tooltip("Custom image used to show the position of the gameObject on the minimap")]
		public Image image;

		bool isMyPlayer;

		// Use this for initialization
		void Start () {
			isMyPlayer = photonView.isMine;

			if (!isMyPlayer)
				Minimap.RegisterMinimapObject (gameObject, image);
		}

		void OnDestroy () {
			if (!isMyPlayer)
				Minimap.RemoveMinimapObject (gameObject);
		}
	}
}
