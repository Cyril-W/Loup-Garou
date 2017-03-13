using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Player name input field. Let the user input his name, that will appear above the player during the game.
	/// </summary>
	[RequireComponent(typeof(InputField))]
	public class PlayerNameInputField : MonoBehaviour {
		
		#region Private Variables

		/// <summary>
		/// Store the PlayerPref Key to avoid typos
		/// </summary>
		static string _playerNamePrefKey = "PlayerName";


		#endregion


		#region MonoBehaviour CallBacks


		void Start () {
			string defaultName = "";
			InputField _inputField = this.GetComponent<InputField>();
			if (_inputField!=null)
			{
				if (PlayerPrefs.HasKey(_playerNamePrefKey))
				{
					defaultName = PlayerPrefs.GetString(_playerNamePrefKey);

					if (!PhotonNetwork.connected)
						defaultName = PlayerManager.GetProperName (defaultName);
					
					_inputField.text = defaultName;
				}
			}

			PhotonNetwork.playerName =  defaultName;
		}


		#endregion


		#region Public Methods


		/// <summary>
		/// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
		/// </summary>
		/// <param name="value">The name of the Player</param>
		public void SetPlayerName(string value)
		{
			// #Important
			PhotonNetwork.playerName = value + " "; // force a trailing space string in case value is an empty string, else playerName would not be updated.

			PlayerPrefs.SetString(_playerNamePrefKey,value);
		}


		#endregion
	}
}
