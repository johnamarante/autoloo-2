using Auth0.AuthenticationApi.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutolooUserInfo : MonoBehaviour
{
    public UserInfo userInfo { get; set; }

    private void Awake()
    {
        userInfo = new UserInfo();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
