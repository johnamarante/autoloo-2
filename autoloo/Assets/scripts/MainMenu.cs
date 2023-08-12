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
    public string directoryUrl = "";
    public string directoryData = "";
    public string directoryTitle = "";
    public string userId = "";
    public string playerName = "";
    public string playerData = "";
    private bool newUserSetup = false;
    private GUIStyle guiStyle;

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
        directoryUrl = AutolooUserInfoUtil.GetUserDirectoryURLBasedOnUserID(userId);
        directoryData = await FriendpasteClient.FriendpasteClient.GetDataAsync(directoryUrl);

        //check if the user is already in the directoy
        var pasteURL = GetPasteIdByUserId(directoryData, userId);
        //if the user is already present in the directory are, then GET the user data
        if (pasteURL is not null)
        {
            var playerData = await FriendpasteClient.FriendpasteClient.GetDataAsync(pasteURL);
            // Deserialize the main JSON object
            JObject mainObject = JObject.Parse(playerData);

            // Get the "snippet" property as a JSON string
            string snippetJsonString = (string)mainObject["snippet"];

            // Deserialize the inner JSON object within the "snippet" property
            JObject snippetObject = JObject.Parse( FriendpasteClient.FriendpasteClient.PrepareFriendPasteSnippetForCSharpJSONParse( snippetJsonString));

            // Now you can access properties within the inner JSON object
            autolooUserInfo.PlayerName = (string)snippetObject["playername"];
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
            var newPlayerPostResponse = await FriendpasteClient.FriendpasteClient.PostDataAsync(FriendpasteClient.FriendpasteClient.baseURL, userId, FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(jsonString));

            var updatedJSON = AddUserIdAndPasteIdToDirectoryData(directoryData, userId, JObject.Parse(newPlayerPostResponse).Value<string>("url"));
            char firstChar = char.ToLower(userId[0]);
            await FriendpasteClient.FriendpasteClient.PutDataAsyncWithTimeout(directoryUrl, $"AutolooUserDirectory{firstChar}" , FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(updatedJSON));

            Debug.Log($"Player name set to {playerName} and paste is at {newPlayerPostResponse}");
            newUserSetup = false;
        }
    }

    static string GetPasteIdByUserId(string jsonString, string userId)
    {
        JObject outerObject = JObject.Parse(jsonString);
        JObject innerObject = JObject.Parse(FriendpasteClient.FriendpasteClient.PrepareFriendPasteSnippetForCSharpJSONParse(outerObject["snippet"].ToString()));
        JArray usersArray = (JArray)innerObject["data"];

        foreach (JObject user in usersArray)
        {
            string currentUserId = user["userId"].ToString();
            if (currentUserId == userId)
            {
                return user["pasteid"].ToString();
            }
        }
        return null; // Return null if userId is not found
    }

    static string AddUserIdAndPasteIdToDirectoryData(string jsonString, string userId, string pasteId)
    {
        var newElement = new JObject
        {
            ["userId"] = userId,
            ["pasteid"] = pasteId
        };
        JObject anotherJsonObject = JObject.Parse(FriendpasteClient.FriendpasteClient.PrepareFriendPasteSnippetForCSharpJSONParse(JObject.Parse(jsonString)["snippet"].ToString()));
        anotherJsonObject["data"][0].AddBeforeSelf(newElement);
        string serializedAnotherJson = JsonConvert.SerializeObject(anotherJsonObject, Formatting.Indented);
        return serializedAnotherJson;
    }

}
