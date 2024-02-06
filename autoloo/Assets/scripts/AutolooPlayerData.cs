using Auth0.AuthenticationApi.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class must inherit Monobehaviour so that it can be preserved between scenes as a Unity gameObject using dontDestroyOnLoad()
public class AutolooPlayerData : MonoBehaviour
{
    public UserInfo Auth0UserInfo { get; set; }
    public List<UnitDetail> unitDetails { get; set; }
    public string PlayerName;
    public string RosterName;
    public string PlayerData;
    public string PlayerDataPasteURL;
    public int turnNumber;

    private void Awake()
    {
        Auth0UserInfo = new UserInfo();
        ClearUnitDetails();
    }
    private void Start()
    {
        
    }
    private void Update()
    {

    }
    public void ClearUnitDetails()
    {
        unitDetails = new List<UnitDetail>();
    }
    public List<UnitDetail> GetUnitDetailsWithAbsoluteQueuePositionsForStorage()
    {
        List<UnitDetail> UnitDetailsWithAbsoluteQueuePositions;
        if (unitDetails == null)
        {
            UnitDetailsWithAbsoluteQueuePositions = new List<UnitDetail>();
        }
        else
        {
            UnitDetailsWithAbsoluteQueuePositions = new List<UnitDetail>(unitDetails.Count);
            foreach (var unitDetail in unitDetails)
            {
                var newUnitDetail = new UnitDetail
                {
                    Name = unitDetail.Name,
                    Attack = unitDetail.Attack,
                    HitPoints = unitDetail.HitPoints,
                    Rank = unitDetail.Rank,
                    QueuePosition = Mathf.Abs(unitDetail.QueuePosition)
                };
                UnitDetailsWithAbsoluteQueuePositions.Add(newUnitDetail);
            }
        }
        return UnitDetailsWithAbsoluteQueuePositions;
    }
}
