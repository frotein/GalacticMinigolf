using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeSlider : MonoBehaviour {

    public float min, max, startingVal;
    public Text text;
    Slider slider;
    // Use this for initialization
	void Start ()
    {
        slider = transform.GetComponent<Slider>();
        slider.value = Time.timeScale;
        slider.minValue = min;
        slider.maxValue = max;
        text.text = Time.timeScale.ToString();
        slider.value = startingVal;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ChangeTimeScale(float newVal)
    {
        if(Time.timeScale != newVal)
           Time.timeScale = newVal;

        text.text =  newVal.ToString(".0#");
    }
}
