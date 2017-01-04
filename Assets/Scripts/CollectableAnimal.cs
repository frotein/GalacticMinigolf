using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableAnimal : MonoBehaviour {

    public Transform planetOn;
    bool collected;
    public bool snapToPlanet;
    // Use this for initialization
	void Start ()
    {
        collected = false;
        Vector2 dir = transform.position - planetOn.position;
        transform.up = dir.normalized;
        BoxCollider2D myCollider = transform.GetComponent<BoxCollider2D>();
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
                        float radius = col.radius * planetOn.transform.localScale.x;
                        transform.position = planetOn.position + (dir.normalized * (radius + (myCollider.size.y / 2f))).XYZ(0);
                        break;
                    }
                }
            }

            transform.parent = planetOn;
        }


    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.transform.tag == "Player")
        {
            collected = true;
            transform.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
