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

    public static void Load(List<Unit> unitRoster)
    {
        string strJsonUnitDetails = File.ReadAllText($"{ Directory.GetCurrentDirectory()}\\UnitDetails.json");

        var unitDetails = JsonUtility.FromJson<UnitDetail>(strJsonUnitDetails);
        //var ag = unitRoster.Find(i => i.GetSpriteName() == unitDetails.SpriteName);
        //Object.Instantiate(ag);
    }
}
