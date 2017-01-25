using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


// camera movement for when the ball is not moving, can drag the camera as long as the ball stays on screen
public class ClickAndDragCamera : MonoBehaviour
{
    public ClickAndDragForce ballController;
    public Transform minArrow, maxArrow;
    Vector2 startPos, cameraStart;
    bool dragging = false;
    public KeepObjectOnScreen otherCameraControl;
    public Transform arrow;
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Controls.Clicked() && !ballController.grabbing && !EventSystem.current.IsPointerOverGameObject())
        {
            startPos = Controls.ScreenPosition();
            cameraStart = transform.position.XY();
            dragging = true;
           // otherCameraControl.enabled = false;
        }
        Camera.main.orthographicSize += Controls.Zoom();
        if(!ballController.gameObject.GetComponent<Rigidbody2D>().isKinematic)
            dragging = false;
        
        if(dragging)
        {
            Vector2 diff = Camera.main.ScreenToWorldPoint(startPos).XY() - Controls.ClickedPosition();
            transform.position = (cameraStart + diff).XYZ(transform.position.z);

            if (Controls.Released()) dragging = false;
        }

        if(!ballController.GetComponent<Renderer>().isVisible)
        {
            arrow.gameObject.SetActive(true);
            arrow.position = new Vector2(Mathf.Max(minArrow.position.x, Mathf.Min(maxArrow.position.x,ballController.transform.position.x)),
                                          Mathf.Max(minArrow.position.y, Mathf.Min(maxArrow.position.y, ballController.transform.position.y)));

            arrow.transform.right = -(transform.position.XY() - ballController.transform.position.XY()).normalized;
        }
        else
            arrow.gameObject.SetActive(false);

    }

   
}
