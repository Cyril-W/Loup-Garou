using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Role tutorial. 
	/// Updates the canvas where all the cards and roles are displayed.
	/// </summary>
	public class RoleTutorial : MonoBehaviour {

		#region Public Variables 


		[Tooltip("The Sprites used to display the roles of the players")]
		public Text description;
		public int currentSprite;


		#endregion


		#region Private Variables


		List<Sprite> _roleSprites = new List<Sprite>();


		#endregion


		#region Monobehaviour Calls


		void Awake () {
			Object[] loadedSprites = Resources.LoadAll ("Cards", typeof(Sprite));
			foreach (Object obj in loadedSprites) {
				if(obj.name != "Witch00" && obj.name != "Witch01" && obj.name != "Witch10" && obj.name != "Dead")
					_roleSprites.Add (obj as Sprite);
			}

			currentSprite = _roleSprites.Count - 1;
			NexRole (true);
		}

		void Update () {
			transform.RotateAround (transform.position, Vector3.up, Time.deltaTime * 50f);
		}


		#endregion


		#region Custom


		public void NexRole (bool isNext) {
			if (isNext) {
				if (currentSprite == _roleSprites.Count - 1)
					currentSprite = -1;
				currentSprite++;
			} else {
				if (currentSprite == 0)
					currentSprite = _roleSprites.Count;
				currentSprite--;
			}

			if (_roleSprites == null)
				Debug.Log ("No cards were found in the Cards folder");
			else {
				GetComponent<Image> ().sprite = _roleSprites [currentSprite];
				description.text = "[" + (currentSprite + 1) + "/" + _roleSprites.Count + "]\n";
				if (_roleSprites [currentSprite].name == "Card")
					description.text += "The roles in \"Werewolf\" are pretty simple. Let's discover them one after another! Click on the arrows to see the next or previous role.";
				else if (_roleSprites [currentSprite].name == "Villager")
					description.text += "The Villager's aim is to eliminate all Werewolves from the game. The only way to achieve that goal is to vote against a player during the day.";
				else if (_roleSprites [currentSprite].name == "Werewolf")
					description.text += "The Werewolf's aim is to eliminate all Villagers from the game. To achieve this, he can vote against a player each night.";
				else if (_roleSprites [currentSprite].name == "Seer")
					description.text += "The Seer's aim is to eliminate all Werewolves from the game. To achieve this, she can discover the role of someone each night.";
				else if (_roleSprites [currentSprite].name == "Hunter")
					description.text += "The Hunter's aim is to eliminate all Werewolves from the game. To achieve this, he can shoot dead someone when he gets killed.";
				else if (_roleSprites [currentSprite].name == "MayorDay")
					description.text += "The Mayor is elected on the first day. He represents the whole Village, and as such his vote counts for double.";
				else if (_roleSprites [currentSprite].name == "MayorNight")
					description.text += "When you're not the Mayor, be clever and stay kind with him, as when he will die, he will have to chose his successor.";
				else if (_roleSprites [currentSprite].name == "Witch")
					description.text += "The Witch's aim is to eliminate all Werewolves from the game. To achieve this, she can kill and/or revive the Werewolves' victim once per game.";
				else if (_roleSprites [currentSprite].name == "LittleGirl")
					description.text += "The Little Girl's aim is to eliminate all Werewolves from the game. To achieve this, she can spy them at night, but if so, she is revealed to them!";
				else
					description.text += "This role has not been implemented yet...";
			}
		}


		#endregion
	}
}
