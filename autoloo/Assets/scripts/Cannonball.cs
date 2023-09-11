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
        if (i == 0)
        {
            transform.position = new Vector3(transform.position.x + (float)FlightpathPoints[i].Item1, transform.position.y + (float)FlightpathPoints[i].Item2, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x + (float)(FlightpathPoints[i].Item1 - FlightpathPoints[i-1].Item1), transform.position.y + (float)(FlightpathPoints[i].Item2 - FlightpathPoints[i - 1].Item2), transform.position.z);
        }
        i++;
        if (i == FlightpathPoints.Count)
        {
            Destroy(gameObject);
        }
    }
}
