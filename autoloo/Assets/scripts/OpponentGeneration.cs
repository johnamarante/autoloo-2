using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OpponentGeneration
{
    public static void Generate()
    {
        //need to clear off any existing opposition
        var gameManager = (GameManager)Object.FindObjectOfType(typeof(GameManager));
        var rosterSize = gameManager.RightUnitRoster.Count;
        System.Random rnd = new System.Random();
        for (int i = 0; i < 5; i++)
        {
            Object.Instantiate(gameManager.RightUnitRoster[rnd.Next(rosterSize)]);
        }
    }
}
