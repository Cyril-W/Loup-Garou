using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Com.Cyril_WIRTZ.Loup_Garou
{
	public class DayNightCycle : MonoBehaviour {

	    public float moonDistance = 600.0f;
	    public float moonScale = 15.0f;

	    public float secondsInDay = 60.0f;
	    public float secondsInNight = 30.0f;
	    [Range(0,1)]
	    public float currentTime = 0.0f;

		private Transform moon;
		private ParticleSystem stars;
	    private Image lunarClock;
	    private Light sun;

	    // Use this for initialization
	    void Start()
	    {
	        currentTime = 0.0f;

	        sun = GetComponent<Light>();
			moon = transform.GetChild (0);
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

	    void checkTime()
	    {
	        if (currentTime < 0.5f)
	            currentTime += (Time.deltaTime / (2.0f * secondsInDay));
	        else if (currentTime < 1.0f)
	            currentTime += (Time.deltaTime / (2.0f * secondsInNight));
	        else
	            currentTime = 0.0f;        
	    }

	    void OnGUI ()
	    {
	        int BoxWidth = 100;
	        int BoxHeight = 30;
	        currentTime = GUI.HorizontalSlider(new Rect((Screen.width - BoxWidth - 2), 90, BoxWidth, BoxHeight), currentTime, 0.0f, 1.0f);
	    }
	}
}
