using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	/// <summary>
	/// Day Night cycle. 
	/// This handles the shifting between day and night, the rotation of the sun and moon, the stars at night and the phases of the game.
	/// </summary>
	public class DayNightCycle : Photon.PunBehaviour, IPunObservable {
		
		#region Public Variables


		[Tooltip("The distance center-Moon")]
		public float moonDistance = 600.0f;
		[Tooltip("The size of the Moon")]
		public float moonScale = 15.0f;
		[Tooltip("The pivot of the clock")]
		public RectTransform pivotPoint;
		[Tooltip("The center of sun/moon timeline")]
		public RectTransform translPoint;
		/// <summary>
		/// This allows distinct duration of each phases of day or night.
		/// </summary>
		public float[] secondsOfPhases;

		public bool isDebugging = false;

		[Range(0,1)]
		public static float currentTime = 0.0f;
		public static DayNightCycle Instance;


		#endregion


		#region Private Variables


		float _secondsToNextPhase;
		Text[] _clockDescriptionTexts;
		Transform _hourglass;
		bool _isHourGlassUp = true;
		Light _sun;
		Transform _moon;
		ParticleSystem _stars;


		#endregion
	    

		#region MonoBehaviour CallBacks


	    void Awake()
	    {
			Instance = this;
	        currentTime = 0f;
			_secondsToNextPhase = secondsOfPhases [GetCurrentState () - 1];

			_clockDescriptionTexts = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (4).GetComponentsInChildren<Text> ();
			_clockDescriptionTexts [0].text = "Welcome! You woke up in this Village, you have time for it now... Meet the other villagers, and Chat with them!\n\nTo dismiss this panel, click the button below.";
			_clockDescriptionTexts[1].transform.parent.gameObject.SetActive (false);
			_hourglass = GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (4).GetChild (2);
			_sun = GetComponentInChildren<Light>();
			_moon = transform.GetChild (1);
	        _moon.transform.localPosition = new Vector3(0, 0, moonDistance);
	        _moon.transform.localScale = new Vector3(moonScale, moonScale, moonScale);
			_stars = GetComponentInChildren<ParticleSystem> ();

			pivotPoint.localRotation = Quaternion.Euler(0f, 0f, 360f * -currentTime);
		}

	    void Update()
	    {
			if (!RoleManager.gameFinished) {
				currentTime += Time.deltaTime / (8f * secondsOfPhases [GetCurrentState () - 1]);
				if (currentTime >= 1f)
					currentTime = 0f;
			}

			_secondsToNextPhase = (GetCurrentState () - 8f * currentTime) * secondsOfPhases[GetCurrentState () - 1];

	        UpdateLights();

	        UpdateClock();

			UpdateClockText (false);

	        transform.localRotation = Quaternion.Euler((currentTime * 360.0f), 0, 0);
	    }


		#endregion


		#region Custom


		void OnGUI ()
		{
			if (isDebugging && PhotonNetwork.isMasterClient) {
				int BoxWidth = 140;
				int BoxHeight = 30;
				DayNightCycle.currentTime = GUI.HorizontalSlider (new Rect ((Screen.width - BoxWidth - 10), 5, BoxWidth, BoxHeight), DayNightCycle.currentTime, 0.0f, 1.0f);
			}
		}

		/// <summary>
		/// Rotates the information clock and translates the sun/moon timeline
		/// </summary>
	    void UpdateClock()
	    {
			pivotPoint.localRotation = Quaternion.Euler(0f, 0f, 360f * -currentTime);
			translPoint.localPosition = new Vector3(-75f + 150f * currentTime * 2, -40f, 0f);

			_hourglass.GetChild(0).GetComponent<Image>().fillAmount = _secondsToNextPhase / secondsOfPhases [GetCurrentState () - 1];
			int numberOfMinutes = (int)(_secondsToNextPhase / 60f);
			int numberOfSecondsRemaining = (int)(_secondsToNextPhase - 60f * numberOfMinutes);
			_clockDescriptionTexts [2].text = (numberOfMinutes < 10) ? "0" : "";
			_clockDescriptionTexts [2].text += numberOfMinutes.ToString () + "\n\n";
			_clockDescriptionTexts [2].text += (numberOfSecondsRemaining < 10) ? "0" : "";
			_clockDescriptionTexts [2].text += numberOfSecondsRemaining.ToString();
	    }

		/// <summary>
		/// The intensity of light and the number of stars are calculated here.
		/// </summary>
	    void UpdateLights()
	    {        
	        float intensityMultiplier = 1.0f;

	        if (currentTime >= 0.52f)
	            intensityMultiplier = 0.0f;
	        else if (currentTime <= 0.02f)
	            intensityMultiplier = Mathf.Clamp01(currentTime * (1 / 0.02f));
	        else if (currentTime >= 0.5f)
	            intensityMultiplier = Mathf.Clamp01(1 - (currentTime - 0.5f) * (1 / 0.02f));

	        // This is to adjust sun intensity
	        _sun.intensity = 1.0f * intensityMultiplier;

	        // No stars during the day, and lots of them at night !
			var em = _stars.emission;
			int nbOfStars = Mathf.RoundToInt ((1 - intensityMultiplier) * 1000);
			em.enabled = nbOfStars == 0 ? false : true;
			if (em.enabled == true) {
				var main = _stars.main;
				main.maxParticles = nbOfStars;
			}
	    }

		/// <summary>
		/// This function indicates to all player what to do depending on the time of the day. It also (in)activate the resetVoteButton
		/// </summary>
		public void UpdateClockText (bool hasStateChanged) {
			PlayerManager localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();	
			Text smallText = _clockDescriptionTexts [1];
			int state = GetCurrentState ();

			if (hasStateChanged) {
				if(CheckIfPhaseIsPassed () == false)
					StartCoroutine ("RotateHourGlass");

				bool isBigTextActive = ShouldBigTextAppear ();
				Text bigText = _clockDescriptionTexts [0];
				bigText.transform.parent.gameObject.SetActive (isBigTextActive);
				smallText.transform.parent.gameObject.SetActive (!isBigTextActive);

				bigText.text = "";

				if (state == 1)
					bigText.text += "Wake up! You may explore the Village, you have time for it now... What happend during night?";
				else if (state == 2) {
					if (VoteManager.Instance.countDay == 1)
						bigText.text += "Time to discuss on the Chat and decide who to vote as the new Mayor by clicking on the button \"Vote\" in front of the player Tent!";
					else
						bigText.text += "Time to discuss on the Chat and decide who to eliminate by clicking on the button \"Vote\" in front of the player Tent!";
				} else if (state == 3)
					bigText.text += "Hurry ! It is your last chance to vote or change your vote by clicking on the button \"Vote\" located in front of the player Tent! If you are sure about your choice, patience.";
				else if (state == 4)
					bigText.text += "Votes have all been counted! Now finish your discussions and go back to your tent, night is falling upon the Village!";
				else if (state == 5)
					bigText.text += "The night has just fallen, keep your eyes closed and wait until dawn, or until asked to play a specific role if you have one!";
				else if (state == 6) {
					if (localPM.role == "Seer")
						bigText.text += "Come out, come out, wherever you are! Seer, time for you to come out and discover someone's role!";
					else
						bigText.text += "Hold on to your little secrets! The Seer may have already discovered your role, and if not: time for him to do so!";
				} else if (state == 7) {
					if (localPM.role == "Werewolf")
						bigText.text += "Time for you, as a Werewolf, to strike down your opponent! Go on the Chat and agree with other Werewolves on who to devour...";
					else
						bigText.text += "Time for the Werewolf to strike down their opponent! Beware: they are silently agreeing on the Chat on who to devour...";
				} else if (state == 8) {
					if (localPM.role == "Witch")
						bigText.text += "Votes have all been counted! The victim's fate is now in your own hands, Witch! Will you save the victim? Will you kill someone?";
					else
						bigText.text += "Votes have all been counted! The victim's fate is now in your own hands, Witch! Will you save the victim? Will you kill someone?";
				}

				bigText.text += "\n\nTo dismiss this panel, click the button below.";
			} else if (!localPM.isAlive)
				smallText.text = "You died... You may spy the other players or leave the game now...";
			else {
				smallText.text = "You have up to " + secondsOfPhases[state - 1] + "seconds to ";
				string mostVotedPlayer = PlayerManager.GetProperName (VoteManager.Instance.mostVotedPlayer);
				if (mostVotedPlayer == "")
					mostVotedPlayer = "Nobody";
				
				if (state == 1) {
					if (VoteManager.Instance.countDay == 1)
						smallText.text += "explore the village.";
					else
						smallText.text += "discover who died during the night.";
				} else if (state == 2) {					
					if (VoteManager.Instance.countDay == 1)
						smallText.text += "vote for the new Major. " + mostVotedPlayer + " has the most votes for now.";
					else
						smallText.text += "vote for the next victim. " + mostVotedPlayer + " has the most votes for now.";
				} else if (state == 3) {
					if (VoteManager.Instance.countDay == 1)
						smallText.text += "modify your vote for the new Mayor. " + mostVotedPlayer + " has the most votes for now.";
					else
						smallText.text += "modify your vote for the next victim. " + mostVotedPlayer + " has the most votes for now.";
				} else if (state == 4)
					smallText.text += "go back home before night.";
				else if (state == 5)
					smallText.text += "wait before the Seer may come out.";
				else if (state == 6) {
					if (localPM.role == "Seer")
						smallText.text += "discover the role of someone.";
					else
						smallText.text += "wait while the Seer discover a role.";
				} else if (state == 7) {
					if (localPM.role == "Werewolf")
						smallText.text += "vote against a player. " + mostVotedPlayer + " has the most votes for now.";
					else
						smallText.text += "wait while Werewolves vote against a player.";
				} else if (state == 8) {
					if (localPM.role == "Witch")
						smallText.text += "decide the fate of players. " + mostVotedPlayer + " has been designated.";
					else 
						smallText.text += "wait while the Witch decides what to do.";					
				}
			}
		}

		/// <summary>
		/// This function is called to know if the local player should be informed witch a big text or not.
		/// </summary>
		bool ShouldBigTextAppear() {
			if (VoteManager.Instance.countDay == 1 || VoteManager.Instance.countNight == 0)
				return true;
			else {
				int phase = GetCurrentState ();
				PlayerManager localPM = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager> ();
				if (!localPM.isAlive)
					return false;
				
				if (phase == 2 || phase == 4)
					return true;
				else if (phase == 6 && localPM.role == "Seer")
					return true;
				else if (phase == 7 && localPM.role == "Werewolf" || localPM.role == "Seer")
					return true;
				else if (phase == 8 && localPM.role == "Witch")
					return true;
				else
					return false;				
			}
		}

		/// <summary>
		/// This function is called to know if the Seer or the Witch phase can be skipped or not.
		/// </summary>
		bool CheckIfPhaseIsPassed() {
			if (!PhotonNetwork.isMasterClient)
				return false;
			
			if (GetCurrentState() != 6 && GetCurrentState() != 8)
				return false;			

			GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");		
			bool isRoleConcernedActive = false;
			foreach (GameObject player in players) {
				PlayerManager pM = player.GetComponent<PlayerManager> ();
				if (pM.isAlive) {
					if (GetCurrentState() == 6 && pM.role == "Seer")
						isRoleConcernedActive = true;
					else if (GetCurrentState() == 8 && (pM.deathPotionAvailable || pM.lifePotionAvailable))
						isRoleConcernedActive = true;
				}
			}

			if (!isRoleConcernedActive) {
				currentTime = GetCurrentState () / 8f;		
				return true;
			} else
				return false;
		}	

		IEnumerator RotateHourGlass() {
			_hourglass.rotation = (_isHourGlassUp) ? Quaternion.Euler(0f * Vector3.forward) : Quaternion.Euler(180f * Vector3.forward);
			_hourglass.localScale = Vector3.one;

			int angleSpeed = 5;
			int angleRotated = 0;
			while (angleRotated < 180) {
				_hourglass.Rotate (-angleSpeed * Vector3.forward);
				angleRotated += angleSpeed;
				_hourglass.localScale += 0.1f * Vector3.one;
				yield return null;
			}
			_hourglass.GetChild (0).GetComponent<Image> ().fillOrigin = (_isHourGlassUp) ? 1 : 0;
			_clockDescriptionTexts[2].transform.Rotate(180f * Vector3.forward) ;
			_isHourGlassUp = !_isHourGlassUp;
			while (angleRotated > 0) {
				angleRotated -= angleSpeed;
				_hourglass.localScale -= 0.1f * Vector3.one;
				yield return null;
			}
		}

		/// <summary>
		/// This static function is called to know in which phase of the game we are.
		/// </summary>
		public static int GetCurrentState () {			
			return Mathf.CeilToInt (Mathf.Clamp (currentTime * 8f, 1f, 8f));
		}


		#endregion


		#region IPunObservable implementation


		void IPunObservable.OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
		{
			if (stream.isWriting)
			{
				// send the others the data
				stream.SendNext(currentTime);
			}else{
				// If MasterClient change something, receive data
				DayNightCycle.currentTime = (float)stream.ReceiveNext();
			}
		}


		#endregion
	}
}
