using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepObjectOnScreen : MonoBehaviour {

	public Transform topLeft, bottomRight, player;
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 movement = Vector3.zero;
        if (player.position.y > topLeft.position.y)
        {
            float diff = player.position.y - topLeft.position.y;
            movement.y += diff;
        }
        else
        {
            if(player.position.y < bottomRight.position.y)
            {
                float diff = player.position.y - bottomRight.position.y;
                movement.y += diff;
            }
        }

        if (player.position.x < topLeft.position.x)
        {
            float diff = player.position.x - topLeft.position.x;
            movement.x += diff;
        }
        else
        {
            if (player.position.x > bottomRight.position.x)
            {
                float diff = player.position.x - bottomRight.position.x;
                movement.x += diff;
            }
        }

        transform.position += movement;

    }
}
