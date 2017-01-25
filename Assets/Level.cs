using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

    public Vector2 max;
    public Vector2 min; // the maximum and minimun positions of This level
    // Use this for initialization
	void Start ()
    {
        max = new Vector2(-999, -999);
        min = new Vector2(999, 999);
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform tr = transform.GetChild(i);
            Renderer rend = tr.GetComponent<Renderer>();
            if(rend != null)
            {
                Vector2 newMin = rend.bounds.min;
                Vector2 newMax = rend.bounds.max;
                min.x = Mathf.Min(min.x, newMin.x);
                min.y = Mathf.Min(min.y, newMin.y);
                max.x = Mathf.Max(max.x, newMax.x);
                max.y = Mathf.Max(max.y, newMax.y);
            }
        }	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
