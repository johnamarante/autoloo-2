using UnityEngine;
using UnityEngine.UI;

public class UnitCard : MonoBehaviour
{
    public Unit unit;
    private GameObject goUnitCard;
    private void Start()
    {
        unit = GetComponent<Unit>();
    }

    private void OnMouseEnter()
    {
        if (unit.side == "left" && !unit.gameManager.InBattleModeAndNotDeploymentMode)
        {
            transform.Find("unit_card").gameObject.SetActive(true);

        }
        //only works if unit.side == left
        //find the unit card child object
        //set the unit card .gameObject.SetActive(true);
        //set the position of the card to the invisible marker on the deploy screen
    }

    private void OnMouseExit()
    {
        if (unit.side == "left")
        {
            transform.Find("unit_card").gameObject.SetActive(true);

        }
    }
}
