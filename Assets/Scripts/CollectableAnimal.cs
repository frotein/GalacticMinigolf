using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableAnimal : MonoBehaviour {

    public Transform planetOn;
    bool collected;
    public bool snapToPlanet;
    float radiusSqr;
    // Use this for initialization
	void Start ()
    {
        collected = false;
        Vector2 dir = transform.position - planetOn.position;
        transform.up = dir.normalized;
        CircleCollider2D myCollider = transform.GetComponent<CircleCollider2D>();
        float radius = myCollider.radius * transform.lossyScale.x;
        radiusSqr = radius * radius;
         // get the collider used for the planets surface do the animal can be placed Collectly on the ground
        CircleCollider2D[] cols = planetOn.GetComponents<CircleCollider2D>();

        if (snapToPlanet)
        {
            if (cols != null)
            {
                foreach (CircleCollider2D col in cols)
                {
                    if (!col.isTrigger)
                    {
                        float radi = col.radius * planetOn.transform.localScale.x;
                        //transform.position = planetOn.position + (dir.normalized * (radi + (myCollider.size.y / 2f))).XYZ(0);
                        break;
                    }
                }
            }

            transform.parent = planetOn;
        }


    }
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        float distSqr = Mathf.Abs(transform.position.x - Ball.instance.transform.position.x) + Mathf.Abs(transform.position.y - Ball.instance.transform.position.y);
        if (distSqr < radiusSqr) Collected();
    }

   
    void Collected()
    {
        collected = true;
        transform.GetComponent<SpriteRenderer>().enabled = false;
    }
}
