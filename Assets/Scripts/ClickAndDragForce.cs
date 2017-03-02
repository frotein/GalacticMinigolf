using UnityEngine;
using System.Collections;

public class ClickAndDragForce : MonoBehaviour {

    public float forceIncreaseRate;
    public float scale;
    public Collider2D grabCollider;
    public bool grabbing;
    Ball ball;
    public OrbitPredictor preditor;
    public KeepObjectOnScreen movingCamera;
    public Vector2 initialForce;
    bool nextFrame = false;
    public float storedDrag;
    int waitFrames;
    // Use this for initialization
	void Start ()
    {
       
        ball = transform.GetComponent<Ball>();
      //  storedDrag = rigidBody.drag;
        StaticObjects.golfBall = transform;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(nextFrame)
        {
            foreach (Effector2D eff in preditor.effectors)
            {
                Collider2D[] cols = eff.GetComponents<Collider2D>();
                foreach(Collider2D col in cols)
                {
                    if (!col.isTrigger)
                        col.enabled = true;
                }
            }
            nextFrame = false;
        }
        if (waitFrames > 0)
        {
            if (Time.timeScale > 0)
            {
                waitFrames--;
                if (waitFrames == 0)
                {
                    OrbitPredictor.instance.ResetConsistentLine();
                    OrbitPredictor.instance.showConsistentLine = true;
                }
            }
        }
        if (Controls.Clicked() && !grabbing)
        {
            Vector2 worldPos = Controls.ClickedPosition();
            if (grabCollider.OverlapPoint(worldPos))
            {
                grabbing = true;
            }
        }

        if(grabbing)
        {
            Vector2 worldPos = Controls.ClickedPosition();
            Vector2 dirAndPower = worldPos - (Vector2)transform.position;
           if(!preditor.simulating)
             preditor.RunSimulateCoroutine(ball, dirAndPower * -forceIncreaseRate / scale, ((int)(500 * scale)));
        }
        if(Controls.Released() && grabbing)
        {
            preditor.SetLineToConsistent();
            ball.simulate = true;
            movingCamera.enabled = true;
            grabbing = false;
        }

       

        
	}

    void FixedUpdate()
    {
    
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        ball.enabled = false;
    }

    void OnCollisionExit2D(Collision2D col)
    {
       
    }

    void OnCollisionStay2D(Collision2D col)
    {
        
    }

    /*void ApplyForce()
    {

        Vector2 worldPos = Controls.ClickedPosition();
        Vector2 dirAndPower = worldPos - (Vector2)transform.position;
        
        ball.velocity += (dirAndPower * -forceIncreaseRate / scale);
        ball.simulate = true;
        waitFrames = 5;
        nextFrame = true;
        OrbitPredictor.instance.showConsistentLine = false;
    }*/
}
