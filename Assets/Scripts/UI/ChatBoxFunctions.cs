using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class ChatBoxFunctions : MonoBehaviour {

		Text showHideButtonText;
		GameObject messagesPanel;
		GameObject chatInputPanel;
		bool isChatShowing = true;

		public void Start() {
			showHideButtonText = transform.GetChild (0).GetComponentInChildren<Text> ();
			messagesPanel = transform.GetChild (1).gameObject;
			chatInputPanel = transform.GetChild (2).gameObject;
		}

		public void ToggleChat () {
			isChatShowing = !isChatShowing;
			messagesPanel.SetActive (isChatShowing);
			chatInputPanel.SetActive (isChatShowing);
			if (isChatShowing)
				showHideButtonText.text = "- Hide Chat -";
			else
				showHideButtonText.text = "- Show Chat -";
		}
	}
}
