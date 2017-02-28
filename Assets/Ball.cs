using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {


    public Vector2 velocity;
    public float radius;
    public float mass;
    public float drag;
    public Vector2 simulatedPosition;
    public bool simulate;
	// Use this for initialization
	void Start ()
    {
        simulatedPosition = transform.position;	
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if(simulate)
        {
            OrbitPredictor.instance.PhysicsStepForConsistentLine(this);
            OrbitPredictor.instance.RenderConsistentLine();
        }

	}
}
