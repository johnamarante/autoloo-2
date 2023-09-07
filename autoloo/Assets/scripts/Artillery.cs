using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artillery : MonoBehaviour
{
    public Unit unit;
    public int range;
    public bool grapeMode = false;
    public Sprite ball;
    public Sprite grape;
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
}
