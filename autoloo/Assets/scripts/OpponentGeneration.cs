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

            var armyDraft = new List<UnitDetail>();
            foreach (var draft in draftData)
            {
                if (Int32.Parse(draft["round"].ToString()) == round && draft["playername"].ToString() != name)
                {
                    foreach (var jsonUnitDetail in draft["UnitDetails"])
                    {
                        try
                        {
                            var unitName = (string)jsonUnitDetail["Name"];
                            var opposingUnit = UnityEngine.Object.Instantiate(gameManager.RightUnitRoster.Find(x => x.GetSpriteName().Split('_')[1] == unitName.Split('_')[1]));
                            opposingUnit.GetComponent<Unit>().Deployed = true;
                            opposingUnit._attack = (int)jsonUnitDetail["Attack"]; ;
                            opposingUnit._rank = (int)jsonUnitDetail["Rank"];
                            opposingUnit._hitPoints = (int)jsonUnitDetail["HitPoints"];
                            opposingUnit._queuePosition = (int)jsonUnitDetail["QueuePosition"];
                            opposingUnit.name = unitName;
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex.ToString());
                        }
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"there was an exception {ex.ToString()} generating the opponet from the friendpaste data");
            Debug.Log("reverting to random opponent generation");
            GenerateRandom();
        }
        finally 
        {
            GameObject hupuObject = new GameObject("hupu");
        }
    }
}
