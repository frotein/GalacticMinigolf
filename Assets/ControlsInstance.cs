using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsInstance : MonoBehaviour {

    public RectTransform[] ignorePanels; // inside these panels clicks arent detected
    // Use this for initialization
	void Start ()
    {
        Controls.IgnoreAreas = ignorePanels;	
	}
	
	// Update is called once per frame
	void Update ()
    {
        
        Controls.GetWorldPosition();
	}

    void LateUpdate()
    {
        Controls.SetTouchCount();
    }
}
