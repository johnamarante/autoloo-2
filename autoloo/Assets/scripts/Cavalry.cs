using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavalry : MonoBehaviour
{
    public Unit unit;
    public string spriteNameLower;
    public bool applyWinBuff = false;
    public bool isLancer = false;
    public int minorWinBuffBonus = 1;
    // Constants for cavalry names
    private const string CARABINERACHEVAL = "carabineracheval";
    private const string CUIRASSIER = "cuirassier";
    private const string HUSSAR = "hussar";
    private const string DRAGOON = "dragoon";
    private const string LANCER = "lancer";

    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<Unit>();
        spriteNameLower = unit.spriteName.ToLower();
        if (spriteNameLower.Contains(LANCER))
        {
            isLancer = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (applyWinBuff)
        {
            applyWinBuff = false;
            WinBuff();
        }
    }

    public void WinBuff()
    {
        var rankBonus = unit.Rank + 1;
        if (spriteNameLower.Contains(CARABINERACHEVAL) || spriteNameLower.Contains(CUIRASSIER))
        {
            ApplyBuff(rankBonus, rankBonus);
        }
        else if (spriteNameLower.Contains(HUSSAR) || spriteNameLower.Contains(DRAGOON) || spriteNameLower.Contains(LANCER))
        {
            ApplyBuff(minorWinBuffBonus, minorWinBuffBonus);
        }
    }

    private void ApplyBuff(int hitPointsBonus, int attackBonus)
    {
        unit.HitPoints += hitPointsBonus;
        unit.AttackBonus += attackBonus;
        unit.gameManager.floatyNumber.SpawnFloatingString($"+{hitPointsBonus}/{attackBonus}", Color.green, unit.transform.position);
    }
}
