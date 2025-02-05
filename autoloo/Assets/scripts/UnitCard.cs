using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UnitCard : MonoBehaviour
{
    public Unit unit;
    private GameObject goUnitCard;
    private void Start()
    {
        try
        {
            unit = GetComponent<Unit>();
        }
        catch (Exception)
        {
            Debug.Log("this may be in the main menu");
        }
    }

    private void OnMouseEnter()
    {
        if ((unit != null && unit.side == "left" && !unit.gameManager.InBattleModeAndNotDeploymentMode )
            || SceneManager.GetActiveScene().name == "mainmenu")
        {
            transform.Find("unit_card").gameObject.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if ((unit != null && unit.side == "left")
            || SceneManager.GetActiveScene().name == "mainmenu")
        {
            transform.Find("unit_card").gameObject.SetActive(false);
        }
    }
}
