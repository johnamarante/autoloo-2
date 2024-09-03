using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DeployFootDragoons : MonoBehaviour
{
    public Unit unit;
    public Unit footDragoonPrefab;

    void Start()
    {
        unit = GetComponent<Unit>();
    }

    public void Dismount()
    {
        Debug.Log("deploying Foot Dragoon...");

        if (footDragoonPrefab != null)
        {
            footDragoonPrefab.side = unit.side;
            footDragoonPrefab._hitPoints = footDragoonPrefab.ComputeHitPointsFromFoumulaString(unit.Rank);
            footDragoonPrefab._attack = footDragoonPrefab.ComputeAttackFromFoumulaString(unit.Rank);
            var goFootDragoon = Instantiate(footDragoonPrefab);
            goFootDragoon.Deployed = true;
            goFootDragoon.transform.position = unit.transform.position;
            if (unit.side == "left")
            {
                goFootDragoon.QueuePosition = -1;
                unit.gameManager.LeftQueueUnits.Add(goFootDragoon);
                unit.gameManager.LeftQueueUnits = unit.gameManager.LeftQueueUnits.OrderByDescending(u => u.QueuePosition).ToList();
            }
            else
            {
                goFootDragoon.QueuePosition = 1;
                unit.gameManager.RightQueueUnits.Add(goFootDragoon);
                unit.gameManager.RightQueueUnits = unit.gameManager.RightQueueUnits.OrderBy(u => u.QueuePosition).ToList();
            }
        }
    }
}
