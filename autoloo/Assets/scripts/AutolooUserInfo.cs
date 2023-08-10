using Auth0.AuthenticationApi.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class must inherit Monobehaviour so that it can be preserved between scenes as a Unity gameObject using dontDestroyOnLoad()
public class AutolooUserInfo : MonoBehaviour
{
    public UserInfo UserInfo { get; set; }
    public string PlayerName;

    private void Awake()
    {
        UserInfo = new UserInfo();
    }
}
