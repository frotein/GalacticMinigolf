using UnityEngine;
using System.Collections;

public class OrbitTracker : MonoBehaviour {

    Transform pointer;
    public bool closest;
    public float totalOrbit;
    float lastTurnAngle;
    public Transform ball;
    public bool orbited;
    // Use this for initialization
	void Start ()
    {
        pointer = transform.GetChild(0);
        pointer.right = (Vector2)ball.position - (Vector2)transform.position;
        lastTurnAngle = pointer.eulerAngles.z;
        orbited = false;
    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        pointer.right = (Vector2)ball.position - (Vector2)transform.position;
        if (closest)
        {
            float ang = pointer.eulerAngles.z - lastTurnAngle;
            if(Mathf.Abs(ang) < 90)
             totalOrbit += ang;
        }

        orbited = OrbitPercentage() > 500;

    }

    void LateUpdate()
    {
        //if (closest)
            lastTurnAngle = pointer.eulerAngles.z;
    }

    public float OrbitPercentage()
    {
        return Mathf.Abs(totalOrbit);
    }
}
