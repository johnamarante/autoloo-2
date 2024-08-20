using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    public List<(double, double)> FlightpathPoints;
    public Unit target;
    public int damage;
    public GameManager gameManager;
    private float timeInterval;
    private int i = 0;

    private void Start()
    {
        timeInterval = gameManager.period / (FlightpathPoints.Count);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= timeInterval)
        {
            if (i == 0)
            {
                transform.position = new Vector3(transform.position.x + (float)FlightpathPoints[i].Item1, transform.position.y + (float)FlightpathPoints[i].Item2, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x + (float)(FlightpathPoints[i].Item1 - FlightpathPoints[i - 1].Item1), transform.position.y + (float)(FlightpathPoints[i].Item2 - FlightpathPoints[i - 1].Item2), transform.position.z);
            }
            i++;
        }
        if (i == FlightpathPoints.Count)
        {
            if (target.Squared)
            {
                gameManager.floatyNumber.SpawnFloatingString($"{damage} × 2 \nCRIT!", Color.red, target.transform.position);
                damage = damage * 2;
            }
            else
            {
                gameManager.floatyNumber.SpawnFloatingNumber(-damage,target.transform.position);
            }

            target.HitPoints -= damage;
            //SpawnFloatingNumber should only ever be called when HitPoints changes
            //manager.floatyNumber.SpawnFloatingNumber(-1*damage, target.transform.position, true);
            target.gameManager.PlayCannonballHit();
            Destroy(this.gameObject);
        }
    }
}
