using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObjects : MonoBehaviour
{
    public static Transform golfBall;
    
    public void ToggleAdjustmentEngines(bool on)
    {
        golfBall.GetComponent<RocketBooster>().enabled = on;
    }

    public void ToggleCameraGragging(bool on)
    {
        Camera.main.GetComponent<ClickAndDragCamera>().enabled = on;
    }

    public void TogglePulseEngine(bool on)
    {
        golfBall.GetComponent<ClickAndDragForce>().enabled = on;
    }

    public void ToggleKeepOnScreen(bool on)
    {
        Camera.main.GetComponent<KeepObjectOnScreen>().enabled = on;
    }
}
