using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Artillery : MonoBehaviour
{
    public Unit unit;
    public int range;
    public bool showArtilleryEffect = false;
    public int effectFrame = 0;
    public Sprite ball;
    public Sprite grape;
    public Sprite flashEffectLeft;
    public Sprite flashEffectRight;
    public Sprite smokeEffectLeft;
    public Sprite smokeEffectRight;
    public GameObject cannonball;
    public AudioClip acLoadGrapShot;
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
        if (showArtilleryEffect)
        {
            FireCannonEffect();
        }
    }

    public void Fire(Unit target)
    {
        if (Math.Abs(unit.QueuePosition) == 1)
        {
            PrepareToShootGrape(target);
        }
        else if (Math.Abs(unit.QueuePosition) <= range)
        {
            ShootBall(target);
        }
    }

    public void ShootBall(Unit target)
    {
        ShowEffect();
        //Debug.Log($"Firing a ball at {target.name}");

        // Calculate the distance to the target
        float distanceToTarget = target.transform.position.x - unit.transform.position.x;

        // Create a new cannonball and set its properties
        GameObject flyingBall = Instantiate(cannonball, transform.position, Quaternion.identity);
        Cannonball cannonballComponent = flyingBall.GetComponent<Cannonball>();

        cannonballComponent.FlightpathPoints = CannonballFlightpath(50, 12, distanceToTarget, 20);
        cannonballComponent.damage = unit.Attack;
        cannonballComponent.target = target;
        cannonballComponent.gameManager = unit.gameManager;
    }


    public void PrepareToShootGrape(Unit target)
    {
        //add the voice audio
        unit.gameManager.PlayTransientAudioClip(acLoadGrapShot);
        //Debug.Log($"will fire grape at {target.name}...");
        //the deduction of enemy HP for grape shot is done in the Battle Phase
    }

    internal void SetAttackByQueue(int e)
    {
        if (Math.Abs(e) == 1)
        {
            unit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = grape;
            unit.AttackBonus = 3;
        }
        else
        {
            unit.textAttack.gameObject.GetComponentInChildren<SpriteRenderer>().sprite = ball;
            unit.AttackBonus = 0;
        }
    }

    private static List<(double, double)> CannonballFlightpath(double muzzleVelocity, double weightInPounds, double targetDistance, int cannonballFrames)
    {
        List<(double, double)> flightpathPoints = new List<(double, double)>();
        const double gravitationalAcceleration = 9.81; // m/s^2, approximate value on Earth's surface
        double weightInKilograms = weightInPounds * 0.453592;
        double angleInRadians = Math.Asin((targetDistance * gravitationalAcceleration) / (muzzleVelocity * muzzleVelocity)) / 2;
        double initialHeight = 0;
        double maxHeight = Math.Pow((muzzleVelocity * Math.Sin(angleInRadians)), 2) / (2 * gravitationalAcceleration);
        double range = (muzzleVelocity * muzzleVelocity * Math.Sin(2 * angleInRadians)) / gravitationalAcceleration;
        var interationStep = (int)(100 / cannonballFrames);
        for (int i = interationStep; i <= 100; i += interationStep)
        {
            double horizontalDistance = Math.Round((i / 100.0) * range, 4);
            double verticalHeight = Math.Round(initialHeight + horizontalDistance * Math.Tan(angleInRadians) - (gravitationalAcceleration * horizontalDistance * horizontalDistance) / (2 * Math.Pow(muzzleVelocity * Math.Cos(angleInRadians), 2)), 4);
            flightpathPoints.Add(new(horizontalDistance, verticalHeight * 5));
            if ((i/5) >= cannonballFrames)
            {
                break;
            }
        }
        return flightpathPoints;
    }

    public void FireCannonEffect()
    {
        unit.gameManager.PlayCannonballFire();
        Debug.Log("cannon fire effect needed");
        HideEffect();
    }

    private void HideEffect()
    {
        showArtilleryEffect = false;
        effectFrame = 0;
    }

    public void ShowEffect()
    {
        showArtilleryEffect = true;
    }
}
