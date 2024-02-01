using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavalry : MonoBehaviour
{
    public Unit unit;

    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnDestroy()
    {
        try
        {
            Unit opposingUnit = (unit.side == "left") ? unit.gameManager.RightQueueUnits[0] : unit.gameManager.LeftQueueUnits[0];
            //below logic assumes that this cavalry unit has already been removed from the queue.
            //this assumption is correct because Gamemanager.CleanupAndMove() will cleanup the L&R queues in the same frame as the objects are destroyed. 
            //That frame will complete before this OnDestroy is called.
            Unit behindUnit = (unit.side == "left") ? unit.gameManager.LeftQueueUnits[0] : unit.gameManager.RightQueueUnits[0];
            if (opposingUnit.Squared)
            {
                opposingUnit.gameManager.SquareCheck(opposingUnit, behindUnit);
            }
        }
        catch (Exception)
        {
            
        }

        //if (opposingUnit.Squared)
        //Did I lose to infantry in square formation (may be null)
        //Is there more cavalry behind me?

    }
}
