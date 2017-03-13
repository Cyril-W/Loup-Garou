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

		public static bool isDebugging = false;
		[Range(0,1)]
		public static float currentTime = 0.0f;


		#endregion


		#region Private Variables


		Light _sun;
		Transform _moon;
		ParticleSystem _stars;


		#endregion
	    

		#region MonoBehaviour CallBacks


	    void Start()
	    {
	        currentTime = 0.0f;

	        _sun = GetComponentInChildren<Light>();
			_moon = transform.GetChild (1);
	        _moon.transform.localPosition = new Vector3(0, 0, moonDistance);
	        _moon.transform.localScale = new Vector3(moonScale, moonScale, moonScale);
			_stars = GetComponentInChildren<ParticleSystem> ();

			// This is the beginner tips panel
			GameObject.FindGameObjectWithTag ("Canvas").transform.GetChild (6).gameObject.SetActive(!isDebugging);

			pivotPoint.localRotation = Quaternion.Euler(0f, 0f, 360f * -currentTime);
		}

	    void Update()
	    {
			currentTime += (Time.deltaTime / ( 8.0f * secondsOfPhases[GetCurrentState() - 1] ));
			if (currentTime > 1f)
				currentTime = 0f;

	        UpdateLights();

	        UpdateClock();

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
