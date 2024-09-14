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
        var report = Camera.main.gameObject.transform.Find("ScoutReport");
        var opponentDraftData = unit.gameManager.deployment.opponentDraftData;
        int iterationCount = 0;
        foreach (var opposingUnitDetail in opponentDraftData["UnitDetails"])
        {
            iterationCount++;
            if (iterationCount > report.childCount)
            {
                break;
            }
            try
            {
                string opposingUnitName = opposingUnitDetail["Name"].ToString();
                int opposingunitRank = (int)opposingUnitDetail["Rank"];
                var opposingUnit = unit.gameManager.RightUnitRoster.Find(x => x.GetSpriteName().Split('_')[1] == opposingUnitName.Split('_')[1]);
                Debug.Log($"found {opposingUnitName} of rank {opposingunitRank}");
                if (opposingunitRank > unit.Rank)
                {
                    report.Find(iterationCount.ToString()).GetComponent<SpriteRenderer>().sprite = questionMark;
                }
                else
                {
                    report.Find(iterationCount.ToString()).GetComponent<SpriteRenderer>().sprite = (opposingUnit.Rspritebackground == null ? opposingUnit.Rsprite : opposingUnit.Rspritebackground);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                report.Find(iterationCount.ToString()).GetComponent<SpriteRenderer>().sprite = questionMark;
            }
        }
    }

    public IEnumerator WaitAndReport()
    {
        // Wait for opponentDraftData to be populated
        while (unit.gameManager.deployment.opponentDraftData == null)
        {
            Debug.Log("Waiting for opponentDraftData for scout report");
            yield return null; // Wait for the next frame and check again
        }

        // opponentDraftData is now populated, run Report()
        Report();
    }

}
