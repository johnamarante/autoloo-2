using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class StoreAndLoadArmyDetails
{
    // Start is called before the first frame update
    public static void Store(List<Unit> army)
    {
        var jsonStringArmyDetails = "";
        foreach (var unit in army)
        {
            jsonStringArmyDetails += JsonUtility.ToJson(unit);
        }
        //jsonStringArmyDetails = jsonStringArmyDetails.Remove(jsonStringArmyDetails.Length - 1, 1);
        Debug.Log(jsonStringArmyDetails);
        JsonUtility.FromJson<Unit>(jsonStringArmyDetails);
        File.WriteAllText($"{ Directory.GetCurrentDirectory()}\\mah_shit.json", jsonStringArmyDetails);
    }

    public static void Load()
    {
        string jsonStringArmyDetails = File.ReadAllText($"{ Directory.GetCurrentDirectory()}\\mah_shit.json");
        var unit = JsonUtility.FromJson<Unit>(jsonStringArmyDetails);
        Debug.Log(unit);
    }
}
