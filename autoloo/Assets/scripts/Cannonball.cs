using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    int i = 0;
    public List<(double, double)> FlightpathPoints;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3((float)FlightpathPoints[i].Item1, (float)FlightpathPoints[i].Item2, transform.position.z);
        i++;
        if (i == FlightpathPoints.Count)
        {
            Destroy(gameObject);
        }
    }
}
