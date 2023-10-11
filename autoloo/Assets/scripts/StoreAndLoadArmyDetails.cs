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
            WriteToPlayerPaste(autolooPlayerData.PlayerDataPasteURL, autolooPlayerData.Auth0UserInfo.UserId, autolooPlayerData.PlayerName, autolooPlayerData.unitDetails, round, wins, losses);
            UpdateArmyDraftsByRosterAndTurn(autolooPlayerData.RosterName, 1, autolooPlayerData.Auth0UserInfo.UserId, autolooPlayerData.PlayerName, autolooPlayerData.unitDetails, round, wins, losses);
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
        var spriteB = unitDetails.Name.Split('_')[1];
        var rosterItem = unitRoster.Find(x => x.GetSpriteName().Split('_')[1] == unitDetails.Name.Split('_')[1]);

        if (rosterItem != null)
        {
            Unit unit = UnityEngine.Object.Instantiate(rosterItem);
            unit.name = unitDetails.Name;
            unit._attack = unitDetails.Attack;
            unit._hitPoints = unitDetails.HitPoints;
            unit._rank = unitDetails.Rank;
            unit.side = "left";
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

    private static async void UpdateArmyDraftsByRosterAndTurn(string roster, int turn, string userId, string playerName, List<UnitDetail> unitDetails, int round, int wins, int losses)
    {
        try
        {
            var url = GetAutolooArmyDraftURL(roster, turn);
            var draftData = await FriendpasteClient.FriendpasteClient.GetDataAsync(url);

            JObject outerObject = JObject.Parse(draftData);
            JObject innerObject = JObject.Parse(FriendpasteClient.FriendpasteClient.PrepareFriendPasteSnippetForCSharpJSONParse(outerObject["snippet"].ToString()));
            JArray usersArray = (JArray)innerObject["data"];

            JObject jsonObject = new JObject(
                new JProperty("playername", playerName),
                new JProperty("round", round),
                new JProperty("wins", wins),
                new JProperty("losses", losses),
                new JProperty("UnitDetails", JArray.FromObject(unitDetails)));

            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            await FriendpasteClient.FriendpasteClient.PutDataAsyncWithTimeout(GetAutolooArmyDraftURL(roster, turn), userId, FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(jsonString));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static string GetAutolooArmyDraftURL(string rosterName, int turnNumber)
    {
        string lowerRosterName = rosterName.ToLower();

        if (lowerRosterName.StartsWith("fre"))
        {
            if (turnNumber == 1)
            {
                return "https://friendpaste.com/7PqkEHCArsKsB1BqfcQ5WY"; // AutolooFre1
            }
            if (turnNumber == 2) 
            {
                return "https://friendpaste.com/5ywZxxsN9T7M46MkVW5lua"; // AutolooFre2
            }
            if (turnNumber >= 3)
            {
                return "https://friendpaste.com/ZTQWDc7yFs5UdBy74Wp3l"; // AutolooFre3
            }
        }

        if (lowerRosterName.StartsWith("bri"))
        {
            if (turnNumber == 1)
            {
                return "https://friendpaste.com/4tqHc6my3iXt8YCKVUnXqx"; // AutolooBri3
            }
            if (turnNumber == 2)
            {
                return "https://friendpaste.com/5ywZxxsN9T7M46MkVVfbZd"; // AutolooBri2
            }
            if (turnNumber >= 3)
            {
                return "https://friendpaste.com/4tqHc6my3iXt8YCKVUnXqx"; // AutolooBri3
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
