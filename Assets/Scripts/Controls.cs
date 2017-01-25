using UnityEngine;
using System.Collections;


// a static class that unifies controls for both PC and mobile
public static class Controls
{
    static int touchCount;
    static Vector2 worldPosition;
    static public RectTransform[] IgnoreAreas;
    static Vector2[] touchBegan; // a list of all touch positions when they began in order they were pressed down 
    public static bool Clicked()
    {

        // if its playihng on a mobile device, use touchEvents, otherwise use mouse
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            return (touchCount == 0 && Input.touchCount == 1);
        else
            return Input.GetMouseButtonDown(0);



    }

    public static bool InsideIgnoreArea()
    {
        Vector2 screenPos = ScreenPosition();
        foreach (RectTransform t in IgnoreAreas)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(t, screenPos)) return true;
        }

        return false;
    }
    public static Vector2 WorldPosition() { return worldPosition; }
    public static bool Held()
    {
      //  Debug.Log(InsideIgnoreArea());
        if (!InsideIgnoreArea())
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                return Input.touchCount == 1;
            else
                return Input.GetMouseButton(0);
        }

        return false;
    }
    public static Vector2 ScreenPosition()
    {
        Vector2 screenPosition = Vector2.zero;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                screenPosition = Input.touches[0].position;
            }
        }
        else
            screenPosition = Input.mousePosition;

        return screenPosition;
    }
    public static Vector2 ClickedPosition() // must make sure clicked is true if on mobile
    {
        
        return worldPosition;
    }

    public static float Zoom()
    {
        float zoomAmount = 0;
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount == 2)
            {
                // Store both touches.
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                // Find the position in the previous frame of each touch.
                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                // Find the magnitude of the vector (the distance) between the touches in each frame.
                float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                // Find the difference in the distances between each frame.
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;


                zoomAmount = deltaMagnitudeDiff;
            }
        }
        else
        {
            zoomAmount = -Input.GetAxis("Mouse ScrollWheel") * 5;
        }

        return zoomAmount;
    }
    public static bool Released()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            return (Input.touchCount == 0) && touchCount == 1;
        else
            return Input.GetMouseButtonUp(0);


    }

    public static void SetTouchCount() // must be called at the end of the frame
    {
        touchCount = Input.touchCount;
    }

    public static void GetWorldPosition()
    {
        Vector2 screenPosition = Vector2.zero;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                screenPosition = Input.touches[0].position;
            }
        }
        else
            screenPosition = Input.mousePosition;

        worldPosition = (Vector2)Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));
    }

   

}
