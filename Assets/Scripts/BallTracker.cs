using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallTracker : MonoBehaviour
{

    public List<OrbitTracker> planets;
    // Use this for initialization
    void Start()
    {
        foreach (OrbitTracker tracker in planets)
        {
            tracker.ball = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float minDist = 99;
        int minIndex = 0;
        int i = 0;
        foreach (OrbitTracker tracker in planets)
        {
            float dist = Vector2.Distance(tracker.transform.position, transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                minIndex = i;
            }
            i++;
        }
        i = 0;
        foreach (OrbitTracker tracker in planets)
        {
            tracker.closest = i == minIndex;
            i++;
        }

    }
}
