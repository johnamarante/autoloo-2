using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BolsterMorale : MonoBehaviour
{
    public Unit unit;
    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SameKindDeploymentBonus()
    {
        var listDeployedAllyUnits = FindObjectsOfType<Unit>().Where(y => y.side == "left" && y.Deployed).ToList();
        if (!unit.gameManager.InBattleModeAndNotDeploymentMode
            && listDeployedAllyUnits.Count > 0
            && unit.KindTags.Length > 0)
        {
            string commonTag = null;
            //check if any other deployed units have a common kind tag
            foreach (Unit alliedUnit in listDeployedAllyUnits)
            {
                commonTag = HasCommonTags(alliedUnit);
                if (commonTag != null)
                {
                    Debug.Log($"{this.name} has common tag {commonTag} with {alliedUnit.name}");
                    alliedUnit.HitPoints++;
                    alliedUnit.Attack++;
                    unit.gameManager.floatyNumber.SpawnFloatingString($"+1/1", Color.green, alliedUnit.transform.position);
                }
            }
            if (commonTag != null)
            {
                unit.gameManager.floatyNumber.SpawnFloatingString($"+1/1", Color.green, this.transform.position);
                unit.HitPoints++;
                unit.Attack++;
            }
        }
    }

    private string HasCommonTags(Unit unitB)
    {
        return unit.KindTags.Intersect(unitB.KindTags).FirstOrDefault();
    }
}
