using Auth0.AuthenticationApi.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private static string menuMessage = "";
    public static UserInfo autolooUserInfo;
    // Start is called before the first frame update
    void Start()
    {
        autolooUserInfo = FindObjectOfType<AutolooUserInfo>().gameObject.GetComponent<AutolooUserInfo>().userInfo;
        menuMessage = $"logged in as {autolooUserInfo.Email}";
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
