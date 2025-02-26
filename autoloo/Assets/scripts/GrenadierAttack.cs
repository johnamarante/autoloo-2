using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GrenadierAttack : MonoBehaviour
{
    public Unit grenadierPrefab;
    public Unit unit;

    private void Start()
    {
        unit = GetComponent<Unit>();
    }

    public void DeployGrenadiers()
    {
        Debug.Log("deploying grenadiers...");
        if (grenadierPrefab != null)
        {
            grenadierPrefab.side = unit.side;
            grenadierPrefab._hitPoints = grenadierPrefab.ComputeHitPointsFromFoumulaString(unit.Rank);
            grenadierPrefab._attack = grenadierPrefab.ComputeAttackFromFoumulaString(unit.Rank);
            var goUnitGrenadier = Instantiate(grenadierPrefab);
            goUnitGrenadier.Deployed = true;
            goUnitGrenadier.transform.position = unit.transform.position;
            if (unit.side == "left") {
                goUnitGrenadier.QueuePosition = -1;
                unit.gameManager.LeftQueueUnits.Add(goUnitGrenadier);
                unit.gameManager.LeftQueueUnits = unit.gameManager.LeftQueueUnits.OrderByDescending(u => u.QueuePosition).ToList();
            }
            else{
                goUnitGrenadier.QueuePosition = 1;
                unit.gameManager.RightQueueUnits.Add(goUnitGrenadier);
                unit.gameManager.RightQueueUnits = unit.gameManager.RightQueueUnits.OrderBy(u => u.QueuePosition).ToList();
            }
            goUnitGrenadier.AddComponent<BlinkEffect>();
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