using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com.Cyril_WIRTZ.Loup_Garou;

public class MinimapObjectID : MonoBehaviour {

	public Image image;

	// Use this for initialization
	void Start () {
		Minimap.RegisterMinimapObject (gameObject, image);
	}

	void OnDestroy () {
		Minimap.RemoveMinimapObject (gameObject);
	}
}
