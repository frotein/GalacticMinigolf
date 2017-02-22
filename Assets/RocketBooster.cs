using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RocketBooster : MonoBehaviour {

    public float power, fuel, fuelConsumptionRate;
    Vector2 direction;
    Rigidbody2D body;
    // Use this for initialization
	void Start ()
    {
        body = transform.GetComponent<Rigidbody2D>();	
	}
	
	// Update is called once per frame
	void Update ()
    {
	   /* if(!body.isKinematic)
        {
            if(Controls.Held() && fuel > 0 && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 direction = (Controls.ClickedPosition() - transform.position.XY()).normalized;
                body.AddForce(direction * power * Time.deltaTime);
                fuel -= fuelConsumptionRate * Time.deltaTime;
               
            }

            
        }

        OrbitPredictor.instance.SimulateForDistance(body, 20f);*/
    } 
}
