using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com.Cyril_WIRTZ.Loup_Garou;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class MinimapObjectID : Photon.PunBehaviour {

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
