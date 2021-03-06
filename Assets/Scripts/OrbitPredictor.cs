﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitPredictor : MonoBehaviour {

    float mass;
    public float drag;
    public float scale;
    public bool hasConsistentLine;
    public bool showConsistentLine;
    public int consistentLineLength;
    float radius;
    Vector2 position;
    Vector2 velocity;
    List<Vector3> allPositions;
    public float quality;
    public List<Vector2> velocities;
    public List<Vector2> actualVels;
    public List<PointEffector2D> effectors;
    List<Vector3> consistentLinePositions;
    Vector2 consistentLinePosition;
    Vector2 consistentLineVelocity;
    public LineRenderer lineRenderer;
    public static OrbitPredictor instance;
    // how much the initial velocity in the predictor needs to increase for the prediction to still work
    public float onGroundIncrease;
    public Ball ball;
    float consistentLineMass;
    public bool simulating;
    int steps;
    public bool consistentLineHitPlanet;
    public bool lineHitPlanet;
    int currentStep = 0;
    // Use this for initialization
	void Start ()
    {
        actualVels = new List<Vector2>();
        instance = this;
        ResetConsistentLine();
        lineHitPlanet = false;
        

    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (hasConsistentLine)
        {
            if(consistentLinePositions.Count > 0)
                consistentLinePositions.RemoveAt(0);
            //
            if(showConsistentLine)
                DrawConsistentLine();   
        }
    }
    public void ResetConsistentLine()
    {
        consistentLinePositions = new List<Vector3>();
        Vector2 vel = ball.velocity;
        consistentLineMass = ball.mass;
        consistentLinePosition = ball.transform.position;
        consistentLineVelocity = vel;
        consistentLineHitPlanet = false;
        if (hasConsistentLine)
        {
            SimulateConsistentLineForDistance(consistentLineLength);
        }
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

       // Render();
    }
    public void SimulateMoving(Rigidbody2D body, float time)
    {
        drag = body.GetComponent<ClickAndDragForce>().storedDrag;
        radius = body.GetComponent<CircleCollider2D>().radius;
        Vector2 vel = body.velocity;
        InitialValues(body.position, vel, body.mass);
        allPositions = new List<Vector3>();
        velocities = new List<Vector2>();
        velocities.Add(velocity);
        allPositions.Add(body.position);
        for (int i = 0; i < time / Time.deltaTime; i++)
        {
            PhysicsStep();
            allPositions.Add(new Vector3(position.x, position.y, 0));
            velocities.Add(velocity);
        }

        Render();
    }
    public void SimulateConsistentLineForDistance(float dist)
    {
        drag = 0;
        radius = ball.GetComponent<CircleCollider2D>().radius;
        float lnth = TotalLengthForConsistentLine();
        int stepsRan = 0;
       
        while (lnth < dist && !consistentLineHitPlanet)
        {
            PhysicsStepForConsistentLine();
            consistentLinePositions.Add(new Vector3(consistentLinePosition.x, consistentLinePosition.y, 0));
            lnth = TotalLengthForConsistentLine();
            stepsRan++;
            currentStep = stepsRan;
        }
    }

    


    public void SimulateForDistance(Rigidbody2D body, float dist)
    {
        drag = body.GetComponent<ClickAndDragForce>().storedDrag;
        radius = body.GetComponent<CircleCollider2D>().radius;
        Vector2 vel = body.velocity;
        InitialValues(body.position, vel, body.mass);
        allPositions = new List<Vector3>();
        velocities = new List<Vector2>();
        velocities.Add(velocity);
        //allPositions.Add(body.position);
        float lnth = TotalLength();
        int stepsRan = 0;
        while (lnth < dist)
        {
            PhysicsStep();
            allPositions.Add(new Vector3(position.x, position.y, 0));
            velocities.Add(velocity);
            lnth = TotalLength();
            stepsRan++;
        }
        Render();
    }

    public void Simulate(Ball body, Vector2 vell, int steps)
    {
     
        drag = body.GetComponent<ClickAndDragForce>().storedDrag;
        Vector2 vel = vell;      
        radius = body.GetComponent<CircleCollider2D>().radius;
        InitialValues(body.transform.position, body.velocity +  vel , body.mass);
        //Debug.Log((" initial vel = " + velocity));
        allPositions = new List<Vector3>();
        velocities = new List<Vector2>();
        velocities.Add(velocity);
        allPositions.Add(body.transform.position);
        for (int i = 0; i < steps; i++)
        {
            PhysicsStep();
            allPositions.Add(new Vector3(position.x, position.y,0));
            velocities.Add(velocity);
        }

        Render();
    }
    public void RunSimulateCoroutine(Ball body, Vector2 vel, int steps)
    {
        lineHitPlanet = false;
        InitialValues(body.transform.position, body.velocity + vel, body.mass);
        this.steps = steps;
        StartCoroutine("SimulateCo");

    }
    public IEnumerator SimulateCo()
    {
        simulating = true;
        allPositions = new List<Vector3>();
        currentStep = 0;
        for (int i = 0; i < steps; i++)
        {
            PhysicsStep();
            allPositions.Add(new Vector3(position.x, position.y, 0));
            //   if (i % 600 == 0)
            //     yield return null;
            if (lineHitPlanet) i = steps;

            currentStep++;
        }

        simulating = false;

        Render();

        yield return null;
        
        
    }
    public void SetLineToConsistent()
    {
        consistentLinePositions = new List<Vector3>(allPositions);
        consistentLinePosition = consistentLinePositions[consistentLinePositions.Count -  1];
        consistentLineVelocity = velocity;
        consistentLineMass = mass;
        consistentLineHitPlanet = false;
    }
    void DrawConsistentLine()
    {
        lineRenderer.numPositions = consistentLinePositions.Count;
        lineRenderer.SetPositions(consistentLinePositions.ToArray());
    }

    // returns the recorded positions in local space of par, if par is null it is in world space
    public Vector2[] SimulationPositions(Transform par = null)
    {
        Vector2[] final = new Vector2[allPositions.Count];
        int i = 0;
        foreach (Vector3 pos in allPositions)
        {
            final[i] = pos.XY();
            if(par != null)
            {
                final[i] = par.InverseTransformPoint(final[i]);
            }
            i++;
        }

        return final;
    }
    public float TotalLength()
    {
        float lnth = 0;
        for(int i = 0; i < allPositions.Count - 1; i++)
        {
            Vector2 pos1 = allPositions[i];
            Vector2 pos2 = allPositions[(i + 1) % allPositions.Count];
            lnth += Vector2.Distance(pos1, pos2);

        }

        return lnth;
    }

    public float TotalLengthForConsistentLine()
    {
        float lnth = 0;
        for (int i = 0; i < consistentLinePositions.Count - 1; i++)
        {
            Vector2 pos1 = consistentLinePositions[i];
            Vector2 pos2 = consistentLinePositions[(i + 1) % consistentLinePositions.Count];
            lnth += Vector2.Distance(pos1, pos2);

        }

        return lnth;
    }


    void Render()
    {
        lineRenderer. numPositions = allPositions.Count;
        lineRenderer.SetPositions(allPositions.ToArray());
    }

    public void RenderConsistentLine()
    {
        lineRenderer.numPositions = consistentLinePositions.Count;
        lineRenderer.SetPositions(consistentLinePositions.ToArray());
    }
    void InitialValues(Vector2 pos, Vector2 vel, float mas)
    {
        mass = mas;
        position = pos;
        velocity = vel;
    }


    public void Step(Ball bal)
    {
       
        Collider2D[] cols2 = Physics2D.OverlapCircleAll(bal.transform.position, radius);
        foreach (Collider2D col in cols2)
        {
            if (col.usedByEffector)
            {
                PointEffector2D effector = col.GetComponent<PointEffector2D>();
                Vector2 dir = new Vector2(effector.transform.position.x - bal.transform.position.x, effector.transform.position.y - bal.transform.position.y);
                float distSqr = dir.magnitude * effector.distanceScale;
                if (effector.forceMode == EffectorForceMode2D.InverseSquared)
                    distSqr *= distSqr;
                float force = effector.forceMagnitude / (distSqr);
                float vel = (force / bal.mass);
                vel *= Time.fixedDeltaTime;

                bal.velocity += -dir.normalized * vel;
            }
        }
        if (bal.drag != 0)
        { bal.velocity *= (1f - (Time.fixedDeltaTime * bal.drag)); }

        bal.transform.position += (bal.velocity * Time.fixedDeltaTime).XYZ(0);
    }
    void PhysicsStep()
    {
        foreach(PointEffector2D effector in effectors)
        {
            Vector2 dir = new Vector2(effector.transform.position.x - position.x, effector.transform.position.y - position.y);
            float distSqr = dir.magnitude * effector.distanceScale;
            float effectorRadius = effector.GetComponent<CircleCollider2D>().radius * effector.transform.lossyScale.x;
            if (dir.magnitude < effectorRadius + 1f && currentStep > 10)
            { lineHitPlanet = true;  }
            

            if (effector.forceMode == EffectorForceMode2D.InverseSquared)
                distSqr *= distSqr;
            float force = effector.forceMagnitude / (distSqr);
            float vel = (force / mass);
            vel *= Time.fixedDeltaTime;

            velocity += -dir.normalized * vel;
            
        }
        if (drag != 0)
        { velocity *= (1f - (Time.fixedDeltaTime * drag)); }

        position += velocity * Time.fixedDeltaTime;
    }

    public void PhysicsStepForConsistentLine(Ball b = null)
    {
        if (!consistentLineHitPlanet)
        {
            foreach (PointEffector2D effector in effectors)
            {
                Vector2 dir = new Vector2(effector.transform.position.x - consistentLinePosition.x, effector.transform.position.y - consistentLinePosition.y);
                float effectorRadius = effector.GetComponent<CircleCollider2D>().radius * effector.transform.lossyScale.x;
                if (b != null)
                {
                    if (dir.magnitude < effectorRadius + b.radius && currentStep > 10)
                    { consistentLineHitPlanet = true; Debug.Log("Hitting " + effector.transform.name); }
                }
                float distSqr = dir.magnitude * effector.distanceScale;
                if (effector.forceMode == EffectorForceMode2D.InverseSquared)
                    distSqr *= distSqr;
                float force = effector.forceMagnitude / (distSqr);
                float vel = (force / consistentLineMass);
                vel *= Time.fixedDeltaTime;

                consistentLineVelocity += -dir.normalized * vel;
            }

            if (drag != 0)
            { consistentLineVelocity *= (1f - (Time.fixedDeltaTime * drag)); }

            consistentLinePosition += consistentLineVelocity * Time.fixedDeltaTime;

            consistentLinePositions.Add(consistentLinePosition);
        }
        // consistentLinePositions.RemoveAt(0);
        if (b != null)
        {
            b.transform.position = consistentLinePositions[0];
            if (consistentLinePositions.Count <= 1)
            { b.simulate = false; consistentLineHitPlanet = false; }
        }

       
       
    }
}
