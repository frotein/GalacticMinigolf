using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOrbit : MonoBehaviour {


    public float startingPower;
    public float mass;
    // Use this for initialization
	void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown("r"))
        {
            OrbitPredictor.instance.SimulateUntilOrbit(transform.position.XY(), mass,
                                                       transform.GetComponent<CircleCollider2D>().radius,
                                                       transform.right.XY() * startingPower);
        }
	}
}
