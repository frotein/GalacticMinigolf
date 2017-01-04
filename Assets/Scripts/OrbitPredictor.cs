using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitPredictor : MonoBehaviour {

    float mass;
    public float drag;
    float radius;
    Vector2 position;
    Vector2 velocity;
    List<Vector3> allPositions;
    public List<Vector2> velocities;
    public List<Vector2> actualVels;
    public List<PointEffector2D> effectors;
    public LineRenderer lineRenderer;
    public static OrbitPredictor instance;
    // how much the initial velocity in the predictor needs to increase for the prediction to still work
    public float onGroundIncrease;

    // Use this for initialization
	void Start ()
    {
        actualVels = new List<Vector2>();
        instance = this;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    // used to predict how orbits of moon and planets that should not be effected by the ball will be 
    //then returns the orbit path so its not needed to be simulated again
    public void SimulateUntilOrbit(Vector2 pos, float pretendMass, float rad, Vector2 initialForce, int maxSteps = 9999)
    {
        drag = 0;
        allPositions = new List<Vector3>();
        velocities = new List<Vector2>();
        velocities.Add(velocity);
        allPositions.Add(pos);
        position = pos;
        radius = rad;
        mass = pretendMass;
        Vector2 vel = (initialForce / pretendMass) * Time.fixedDeltaTime;
        velocity = vel;
        int i = 0;
        while(i < maxSteps)
        {
            PhysicsStep();
            allPositions.Add(new Vector3(position.x, position.y, 0));
            velocities.Add(velocity);
            if (i > 15)
            {
                if (Vector2.Distance(allPositions[0], allPositions[allPositions.Count - 1]) < 0.01f)
                {
                    break;
                }
            }

            i++;
        }

        if (i == 999) Debug.Log("hit max steps");

        Render();
    }
    public void Simulate(Rigidbody2D body, Vector2 initialForce, int steps)
    {
     
        drag = body.GetComponent<ClickAndDragForce>().storedDrag;
        Vector2 vel = (initialForce / body.mass) * Time.fixedDeltaTime;      
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
    public Vector2[] SimulationPositions()
    {
        Vector2[] final = new Vector2[allPositions.Count];
        int i = 0;
        foreach (Vector3 pos in allPositions)
        {
            final[i] = pos.XY();
            i++;
        }

        return final;
    }

    void Render()
    {
       
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
        if (drag != 0)
        { velocity *= (1f - (Time.fixedDeltaTime * drag)); }

        position += velocity * Time.fixedDeltaTime;
    }
}
