using Auth0.AuthenticationApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private static string menuMessage = "";
    public static AutolooUserInfo autolooUserInfo;
    private string userId = "";
    private bool newUserSetup = false;
    private GUIStyle guiStyle;

    [Header("Directory")]
    public string directoryUrl = "";
    private string directoryData = "";

    [Header("Player")]
    public string playerName = "";
    private string playerData = "";

    private void Awake()
    {
        guiStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = 48 };
    }

    private async void Start()
    {
        autolooUserInfo = FindObjectOfType<AutolooUserInfo>();
        if (autolooUserInfo.UserInfo.UserId == "guest")
        {
            userId = autolooUserInfo.UserInfo.UserId;
            newUserSetup = false;
        }
        else
        {
            userId = autolooUserInfo.UserInfo.UserId.Split("|")[1];

            directoryUrl = AutolooUserInfoUtil.GetUserDirectoryURLBasedOnUserID(userId);
            directoryData = await FriendpasteClient.FriendpasteClient.GetDataAsync(directoryUrl);

            var pasteURL = GetPasteIdByUserId(directoryData, userId);
            if (!string.IsNullOrEmpty(pasteURL))
            {
                var playerData = await FriendpasteClient.FriendpasteClient.GetDataAsync(pasteURL);
                autolooUserInfo.PlayerName = ExtractPlayerNameFromData(playerData);
                //load other returning player data (current game turn number)
            }
            else
            {
                playerName = autolooUserInfo.UserInfo.Email.Split("@")[0];
                newUserSetup = true;
            }

            menuMessage = $"Logged in as {GetPlayerDisplayName()}";
        }
    }

    private string GetPlayerDisplayName()
    {
        return String.IsNullOrEmpty(autolooUserInfo.PlayerName) ? autolooUserInfo.UserInfo.Email : autolooUserInfo.PlayerName;
    }

    private async void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 60, 30, 1000), menuMessage, guiStyle);

        if (newUserSetup)
        {
            await ShowNewUserSetupDialogue();
        }
        else
        {
            //if this is a returning user
        }
    }

    private async Task ShowNewUserSetupDialogue()
    {
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        float inputWidth = 500;
        float inputHeight = 60;
        float buttonWidth = 500;
        float buttonHeight = 60;

        float inputX = centerX - inputWidth * 0.5f;
        float inputY = centerY - inputHeight * 0.5f;
        float buttonX = centerX - buttonWidth * 0.5f;
        float buttonY = inputY + inputHeight + 20;

        playerName = GUI.TextField(new Rect(inputX, inputY, inputWidth, inputHeight), playerName);

        if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Set Name"))
        {
            await SetupNewUser();
        }
    }

    private async Task SetupNewUser()
    {
        JObject jsonObject = new JObject(
            new JProperty("playername", playerName),
            new JProperty("UnitDetails", new JArray())
        );

        string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        var newPlayerPostResponse = await FriendpasteClient.FriendpasteClient.PostDataAsync(FriendpasteClient.FriendpasteClient.BaseUrl, userId, FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(jsonString));

        var updatedJSON = AddUserIdAndPasteIdToDirectoryData(directoryData, userId, JObject.Parse(newPlayerPostResponse).Value<string>("url"));
        char firstChar = char.ToLower(userId[0]);
        await FriendpasteClient.FriendpasteClient.PutDataAsyncWithTimeout(directoryUrl, $"AutolooUserDirectory{firstChar}", FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(updatedJSON));

        Debug.Log($"Player name set to {playerName} and paste is at {newPlayerPostResponse}");
        newUserSetup = false;
    }

    private string ExtractPlayerNameFromData(string playerData)
    {
        JObject mainObject = JObject.Parse(playerData);
        string snippetJsonString = (string)mainObject["snippet"];
        JObject snippetObject = JObject.Parse(FriendpasteClient.FriendpasteClient.PrepareFriendPasteSnippetForCSharpJSONParse(snippetJsonString));
        return (string)snippetObject["playername"];
    }

    private string GetPasteIdByUserId(string jsonString, string userId)
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
        return null;
    }

    private string AddUserIdAndPasteIdToDirectoryData(string jsonString, string userId, string pasteId)
    {
        var newElement = new JObject
        {
            ["userId"] = userId,
            ["pasteid"] = pasteId
        };
        JObject anotherJsonObject = JObject.Parse(FriendpasteClient.FriendpasteClient.PrepareFriendPasteSnippetForCSharpJSONParse(JObject.Parse(jsonString)["snippet"].ToString()));
        anotherJsonObject["data"][0].AddBeforeSelf(newElement);
        return JsonConvert.SerializeObject(anotherJsonObject, Formatting.Indented);
    }
}
