using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class DayNightCycle : Photon.PunBehaviour, IPunObservable {
		#region Public Variables


		[Tooltip("The distance center-Moon")]
		public float moonDistance = 600.0f;
		[Tooltip("The size of the Moon")]
		public float moonScale = 15.0f;

		[Tooltip("Enter a number of seconds to set the duration of a day")]
		public static float secondsInDay = 300.0f;
		[Tooltip("Enter a number of seconds to set the duration of a night")]
		public static float secondsInNight = 180.0f;
		[Range(0,1)]
		public static float currentTime = 0.0f;


		#endregion


		#region Private Variables


		private Light sun;
		private Transform moon;
		private ParticleSystem stars;
		private Image lunarClock;


		#endregion
	    

		#region MonoBehaviour CallBacks


	    // Use this for initialization
	    void Start()
	    {
	        currentTime = 0.0f;

	        sun = GetComponentInChildren<Light>();
			moon = transform.GetChild (1);
	        moon.transform.localPosition = new Vector3(0, 0, moonDistance);
	        moon.transform.localScale = new Vector3(moonScale, moonScale, moonScale);
			stars = GetComponentInChildren<ParticleSystem> ();
			lunarClock = GameObject.FindGameObjectWithTag ("Clock").GetComponent<Image>();
			lunarClock.fillAmount = 0;
	    }

	    // Update is called once per frame
	    void Update()
	    {
	        checkTime();

	        UpdateLights();

	        UpdateClock();

	        transform.localRotation = Quaternion.Euler((currentTime * 360.0f), 0, 0);
	    }


		#endregion


		#region Custom


		void OnGUI ()
		{
			if (PhotonNetwork.isMasterClient) {
				int BoxWidth = 140;
				int BoxHeight = 30;
				DayNightCycle.currentTime = GUI.HorizontalSlider (new Rect ((Screen.width - BoxWidth - 10), 5, BoxWidth, BoxHeight), DayNightCycle.currentTime, 0.0f, 1.0f);
			}
		}

		/// <summary>
		/// Depending on the time, the clock fills clockwise or anticlockwise.
		/// </summary>
	    void UpdateClock()
	    {
			lunarClock.fillOrigin = (int)Image.Origin360.Top;

	        if (currentTime <= 0.5)
	        {
				lunarClock.fillClockwise = true;
				lunarClock.fillAmount = currentTime * 2;

	        }
	        else
	        {
				lunarClock.fillClockwise = false;            
				lunarClock.fillAmount = (1 - currentTime) * 2;
	        }
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
	        sun.intensity = 1.0f * intensityMultiplier;

	        // No stars during the day, and lots of them at night !
			var em = stars.emission;
			int nbOfStars = Mathf.RoundToInt ((1 - intensityMultiplier) * 1000);
			em.enabled = nbOfStars == 0 ? false : true;
			if (em.enabled == true) {
				var main = stars.main;
				main.maxParticles = nbOfStars;
			}
	    }

		/// <summary>
		/// This allows to have different duration of a day and a night.
		/// </summary>
	    void checkTime()
	    {
	        if (currentTime < 0.5f)
	            currentTime += (Time.deltaTime / (2.0f * secondsInDay));
	        else if (currentTime < 1.0f)
	            currentTime += (Time.deltaTime / (2.0f * secondsInNight));
	        else
	            currentTime = 0.0f;        
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
