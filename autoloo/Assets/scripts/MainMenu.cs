using Auth0.AuthenticationApi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private static string menuMessage = "";
    public static UserInfo autolooUserInfo;
    public string userId = "";

    // Start is called before the first frame update
    void Start()
    {
        autolooUserInfo = FindObjectOfType<AutolooUserInfo>().gameObject.GetComponent<AutolooUserInfo>() .userInfo;
        userId = autolooUserInfo.UserId.Split("|")[1];
        menuMessage = $"logged in as {autolooUserInfo.Email}, id {userId}";
        
        //check if the user is already in the directoy
        //the variable should be readin JSON
        var directoryData = FriendpasteClient.GetDataAsync(AutolooUserInfoUtil.GetUserDirectoryURLBasedOnUserID(userId));

        //if they are, then Get
        //if they are not, do new user setup, which is
           //POST, and get the URL of that post, then
           //PUT the URL of the POST that was just made into the right directory, mapped to the userID

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        int fontSize = 48;
        GUI.Label(new Rect(10, Screen.height - 60, 30, 1000), menuMessage, new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = fontSize });
    }
}
