using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrenadierAttack : MonoBehaviour
{
    public Unit grenadierPrefab;
    public Unit unit;

    private void Start()
    {
        unit = GetComponent<Unit>();
    }

    private void OnDestroy()
    {
        // This method will be called when the GameObject or script is destroyed
        Debug.Log($"{gameObject.name} destroyed");
    }
    public void DeployGrenadiers(string side)
    {
        Debug.Log("deploying grenadiers...");
        if (grenadierPrefab != null)
        {

        }
    }

    private void OnEnable()
    {
        transform.Find("grenadier_Badge").gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        transform.Find("grenadier_Badge").gameObject.SetActive(false);
    }

}