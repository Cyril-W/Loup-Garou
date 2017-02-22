using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class RoleTutorial : MonoBehaviour {
		[Tooltip("The Sprites used to display the roles of the players")]
		public Text description;
		[Tooltip("The Sprites used to display the roles of the players")]
		public int currentSprite;

		List<Sprite> _roleSprites = new List<Sprite>();

		void Awake () {
			Object[] loadedSprites = Resources.LoadAll ("Cards", typeof(Sprite));
			foreach (Object obj in loadedSprites)
				_roleSprites.Add (obj as Sprite);

			currentSprite = _roleSprites.Count - 1;
			NexRole (true);
		}

		void Update () {
			transform.RotateAround (transform.position, Vector3.up, Time.deltaTime * 50f);
		}

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
					description.text += "The roles in \"Werewolf\" are pretty simple. Let's discover them one after another! Clic on the arrows to see the next or previous role.";
				else if (_roleSprites [currentSprite].name == "Villager")
					description.text += "The Villager's aim is to eliminate all Werewolves from the game. The only way to achieve that goal is to vote against a player during the day.";
				else if (_roleSprites [currentSprite].name == "Werewolf")
					description.text += "The Werewolf's aim is to eliminate all Villagers from the game. To achieve this, they can vote one more time against a player, at night.";
				else if (_roleSprites [currentSprite].name == "Seer")
					description.text += "The Seer's aim is to eliminate all Werewolves from the game. To achieve this, she can discover the role of someone each night.";
				else if (_roleSprites [currentSprite].name == "MayorDay")
					description.text += "The Mayor is elected on the first day. He represents the whole Village, and as such his vote counts for double.";
				else if (_roleSprites [currentSprite].name == "MayorNight")
					description.text += "When you're not the Mayor, be clever and stay kind with him, as when he will die, he will have to chose his successor.";
				else
					description.text += "This role has not been implemented yet...";
			}
		}
	}
}
