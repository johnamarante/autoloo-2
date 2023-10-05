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
    public bool grapeMode = false;
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
            ShootGrape(target);
        }
        else if (Math.Abs(unit.QueuePosition) <= range)
        {
            ShootBall(target);
        }
    }

    public void ShootBall(Unit target)
    {
        ShowEffect();
        Debug.Log($"Firing a ball at {target.name}");

        // Calculate the distance to the target
        float distanceToTarget = target.transform.position.x - unit.transform.position.x;

        // Create a new cannonball and set its properties
        GameObject flyingBall = Instantiate(cannonball, transform.position, Quaternion.identity);
        Cannonball cannonballComponent = flyingBall.GetComponent<Cannonball>();

        cannonballComponent.FlightpathPoints = CannonballFlightpath(50, 12, distanceToTarget, (int)(unit.gameManager.realFPS/3));
        cannonballComponent.damage = unit.Attack;
        cannonballComponent.target = target;
        cannonballComponent.manager = unit.gameManager;
    }


    public void ShootGrape(Unit target)
    {
        //add the voice audio
        unit.gameManager.PlayTransientAudioClip(acLoadGrapShot);
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

    private static List<(double, double)> CannonballFlightpath(double muzzleVelocity, double weightInPounds, double targetDistance, int cannonballFrames)
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

        // Calculate iteration step
        var interationStep = (int)(100 / cannonballFrames);

        // Calculate and output heights at various horizontal distances
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
        switch (effectFrame)
        {
            case 1:
                unit.gameManager.PlayCannonballFire();
                ShowFlashEffect();
                break;
            case 10:
                ShowSmokeEffect();
                break;
        }

        effectFrame++;

        if (effectFrame > 60)
        {
            HideEffect();
        }
    }

    private void ShowFlashEffect()
    {
        unit.effectsComponent.enabled = true;
        unit.effectsComponent.sprite = (unit.side == "left") ? flashEffectLeft : flashEffectRight;
        var uniteffectscomponentspritename = unit.effectsComponent.sprite.name;
        unit.effectsComponent.transform.position = new Vector3(gameObject.transform.position.x + Int32.Parse(uniteffectscomponentspritename.Split("_")[3]), gameObject.transform.position.y + Int32.Parse(uniteffectscomponentspritename.Split("_")[4]), unit.effectsComponent.transform.position.z);
    }

    private void ShowSmokeEffect()
    {
        unit.effectsComponent.sprite = (unit.side == "left") ? smokeEffectLeft : smokeEffectRight;
    }

    private void HideEffect()
    {
        showArtilleryEffect = false;
        unit.effectsComponent.enabled = false;
        effectFrame = 0;
    }

    public void ShowEffect()
    {
        showArtilleryEffect = true;
    }
}
