using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrenadierAttack : MonoBehaviour
{
    public Unit grenadierPrefab;
    public Unit unit;

    private void Start()
    {
        unit = GetComponent<Unit>();
    }

    private void OnDestroy()
    {
        // This method will be called when the GameObject or script is destroyed
        Debug.Log($"{gameObject.name} destroyed");
    }
    public void DeployGrenadiers(string side)
    {
        Debug.Log("deploying grenadiers...");
        if (grenadierPrefab != null)
        {
            grenadierPrefab.side = side;
            grenadierPrefab._hitPoints = grenadierPrefab.ComputeHitPointsFromFoumulaString(unit.Rank);
            grenadierPrefab._attack = grenadierPrefab.ComputeAttackFromFoumulaString(unit.Rank);
            var goGrenadier = Instantiate(grenadierPrefab);
            goGrenadier.Deployed = true;
            goGrenadier.transform.position = unit.transform.position;
            if (side == "left") {
                goGrenadier.QueuePosition = -1;
                unit.gameManager.LeftQueueUnits.Add(goGrenadier);
                unit.gameManager.LeftQueueUnits = unit.gameManager.LeftQueueUnits.OrderByDescending(u => u.QueuePosition).ToList();
                //unit.gameManager.ArrangeUnitsOnBattlefield(ref unit.gameManager.LeftQueueUnits, unit.gameManager.fightQueuePositions);
            }
            else{
                goGrenadier.QueuePosition = 1;
                unit.gameManager.RightQueueUnits.Add(goGrenadier);
                unit.gameManager.RightQueueUnits = unit.gameManager.RightQueueUnits.OrderByDescending(u => u.QueuePosition).ToList();
                //unit.gameManager.ArrangeUnitsOnBattlefield(ref unit.gameManager.RightQueueUnits, unit.gameManager.fightQueuePositions);
            }
        }
    }

    private void OnEnable()
    {
        transform.Find("grenadier_Badge").gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        transform.Find("grenadier_Badge").gameObject.SetActive(false);
    }

}