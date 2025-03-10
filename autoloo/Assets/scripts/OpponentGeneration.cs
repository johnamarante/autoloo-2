using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Unity.VisualScripting;

public static class OpponentGeneration
{
    public static void GenerateRandom()
    {
        var gameManager = (GameManager)UnityEngine.Object.FindObjectOfType(typeof(GameManager));
        var rosterSize = gameManager.RightUnitRoster.Count;
        System.Random rnd = new System.Random();
        for (int i = 0; i < 5; i++)
        {
            var goOpposingUnit = UnityEngine.Object.Instantiate(gameManager.RightUnitRoster[rnd.Next(rosterSize)]);
            goOpposingUnit.GetComponent<Unit>().Deployed = true;
        }
    }

    public static async Task<JToken> GetDraftDataAsync(string name, int round, int wins, int losses)
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
            if (Int32.Parse(dd["round"].ToString()) == round)
                //&& dd["playername"].ToString() != name) TODO: this may work better as a setting called "allow me to play against my previous rosters"
                filteredDraftData.Add(dd);
        }

        //return filteredDraftData;
        System.Random rnd = new System.Random();
        int randomNumber = rnd.Next(draftData.Count);
        var randomfdd = draftData[randomNumber];
        return randomfdd;
    }

    public static void GenerateFromDraftData(GameManager gameManager, JToken draftData, out string playerName)
    {
        try
        {
            playerName = draftData["playername"].ToString();
            foreach (var jsonUnitDetail in draftData["UnitDetails"])
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
            Debug.Log($"There was an exception {ex} generating the opponent from the friendpaste data");
            Debug.Log("Reverting to random opponent generation");
            playerName = Guid.NewGuid().ToString().Split("-").First();
            GenerateRandom();
        }
        finally
        {

            new GameObject("OpponentGenerationCompleted");
        }
    }

    private static void InstantiateUnit(GameManager gameManager, JToken jsonUnitDetail)
    {
        try
        {
            var unitName = (string)jsonUnitDetail["Name"];
            var goOpposingUnit = UnityEngine.Object.Instantiate(gameManager.RightUnitRoster.Find(x => x.GetSpriteName().Split('_')[1] == unitName.Split('_')[1]));
            goOpposingUnit.GetComponent<Unit>().Deployed = true;
            goOpposingUnit._attack = (int)jsonUnitDetail["Attack"]; ;
            goOpposingUnit._rank = (int)jsonUnitDetail["Rank"];
            goOpposingUnit._hitPoints = (int)jsonUnitDetail["HitPoints"];
            goOpposingUnit._queuePosition = (int)jsonUnitDetail["QueuePosition"];
            goOpposingUnit.name = unitName;
            goOpposingUnit.AddComponent<BlinkEffect>();
        } 
        catch (Exception ex) 
        { 
            Debug.Log(ex.Message); 
        }
    }
}
