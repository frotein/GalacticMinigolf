using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour {

    public GameObject nextLevel;
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.transform.tag == "Player")
        {
            col.gameObject.SetActive(false);
            if(nextLevel != null)
                nextLevel.SetActive(true);
            transform.TopParent().gameObject.SetActive(false);
        }
    }
}
