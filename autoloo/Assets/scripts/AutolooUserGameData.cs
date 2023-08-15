using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AutolooUserGameData : MonoBehaviour
{
    public List<UnitDetail> unitDetails { get; set; }
    public string PlayerRoster;

    private void Awake()
    {
        unitDetails = new List<UnitDetail>();
    }
}

