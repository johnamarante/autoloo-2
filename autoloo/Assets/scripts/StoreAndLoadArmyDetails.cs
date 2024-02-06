using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class StoreAndLoadArmyDetails
{
    public static void Store(AutolooPlayerData autolooPlayerData, int round, int wins, int losses)
    {
        string strJsonUnitDetails = JsonUtility.ToJson(new UnitDetailListWrapper { UnitDetails = autolooPlayerData.unitDetails }, true);
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "UnitDetails.json"), strJsonUnitDetails);
        if (autolooPlayerData.PlayerName != "guest")
        {
            WriteToPlayerPaste(autolooPlayerData.PlayerDataPasteURL, autolooPlayerData.Auth0UserInfo.UserId, autolooPlayerData.PlayerName, autolooPlayerData.GetUnitDetailsWithAbsoluteQueuePositionsForStorage(), round, wins, losses);
            UpdateArmyDraftsByRosterAndTurn(autolooPlayerData.RosterName, autolooPlayerData.Auth0UserInfo.UserId, autolooPlayerData.PlayerName, autolooPlayerData.GetUnitDetailsWithAbsoluteQueuePositionsForStorage(), round, wins, losses);
        }
    }

    public static void Load(List<Unit> unitRoster, GameManager gameManager)
    {
        string strJsonUnitDetails = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "UnitDetails.json"));
        UnitDetailListWrapper wrapper = JsonUtility.FromJson<UnitDetailListWrapper>(strJsonUnitDetails);

        foreach (var detail in wrapper.UnitDetails)
        {
            try
            {
                GenerateReloadedUnitFromDetail(unitRoster, gameManager, detail, "left");
            }
            catch (Exception ex)
            {
                Debug.Log($"An exception occurred on the JSON: {detail} with the following exception");
                Debug.Log(ex.ToString());
            }
        }
    }

    public static void GenerateReloadedUnitFromDetail(List<Unit> unitRoster, GameManager gameManager, UnitDetail unitDetails, string side)
    {
        var spriteB = unitDetails.Name.Split('_')[1];
        var rosterItem = unitRoster.Find(x => x.GetSpriteName().Split('_')[1] == unitDetails.Name.Split('_')[1]);

        if (rosterItem != null)
        {
            Unit unit = UnityEngine.Object.Instantiate(rosterItem);
            unit.name = unitDetails.Name;
            unit._attack = unitDetails.Attack;
            unit._hitPoints = unitDetails.HitPoints;
            unit._rank = unitDetails.Rank;
            unit.side = side;
            unit.QueuePosition = unitDetails.QueuePosition;
            unit._deployed = true;
            unit.gameManager = gameManager;
            unit.DeployAndSnapPositionToDeploymentMarker(UnityEngine.Object.FindObjectsOfType<DeploymentMarker>().Where(x => x.positionKey == unit.QueuePosition).First());
        }
        else
        {
            Debug.LogWarning($"No roster item found for unit details: {unitDetails.Name}");
        }
    }

    private static async void WriteToPlayerPaste(string playerPasteUrl, string userId, string playerName, List<UnitDetail> unitDetails, int round, int wins, int losses)
    {
        try
        {
            JObject jsonObject = new JObject(
                new JProperty("playername", playerName),
                new JProperty("round", round),
                new JProperty("wins", wins),
                new JProperty("losses", losses),
                new JProperty("UnitDetails", JArray.FromObject(unitDetails)));

            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            await FriendpasteClient.FriendpasteClient.PutDataAsyncWithTimeout(playerPasteUrl, userId, FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(jsonString));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static async void UpdateArmyDraftsByRosterAndTurn(string roster, string userId, string playerName, List<UnitDetail> unitDetails, int round, int wins, int losses)
    {
        try
        {
            var url = GetAutolooArmyDraftURL(roster, round);
            var rawData = await FriendpasteClient.FriendpasteClient.GetDataAsync(url);

            var outerObject = JObject.Parse(rawData);
            var snippetString = FriendpasteClient.FriendpasteClient.PrepareFriendPasteSnippetForCSharpJSONParse(outerObject["snippet"].ToString());
            var snippet = JObject.Parse(snippetString);
            var draftData = (JArray)snippet["data"];

            var newJsonObject = new JObject(
                new JProperty("playername", playerName),
                new JProperty("round", round),
                new JProperty("wins", wins),
                new JProperty("losses", losses),
                new JProperty("UnitDetails", JArray.FromObject(unitDetails)));

            draftData.Add(newJsonObject);

            var jsonString = "{ \"data\" : " + JsonConvert.SerializeObject(draftData, Formatting.Indented) + "}";
            await FriendpasteClient.FriendpasteClient.PutDataAsyncWithTimeout(url, $"Autoloo{roster[0]}{roster[1]}{round}", FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(jsonString));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static string GetAutolooArmyDraftURL(string rosterName, int turnNumber)
    {
        string lowerRosterName = rosterName.ToLower();

        if (lowerRosterName.StartsWith("fr"))
        {
            if (turnNumber == 1)
            {
                return "https://friendpaste.com/7PqkEHCArsKsB1BqfcQ5WY";
            }
            if (turnNumber == 2) 
            {
                return "https://friendpaste.com/5ywZxxsN9T7M46MkVW5lua";
            }
            if (turnNumber >= 3)
            {
                return "https://friendpaste.com/ZTQWDc7yFs5UdBy74Wp3l"; 
            }
        }

        if (lowerRosterName.StartsWith("br"))
        {
            if (turnNumber == 1)
            {
                return "https://friendpaste.com/5ywZxxsN9T7M46MkVVNzDl";
            }
            if (turnNumber == 2)
            {
                return "https://friendpaste.com/5ywZxxsN9T7M46MkVVfbZd";
            }
            if (turnNumber >= 3)
            {
                return "https://friendpaste.com/4tqHc6my3iXt8YCKVUnXqx";
            }
        }

        return "";
    }

    [Serializable]
    private class UnitDetailListWrapper
    {
        public List<UnitDetail> UnitDetails;
    }
}
