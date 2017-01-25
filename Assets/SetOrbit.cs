using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOrbit : MonoBehaviour {

    public Transform orbiting;
    public bool orbit;
    public float startingPower;
    public float mass;
    public float rotationSpeed;
    int index;
    Vector2[] positions;
    // Use this for initialization
	void Start ()
    {
        if (orbit)
        {
            OrbitPredictor.instance.SimulateUntilOrbit(transform.position.XY(), mass,
                                                           transform.GetComponent<CircleCollider2D>().radius,
                                                           transform.right.XY() * startingPower);
            positions = OrbitPredictor.instance.SimulationPositions(orbiting);
        }
        
        index = 0;
    }
	
	void FixedUpdate()
    {
        if (orbit)
        {
            transform.localPosition = positions[index];

            index += 1;
            index %= positions.Length;
        }
    }

    void Update()
    {
        transform.eulerAngles += new Vector3(0, 0, rotationSpeed * Time.deltaTime);
    }
}
