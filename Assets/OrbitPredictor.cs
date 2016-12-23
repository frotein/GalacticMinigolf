using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitPredictor : MonoBehaviour {

    float mass;
    float drag;
    float radius;
    Vector2 position;
    Vector2 velocity;
    List<Vector3> allPositions;
    public List<Vector2> velocities;
    public List<Vector2> actualVels;
    public List<PointEffector2D> effectors;
    public LineRenderer lineRenderer;

    // how much the initial velocity in the predictor needs to increase for the prediction to still work
    public float onGroundIncrease;

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
     
        drag = body.GetComponent<ClickAndDragForce>().storedDrag;
        Vector2 vel = (initialForce / body.mass) * Time.fixedDeltaTime;
        // drag = .1f;// body.drag;
        vel = vel * (1 - Time.fixedDeltaTime * drag);
        radius = body.GetComponent<CircleCollider2D>().radius;
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
        /*List<Vector3> eachOther = new List<Vector3>();
        for(int i = 0; i < allPositions.Count; i++)
        {
            if(i % 2 == 0)
            {
                eachOther.Add(allPositions[i]);
            }
        }*/
        lineRenderer. numPositions = allPositions.Count;
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
        Collider2D[] cols2 = Physics2D.OverlapCircleAll(position, radius);
        foreach(Collider2D col in cols2)
        {
            if(col.usedByEffector)
            {
                PointEffector2D effector = col.GetComponent<PointEffector2D>();
                Vector2 dir = new Vector2(effector.transform.position.x - position.x, effector.transform.position.y - position.y);
                float distSqr = dir.magnitude * effector.distanceScale;
                if (effector.forceMode == EffectorForceMode2D.InverseSquared)
                    distSqr *= distSqr;
                float force = effector.forceMagnitude / (distSqr);
                float vel = (force / mass);
                vel *= Time.fixedDeltaTime;

                velocity += -dir.normalized * vel;
            }
        }
        if(drag != 0)
           velocity *= (1f - (Time.fixedDeltaTime * drag));

        position += velocity * Time.fixedDeltaTime;
    }
}
