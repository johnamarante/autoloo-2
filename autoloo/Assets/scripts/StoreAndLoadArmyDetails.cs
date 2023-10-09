using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class StoreAndLoadArmyDetails
{
    public static void Store(List<UnitDetail> unitDetails)
    {
        string strJsonUnitDetails = JsonUtility.ToJson(new UnitDetailListWrapper { UnitDetails = unitDetails }, true);
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "UnitDetails.json"), strJsonUnitDetails);
    }

    public static void Store(AutolooPlayerData autolooPlayerData)
    {
        WriteToFriendPaste(autolooPlayerData.PlayerDataPasteURL, autolooPlayerData.Auth0UserInfo.UserId, autolooPlayerData.PlayerName, autolooPlayerData.unitDetails);
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
        var spriteB = unitDetails.SpriteName.Split('_')[1];
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

    private static async void WriteToFriendPaste(string playerPasteUrl, string userId, string playerName, List<UnitDetail> unitDetails)
    {
        try
        {
            JObject jsonObject = new JObject(
                new JProperty("playername", playerName),
                new JProperty("UnitDetails", JArray.FromObject(unitDetails)));

            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            await FriendpasteClient.FriendpasteClient.PutDataAsyncWithTimeout(playerPasteUrl, userId, FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(jsonString));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [Serializable]
    private class UnitDetailListWrapper
    {
        public List<UnitDetail> UnitDetails;
    }
}
