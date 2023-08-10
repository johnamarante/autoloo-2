using Auth0.AuthenticationApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private static string menuMessage = "";
    public static AutolooUserInfo autolooUserInfo;
    public string userId = "";
    public string userData = "";
    public string playerName = "";
    private bool newUserSetup = false;
    private GUIStyle guiStyle;// = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = 48 };

    private void Awake()
    {
        guiStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = 48 };
    }

    // Start is called before the first frame update
    async void Start()
    { 
        autolooUserInfo = FindObjectOfType<AutolooUserInfo>();
        userId = autolooUserInfo.UserInfo.UserId.Split("|")[1];
        
        //Get the user directory data based on the first char of the userid from Auth0
        var directoryData = await FriendpasteClient.GetDataAsync(AutolooUserInfoUtil.GetUserDirectoryURLBasedOnUserID(userId));

        //check if the user is already in the directoy
        var pasteId = GetPasteIdByUserId(directoryData, userId);
        //if they are, then PUT the user data
        if (pasteId is not null)
        {
            autolooUserInfo.PlayerName = await FriendpasteClient.GetDataAsync($"https://friendpaste.com/{pasteId}");
        }
        else {
            //otherwise, do new user setup, which is
            //POST, and get the URL of that post and
            //PUT the URL of the POST that was just made into the right directory, mapped to the userID
            //make a POST the new user data and PUT to the directory
            playerName = autolooUserInfo.UserInfo.Email.Split("@")[0];
            newUserSetup = true;
        }


        menuMessage = $"logged in as {(String.IsNullOrEmpty(autolooUserInfo.PlayerName) ? autolooUserInfo.UserInfo.Email : autolooUserInfo.PlayerName)}, id {userId}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private async Task OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 60, 30, 1000), menuMessage, guiStyle);

        if (newUserSetup)
        {
            await ShowNewUserSetupDialouge();
        }
    }

    private async Task ShowNewUserSetupDialouge()
    {
        // Calculate the center of the screen
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        // Calculate the dimensions for the input field and button
        float inputWidth = 500;
        float inputHeight = 60;
        float buttonWidth = 500;
        float buttonHeight = 60;

        // Calculate positions for the input field and button
        float inputX = centerX - inputWidth * 0.5f;
        float inputY = centerY - inputHeight * 0.5f;
        float buttonX = centerX - buttonWidth * 0.5f;
        float buttonY = inputY + inputHeight + 20;

        // Draw the input field
        playerName = GUI.TextField(new Rect(inputX, inputY, inputWidth, inputHeight), playerName);

        // Draw the button
        if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Set Name"))
        {
            JObject jsonObject = new JObject(
                new JProperty("playername", playerName),
                new JProperty("UnitDetails", new JArray())
            );

            string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            var newPlayerPostResponse = await FriendpasteClient.PostDataAsync(FriendpasteClient.baseURL, autolooUserInfo.UserInfo.UserId, jsonString);

            // Button click logic (you can implement your own logic here)
            Debug.Log("Player name set to: " + playerName);
            newUserSetup = false;
        }
    }

    static string GetPasteIdByUserId(string jsonString, string userId)
    {
        JObject jsonObject = JObject.Parse(jsonString);
        JArray usersArray = (JArray)jsonObject["users"];

        foreach (JToken user in usersArray)
        {
            string currentUserId = user["userId"].ToString();
            if (currentUserId == userId)
            {
                return user["pasteId"].ToString();
            }
        }

        return null; // Return null if userId is not found
    }

    static string AddUserToPasteSnip(string jsonString, string newUserId, string newPasteId)
    {
        JObject jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);

        JArray usersArray = (JArray)jsonObject["users"];

        JObject newUser = new JObject();
        newUser["userId"] = newUserId;
        newUser["pasteId"] = newPasteId;

        usersArray.Add(newUser);

        return jsonObject.ToString();
    }
}
