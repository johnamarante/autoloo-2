using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnitPrice : MonoBehaviour
{
    public Unit unit;
    private GameObject goUnitPrice;
    // Start is called before the first frame update

    void Start()
    {
        unit = GetComponent<Unit>();
        goUnitPrice = GameObject.Find("unit_price").transform.GetComponentInChildren<TextMeshPro>(true).gameObject;
    }
    private void OnMouseEnter()
    {
        if (unit != null && unit.side == "left" && !unit.gameManager.InBattleModeAndNotDeploymentMode)
        {
            goUnitPrice.gameObject.SetActive(true);
            if (!unit.Deployed)
            {
                goUnitPrice.GetComponent<TextMeshPro>().color = Color.white;
                goUnitPrice.GetComponent<TextMeshPro>().text = $"Cost:  {unit.Cost}";
                goUnitPrice.gameObject.transform.parent.gameObject.transform.position = new Vector3(-133.5f, 3f, -10.22048f);
            }
            else
            {
                goUnitPrice.GetComponent<TextMeshPro>().color = Color.green;
                goUnitPrice.GetComponent<TextMeshPro>().text = $"Sell:  {1 + unit.CurrencyBumpBasedOnRank(unit.Rank)}";
                goUnitPrice.gameObject.transform.parent.gameObject.transform.position = new Vector3(-133.5f, 25f, -10.22048f);
            }
            
        }
    }

    private void OnMouseExit()
    {
        if (unit != null)
        {
            goUnitPrice.gameObject.SetActive(false);
        }
    }
}
