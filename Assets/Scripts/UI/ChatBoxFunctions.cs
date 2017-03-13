using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Chat box functions. 
	/// Contains all the functions essential to the chat box.
	/// </summary>
	public class ChatBoxFunctions : MonoBehaviour {

		#region Private Variables


		Text _showHideButtonText;
		GameObject _messagesPanel;
		GameObject _chatInputPanel;
		bool _isChatShowing = true;


		#endregion


		#region MonoBehaviour Calls


		public void Start() {
			_showHideButtonText = transform.GetChild (0).GetComponentInChildren<Text> ();
			_messagesPanel = transform.GetChild (1).gameObject;
			_chatInputPanel = transform.GetChild (2).gameObject;
		}


		#endregion


		#region Custom


		public void ToggleChat () {
			_isChatShowing = !_isChatShowing;
			_messagesPanel.SetActive (_isChatShowing);
			_chatInputPanel.SetActive (_isChatShowing);
			if (_isChatShowing)
				_showHideButtonText.text = "- Hide Chat -";
			else
				_showHideButtonText.text = "- Show Chat -";
		}


		#endregion
	}
}
