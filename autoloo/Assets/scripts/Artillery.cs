using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Artillery : MonoBehaviour
{
    public Unit unit;
    public int range;
    public bool grapeMode = false;
    public Sprite ball;
    public Sprite grape;
    public GameObject cannonball;
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
        //unit.textAttack.gameObject.GetComponent<SpriteRenderer>().sprite = ball;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire(Unit target)
    {
        if (Math.Abs(unit.QueuePosition) == 1)
        {
            ShootGrape(target);
        }
        else if (Math.Abs(unit.QueuePosition) <= range)
        {
            ShootBall(target);
        }
    }

    public void ShootBall(Unit target)
    {
        //get the enemy
        //the enemy is the guy where the queue position is -1 * (myqueueposition/math.abs(myqueueposition))
        //CHECK THAT
        Debug.Log($"firing a ball at {target.name}");
        target.HitPoints -= unit.Attack;
        var dist = (target.transform.position.x - unit.transform.position.x);
        var flyingball = Instantiate(cannonball);
        flyingball.transform.position = this.transform.position;
        Thread.Sleep(1000);
        flyingball.GetComponent<Cannonball>().FlightpathPoints = CannonballFlightpath(50, 12, dist);
        //the target can be known at the time fire is called from Gamemanager PreBattlePhase() ( see how it is done in gamemanager Fight()  )
        //do not do the damage until the "flying ball" collides with the enemy unit
        //the flying ball needs to get there before the attack pahse goes off (need to delay next beat? yes)
    }

    public void ShootGrape(Unit target)
    {
        Debug.Log($"will fire grape at {target.name}...");
        //the deduction of enemy HP for grape shot is done in the Battle Phase
    }

    internal void SetAttackByQueue(int e)
    {
        if (Math.Abs(e) == 1)
        {
            grapeMode = true;
            unit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = grape;
            unit.AttackBonus = 3;
        }
        else
        {
            grapeMode = false;
            unit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = ball;
            unit.AttackBonus = 0;
        }
    }

    private static List<(double, double)> CannonballFlightpath(double muzzleVelocity, double weightInPounds, double targetDistance)
    {
        List<(double, double)> flightpathPoints = new List<(double, double)>();
        // Constants
        const double gravitationalAcceleration = 9.81; // m/s^2, approximate value on Earth's surface

        // Convert weight from pounds to kilograms
        double weightInKilograms = weightInPounds * 0.453592;

        // Calculate the angle of projection (in radians)
        double angleInRadians = Math.Asin((targetDistance * gravitationalAcceleration) / (muzzleVelocity * muzzleVelocity)) / 2;

        // Calculate the initial height
        double initialHeight = 0; // Assuming the cannon and target are at the same elevation

        // Calculate the maximum height
        double maxHeight = Math.Pow((muzzleVelocity * Math.Sin(angleInRadians)), 2) / (2 * gravitationalAcceleration);

        // Calculate the horizontal range (arc length)
        double range = (muzzleVelocity * muzzleVelocity * Math.Sin(2 * angleInRadians)) / gravitationalAcceleration;

        // Calculate and output heights at various horizontal distances
        for (int i = 5; i <= 100; i += 5)
        {
            double horizontalDistance = Math.Round((i / 100.0) * range, 1);
            double verticalHeight = Math.Round(initialHeight + horizontalDistance * Math.Tan(angleInRadians) - (gravitationalAcceleration * horizontalDistance * horizontalDistance) / (2 * Math.Pow(muzzleVelocity * Math.Cos(angleInRadians), 2)), 1);
            flightpathPoints.Add(new(horizontalDistance, verticalHeight * 5));
        }

        return flightpathPoints;
    }

}
