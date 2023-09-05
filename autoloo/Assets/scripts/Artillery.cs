using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artillery : MonoBehaviour
{
    public Unit unit;
    public int range;
    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<Unit>();
        if (unit.spriteName.Contains("heavy"))
        {
            range = 5;
        }
        else 
        {
            range = 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire()
    {
        if (Math.Abs(unit.QueuePosition) == 1)
        {
            ShootGrape();
        }
        else if (Math.Abs(unit.QueuePosition) <= range)
        {
            ShootBall();
        }
    }

    public void ShootBall()
    {
        //get the enemy
        //the enemy is the guy where the queue position is -1 * (myqueueposition/math.abs(myqueueposition))
        //do not do the damage until the "flying ball" collides with the enemy unit
        //the flying ball needs to get there before the attack pahse goes off (need to delay next beat? yes)
    }

    public void ShootGrape()
    {

    }
}
