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

    private void Awake()
    {
        Auth0UserInfo = new UserInfo();
        unitDetails = new List<UnitDetail>();
    }
    private void Start()
    {
        
    }
    private void Update()
    {

    }
}
