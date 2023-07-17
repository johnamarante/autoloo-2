using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class StoreAndLoadArmyDetails
{
    // Start is called before the first frame update
    public static void Store(List<Unit> army)
    {
        string strJsonUnitDetails = "[";
        foreach (var unit in army)
        {
            strJsonUnitDetails += JsonUtility.ToJson(unit.GetDetail()) + "~";
        }
        strJsonUnitDetails = strJsonUnitDetails.Remove(strJsonUnitDetails.Length - 1, 1) + "]";
        File.WriteAllText($"{ Directory.GetCurrentDirectory()}\\UnitDetails.json", strJsonUnitDetails);
    }

    public static void Load(List<Unit> unitRoster, GameManager gameManager)
    {
        string strJsonUnitDetails = File.ReadAllText($"{ Directory.GetCurrentDirectory()}\\UnitDetails.json").Replace("[","").Replace("]","");
        foreach (var detail in strJsonUnitDetails.Split("~"))
        {
            try
            {
                GenerateReloadedUnitFromDetail(unitRoster, gameManager, detail);
            }
            catch (Exception ex)
            {
                Debug.Log($"an exception occured on the JSON {detail} with the following exception");
                Debug.Log(ex.ToString());
            }
        }

        //var ag = unitRoster.Find(i => i.GetSpriteName() == unitDetails.SpriteName);
        //Object.Instantiate(ag);
    }

    private static void GenerateReloadedUnitFromDetail(List<Unit> unitRoster, GameManager gameManager, string detail)
    {
        var unitDetails = JsonUtility.FromJson<UnitDetail>(detail);
        var rosterItem = unitRoster.Where(x => x.GetSpriteName().Split("_")[1] == unitDetails.SpriteName.Split("_")[1]).First();
        var unit = UnityEngine.Object.Instantiate(rosterItem);
        unit.name = unitDetails.Name;
        unit._attack = unitDetails.Attack;
        unit._hitPoints = unitDetails.HitPoints;
        unit._rank = unitDetails.Rank;
        unit.side = unitDetails.Side;
        unit.QueuePosition = unitDetails.QueuePosition;
        unit._deployed = true;
        unit.gameManager = gameManager;
        unit.DeployAndSnapPositionToDeploymentMarker(UnityEngine.Object.FindObjectsOfType<DeploymentMarker>().Where(x => x.positionKey == unit.QueuePosition).First());
    }
}
