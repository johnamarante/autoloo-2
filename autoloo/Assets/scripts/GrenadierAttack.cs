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
            var goUnitGrenadier = Instantiate(grenadierPrefab);
            goUnitGrenadier.Deployed = true;
            goUnitGrenadier.transform.position = unit.transform.position;
            if (side == "left") {
                goUnitGrenadier.QueuePosition = -1;
                unit.gameManager.LeftQueueUnits.Add(goUnitGrenadier);
                unit.gameManager.LeftQueueUnits = unit.gameManager.LeftQueueUnits.OrderByDescending(u => u.QueuePosition).ToList();
            }
            else{
                goUnitGrenadier.QueuePosition = 1;
                unit.gameManager.RightQueueUnits.Add(goUnitGrenadier);
                unit.gameManager.RightQueueUnits = unit.gameManager.RightQueueUnits.OrderBy(u => u.QueuePosition).ToList();
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