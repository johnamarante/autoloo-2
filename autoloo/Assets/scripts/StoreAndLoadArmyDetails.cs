using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class StoreAndLoadArmyDetails
{
    public static void Store(List<Unit> army)
    {
        List<UnitDetail> unitDetailsList = new List<UnitDetail>();

        foreach (var unit in army)
        {
            unitDetailsList.Add(unit.GetDetail());
        }

        string strJsonUnitDetails = JsonUtility.ToJson(new UnitDetailListWrapper { UnitDetails = unitDetailsList }, true);
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "UnitDetails.json"), strJsonUnitDetails);
    }

    public static void Load(List<Unit> unitRoster, GameManager gameManager)
    {
        string strJsonUnitDetails = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "UnitDetails.json"));
        UnitDetailListWrapper wrapper = JsonUtility.FromJson<UnitDetailListWrapper>(strJsonUnitDetails);

        foreach (var detail in wrapper.UnitDetails)
        {
            try
            {
                GenerateReloadedUnitFromDetail(unitRoster, gameManager, detail);
            }
            catch (Exception ex)
            {
                Debug.Log($"An exception occurred on the JSON: {detail} with the following exception");
                Debug.Log(ex.ToString());
            }
        }
    }

    private static void GenerateReloadedUnitFromDetail(List<Unit> unitRoster, GameManager gameManager, UnitDetail unitDetails)
    {
        var rosterItem = unitRoster.Find(x => x.GetSpriteName().Split('_')[1] == unitDetails.SpriteName.Split('_')[1]);

        if (rosterItem != null)
        {
            Unit unit = UnityEngine.Object.Instantiate(rosterItem);
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
        else
        {
            Debug.LogWarning($"No roster item found for unit details: {unitDetails.SpriteName}");
        }
    }

    [Serializable]
    private class UnitDetailListWrapper
    {
        public List<UnitDetail> UnitDetails;
    }
}
