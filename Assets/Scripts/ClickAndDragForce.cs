using UnityEngine;
using System.Collections;

public class ClickAndDragForce : MonoBehaviour {

    public float forceIncreaseRate;
    public float scale;
    Collider2D collider;
    public bool grabbing;
    Rigidbody2D rigidBody;
    public OrbitPredictor preditor;
    public KeepObjectOnScreen movingCamera;
    public Vector2 initialForce;
    bool nextFrame = false;
    public float storedDrag;
    // Use this for initialization
	void Start ()
    {
        collider = transform.GetComponent<Collider2D>();
        rigidBody = transform.GetComponent<Rigidbody2D>();
        storedDrag = rigidBody.drag;
       
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
            Debug.Log(rigidBody.velocity + " after velocity");
            nextFrame = false;
        }
       
        if (Controls.Clicked() && !grabbing)
        {
            Vector2 worldPos = Controls.ClickedPosition();
            if (collider.OverlapPoint(worldPos))
            {
                grabbing = true;
            }
        }

        if(grabbing)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dirAndPower = worldPos - (Vector2)transform.position;
           
            preditor.Simulate(rigidBody, dirAndPower * -forceIncreaseRate / scale, ((int)(300 * scale)));
        }
        if(Controls.Released() && grabbing)
        {
            ApplyForce();
            movingCamera.enabled = true;
            grabbing = false;
        }

       

        
	}

    void FixedUpdate()
    {
    
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Planet"))
        {
            rigidBody.drag = 2;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Planet"))
        {
           // rigidBody.drag = .1f;
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if(rigidBody.velocity.magnitude < 0.2f && col.relativeVelocity.magnitude < 0.2f)
        {
            rigidBody.isKinematic = true;
            transform.parent = col.gameObject.transform;
        }
    }

    void ApplyForce()
    {

        Vector2 worldPos = Controls.ClickedPosition();
        Vector2 dirAndPower = worldPos - (Vector2)transform.position;
        if(!rigidBody.simulated)
            rigidBody.simulated = true;
        rigidBody.drag = storedDrag;
        Debug.Log(rigidBody.velocity + " before velocity");
        Debug.Log("hit velocity " + (((dirAndPower * -forceIncreaseRate) / rigidBody.mass) * Time.fixedDeltaTime));
        rigidBody.AddForce(dirAndPower * -forceIncreaseRate / scale);
        
       /* foreach(Effector2D eff in preditor.effectors)
        {
            Collider2D[] cols = eff.GetComponents<Collider2D>();
            foreach (Collider2D col in cols)
            {
                if (!col.isTrigger)
                    col.enabled = false;
            }
        }*/
        nextFrame = true;
    }
}
