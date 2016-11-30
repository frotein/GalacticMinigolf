using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitPredictor : MonoBehaviour {

    float mass;
    float drag;
    Vector2 position;
    Vector2 velocity;
    List<Vector3> allPositions;
    public List<Vector2> velocities;
    public List<Vector2> actualVels;
    public List<PointEffector2D> effectors;
    public LineRenderer lineRenderer;
    // Use this for initialization
	void Start ()
    {
        actualVels = new List<Vector2>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Simulate(Rigidbody2D body, Vector2 initialForce, int steps)
    {
        Vector2 vel = (initialForce / body.mass) * Time.fixedDeltaTime;
        Debug.Log("predicted" + vel.y);
        drag = body.drag;
        InitialValues(body.position, vel, body.mass);
        allPositions = new List<Vector3>();
        velocities = new List<Vector2>();
        velocities.Add(velocity);
        allPositions.Add(body.position);
        for (int i = 0; i < steps; i++)
        {
            PhysicsStep();
            allPositions.Add(new Vector3(position.x, position.y,0));
            velocities.Add(velocity);
        }

        Render();
    }

    void Render()
    {
        lineRenderer.SetVertexCount(allPositions.Count);
        lineRenderer.SetPositions(allPositions.ToArray());
    }
    void InitialValues(Vector2 pos, Vector2 vel, float mas)
    {
        mass = mas;
        position = pos;
        velocity = vel;
    }
    void PhysicsStep()
    {
        foreach (PointEffector2D effector in effectors)
        {
            Vector2 dir = new Vector2(effector.transform.position.x - position.x, effector.transform.position.y - position.y);
            float distSqr = dir.magnitude * effector.distanceScale;
            if (effector.forceMode == EffectorForceMode2D.InverseSquared)
                distSqr *= distSqr;
            float force =  effector.forceMagnitude / (distSqr);
            float vel = (force / mass);
            vel*= Time.fixedDeltaTime;
            
            velocity += -dir.normalized * vel;
           
        }
        velocity *= (1f - (Time.fixedDeltaTime * drag));
        position += velocity * Time.fixedDeltaTime;
    }
}
