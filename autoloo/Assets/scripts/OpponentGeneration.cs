using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

public static class OpponentGeneration
{
    public static void GenerateRandom()
    {
        var gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
        var rosterSize = gameManager.RightUnitRoster.Count;
        System.Random rnd = new System.Random();
        for (int i = 0; i < 5; i++)
        {
            var opposingUnit = UnityEngine.Object.Instantiate(gameManager.RightUnitRoster[rnd.Next(rosterSize)]);
            opposingUnit.GetComponent<Unit>().Deployed = true;
        }
    }

    public static async void GenerateFromDraftAsync(string name, int round, int wins, int losses)
    {
        try
        {
            var gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
            var rosterName = gameManager.RightUnitRoster[0].name.Split('_')[0];
            var url = StoreAndLoadArmyDetails.GetAutolooArmyDraftURL(rosterName, round);
            var rawData = await FriendpasteClient.FriendpasteClient.GetDataAsync(url);

            var outerObject = JObject.Parse(rawData);
            var snippetString = FriendpasteClient.FriendpasteClient.PrepareFriendPasteSnippetForCSharpJSONParse(outerObject["snippet"].ToString());
            var snippet = JObject.Parse(snippetString);
            var draftData = (JArray)snippet["data"];
            var filteredDraftData = new JArray();
            foreach (var dd in draftData)
            {
                if (Int32.Parse(dd["round"].ToString()) == round && dd["playername"].ToString() != name)
                    filteredDraftData.Add(dd);
            }

            System.Random rnd = new System.Random();
            int randomNumber = rnd.Next(filteredDraftData.Count);
            var randomfdd = filteredDraftData[randomNumber];

            foreach (var jsonUnitDetail in randomfdd["UnitDetails"])
            {
                try
                {
                    InstantiateUnit(gameManager, jsonUnitDetail);
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"there was an exception {ex} generating the opponet from the friendpaste data");
            Debug.Log("reverting to random opponent generation");
            GenerateRandom();
        }
        finally
        {
            new GameObject("OpponentGenerationCompleted");
        }
    }

    private static void InstantiateUnit(GameManager gameManager, JToken jsonUnitDetail)
    {
        var unitName = (string)jsonUnitDetail["Name"];
        var opposingUnit = UnityEngine.Object.Instantiate(gameManager.RightUnitRoster.Find(x => x.GetSpriteName().Split('_')[1] == unitName.Split('_')[1]));
        opposingUnit.GetComponent<Unit>().Deployed = true;
        opposingUnit._attack = (int)jsonUnitDetail["Attack"]; ;
        opposingUnit._rank = (int)jsonUnitDetail["Rank"];
        opposingUnit._hitPoints = (int)jsonUnitDetail["HitPoints"];
        opposingUnit._queuePosition = (int)jsonUnitDetail["QueuePosition"];
        opposingUnit.name = unitName;
        try { opposingUnit._skirmishMode = (bool)jsonUnitDetail["SkirmishMode"]; } catch (Exception ex) { Debug.Log(ex.Message); }
    }
}
