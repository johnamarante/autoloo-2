using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scout : MonoBehaviour
{
    public Unit unit;
    public Sprite questionMark;
    void Start()
    {
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        
    }

    internal void Report()
    {
        Debug.Log("Scout report");
        var report = Camera.main.gameObject.transform.Find("ScoutReport");
        var opponentDraftData = unit.gameManager.deployment.opponentDraftData;
        //for (int i = 0; i < report.childCount; i++)
        //{
        //    var opponentDraftData = unit.gameManager.deployment.opponentDraftData;
        //    Debug.Log(opponentDraftData);
        //    var unitName = (string)opponentDraftData["Name"];
        //    var opposingUnit = UnityEngine.Object.Instantiate(gameManager.RightUnitRoster.Find(x => x.GetSpriteName().Split('_')[1] == unitName.Split('_')[1]));
        //}
        int iterationCount = 0;
        foreach (var unitDetail in opponentDraftData["UnitDetails"])
        {
            iterationCount++;
            if (iterationCount > report.childCount)
            {
                break;
            }

            string unitName = unitDetail["Name"].ToString();
            Debug.Log(unitName);
            var opposingUnit = unit.gameManager.RightUnitRoster.Find(x => x.GetSpriteName().Split('_')[1] == unitName.Split('_')[1]);
            report.Find(iterationCount.ToString()).GetComponent<SpriteRenderer>().sprite = (opposingUnit.Rspritebackground == null ? opposingUnit.Rsprite : opposingUnit.Rspritebackground);
        }
    }
}
