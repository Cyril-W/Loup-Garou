using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
