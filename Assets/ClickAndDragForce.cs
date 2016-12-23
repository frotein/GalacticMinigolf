using UnityEngine;
using System.Collections;

public class ClickAndDragForce : MonoBehaviour {

    public float forceIncreaseRate;
    Collider2D collider;
    bool grabbing;
    Rigidbody2D rigidBody;
    public OrbitPredictor preditor;
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
            nextFrame = false;
        }
       
        if (Input.GetMouseButtonDown(0) && !grabbing)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (collider.OverlapPoint(worldPos))
            {
                grabbing = true;
            }
        }

        if(grabbing)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dirAndPower = worldPos - (Vector2)transform.position;
            preditor.Simulate(rigidBody, dirAndPower * -forceIncreaseRate, 300);
        }
        if(Input.GetMouseButtonUp(0) && grabbing)
        {
            ApplyForce();
            grabbing = false;
        }

       

        
	}

    void FixedUpdate()
    {
       /* if (!rigidBody.isKinematic)
        {
            preditor.actualVels.Add(rigidBody.velocity);
        }*/
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

    void ApplyForce()
    {

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dirAndPower = worldPos - (Vector2)transform.position;
        rigidBody.isKinematic = false;
        rigidBody.drag = storedDrag;
        Debug.Log(rigidBody.drag);
        rigidBody.AddForce(dirAndPower * -forceIncreaseRate);
        foreach(Effector2D eff in preditor.effectors)
        {
            Collider2D[] cols = eff.GetComponents<Collider2D>();
            foreach (Collider2D col in cols)
            {
                if (!col.isTrigger)
                    col.enabled = false;
            }
        }
        nextFrame = true;
    }
}
