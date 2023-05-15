using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeploymentMarker : MonoBehaviour
{
    public int positionKey;
    public string side;
    public Deployment deployment;
    public Unit _occupant;
    public Unit occupant
    {
        get { return _occupant; }
        set
        {
            _occupant = value;
            this?.OnOccupantChanged(_occupant);
        }
    }
    public Action<Unit> OnOccupantChanged;
    public GameObject goArrow;
    public GameObject goCombine;

    // Start is called before the first frame update
    void Start()
    {
        OnOccupantChanged += (f) => ShowHoverIndicator(false);
        if (positionKey < 0)
        {
            side = "left";
        }
        else 
        {
            side = "right";
        }
        goArrow = this.gameObject.transform.Find("arrow").gameObject;
        goCombine = this.gameObject.transform.Find("combine").gameObject;
    }

    private void OnMouseEnter()
    {
        ShowHoverIndicator(true);
    }

    private void OnMouseExit()
    {
        ShowHoverIndicator(false);
    }

    public void ShowHoverIndicator(bool show)
    {
        //deploymarkers only show mouse hover indicators when unoccupied
        //but is that ^ right? // && (occupant == null)
        var activeValue = (show);
        transform.Find("hover_over_indicator").gameObject.SetActive(activeValue);
    }

    //Click and Click deployment Part 2
    //... click.
    private void OnMouseDown()
    {
        if (deployment.gameManager.selectedUnit != null)
        {
            var selectedUnit = deployment.gameManager.selectedUnit;
            if (selectedUnit != null && (selectedUnit.CanAfford() || selectedUnit.Deployed))
            {
                if (occupant == null)
                {
                    selectedUnit.DeployAndSnapPositionToDeploymentMarker(this);
                }
                else if (deployment.IsThereAnEmptySpaceToShiftTo(positionKey, out int ShiftPositionKey))
                {
                    deployment.ShiftUnits(positionKey, ShiftPositionKey);
                }
                deployment.gameManager.Deselect();
            }
        }
    }
}
