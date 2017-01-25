using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepObjectOnScreen : MonoBehaviour {

	public Transform min, max, player;
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 movement = Vector3.zero;
        if (player.position.y > max.position.y)
        {
            float diff = player.position.y - max.position.y;
            movement.y += diff;
        }
        else
        {
            if(player.position.y < min.position.y)
            {
                float diff = player.position.y - min.position.y;
                movement.y += diff;
            }
        }

        if (player.position.x < min.position.x)
        {
            float diff = player.position.x - min.position.x;
            movement.x += diff;
        }
        else
        {
            if (player.position.x > max.position.x)
            {
                float diff = player.position.x - max.position.x;
                movement.x += diff;
            }
        }

        transform.position += movement;

    }
}
