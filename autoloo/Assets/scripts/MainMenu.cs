using Auth0.AuthenticationApi.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    
    public Texture2D britainFlag;
    public Texture2D franceFlag;
    private static string menuMessage = "";
    public static AutolooPlayerData autolooPlayerData;
    public ConfigurationManager configManager;
    public Configuration configuration;
    private string userId = "";
    private bool newUserSetup = false;
    private bool isSettingUpUser = false;
    private GUIStyle guiStyle;

    [Header("Directory")]
    public string directoryUrl = "";
    private string directoryData = "";

    [Header("Player")]
    public string playerName = "";
    //private string playerData = "";
    public GameObject mainMenuChooseYourSide, playAsFrance, playAsBritain, hoverToShowDetails;
    public GameObject[] frenchRoster, britishRoster;

    private void Awake()
    {
        guiStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.white }, fontSize = 24 };
    }

    private async void Start()
    {
        GUI.enabled = false;
        autolooPlayerData = FindObjectOfType<AutolooPlayerData>();
        configManager = FindObjectOfType<ConfigurationManager>();
        configuration = configManager.config;
        DontDestroyOnLoad(configManager);

        if (autolooPlayerData.Auth0UserInfo.UserId == "guest")
        {
            userId = autolooPlayerData.Auth0UserInfo.UserId;
            newUserSetup = false;
        }
        else
        {
            userId = autolooPlayerData.Auth0UserInfo.UserId.Split("|")[1];

            directoryUrl = AutolooUserInfoUtil.GetUserDirectoryURLBasedOnUserID(userId);
            directoryData = await FriendpasteClient.FriendpasteClient.GetDataAsync(directoryUrl);

            var pasteURL = GetPasteIdByUserId(directoryData, userId);
            if (!string.IsNullOrEmpty(pasteURL))
            {
                var playerData = await FriendpasteClient.FriendpasteClient.GetDataAsync(pasteURL);
                autolooPlayerData.PlayerName = ExtractPlayerNameFromData(playerData);
                autolooPlayerData.PlayerData = playerData;
                autolooPlayerData.PlayerDataPasteURL = pasteURL;
                //load other returning player data (current game turn number)
            }
            else
            {
                playerName = autolooPlayerData.Auth0UserInfo.Email.Split("@")[0];
                newUserSetup = true;
            }

            menuMessage = $"Logged in as {GetPlayerDisplayName()}";
        }
        GUI.enabled = true;
        Screen.SetResolution(1920, 1080, true);
    }

    private string GetPlayerDisplayName()
    {
        return String.IsNullOrEmpty(autolooPlayerData.PlayerName) ? autolooPlayerData.Auth0UserInfo.Email : autolooPlayerData.PlayerName;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 60, 30, 1000), menuMessage, guiStyle);
        GUI.Label(new Rect(10, 10, 30, 1000), $"version {configuration.version}", guiStyle);
        
        if (newUserSetup)
        {
            _ = ShowNewUserSetupDialogue();
        }
        if (!string.IsNullOrEmpty(autolooPlayerData.PlayerName))
        {
            ShowChooseNation();
        }
    }
    void ShowT()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        GUI.color = Color.white;

        float topLineY = screenHeight * 0.2f; // 20% from the top
        GUI.DrawTexture(new Rect(0, topLineY, screenWidth, 2), Texture2D.whiteTexture);

        float middleX = screenWidth * 0.5f; // Horizontal center of the screen
        GUI.DrawTexture(new Rect(middleX - 1, topLineY, 2, screenHeight), Texture2D.whiteTexture); // 2px width for the vertical line
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

        GUI.enabled = !isSettingUpUser; 
        if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Set Name"))
        {
            isSettingUpUser = true; 

            if (playerName == "guest")
            {
                playerName = "guest" + autolooPlayerData.Auth0UserInfo.UserId;
            }

            await SetupNewUser();

            isSettingUpUser = false;
        }
        GUI.enabled = true;
    }

    private void ShowChooseNation()
    {
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        float inputHeight = -500;
        float buttonWidth = 250;
        float buttonHeight = 250;
        float buttonMarginX = 25;
        float buttonMarginY = 50;

        float inputY = centerY - (inputHeight * 0.5f) - buttonMarginY;
        float buttonX = centerX - buttonWidth - buttonMarginX;
        float button2X = (2*centerX) - buttonWidth - buttonMarginX;
        float buttonY = inputY + inputHeight + 20;

        if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), franceFlag))
        { 
            LoadGame("France");
        }
        if (GUI.Button(new Rect(button2X, buttonY, buttonWidth, buttonHeight), britainFlag))
        {
            LoadGame("Britain");
        }
        ShowChooseYourSideTexts();
        //ShowT();
        foreach (GameObject obj in frenchRoster)
        {
            obj.SetActive(true);
        }
        foreach (GameObject obj in britishRoster)
        {
            obj.SetActive(true);
        }
    }

    private void ShowChooseYourSideTexts()
    {
        // Enable the game objects if they are found
        if (mainMenuChooseYourSide != null)
            mainMenuChooseYourSide.SetActive(true);
        else
            Debug.LogWarning("MainMenuchooseYourSide not found!");

        if (playAsFrance != null)
            playAsFrance.SetActive(true);
        else
            Debug.LogWarning("PlayAsFrance not found!");

        if (playAsBritain != null)
            playAsBritain.SetActive(true);
        else
            Debug.LogWarning("PlayAsBritain not found!");
    }

    public void LoadGame(string playerRoster)
    {
        autolooPlayerData.RosterName = playerRoster;
        DontDestroyOnLoad(autolooPlayerData);
        //var ag = FindObjectOfType<AudioSource>().gameObject;
        Destroy(FindObjectOfType<AudioSource>().gameObject);
        SceneManager.LoadScene("SampleScene");
    }

    private async Task SetupNewUser()
    {
        JObject jsonObject = new JObject(
            new JProperty("playername", playerName),
            new JProperty("UnitDetails", new JArray())
        );

        string jsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        var newPlayerPostResponse = await FriendpasteClient.FriendpasteClient.PostDataAsync(FriendpasteClient.FriendpasteClient.BaseUrl, userId, FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(jsonString));
        var newPlayerPostResponseUrl = JObject.Parse(newPlayerPostResponse).Value<string>("url");

        var updatedJSON = AddUserIdAndPasteIdToDirectoryData(directoryData, userId, newPlayerPostResponseUrl);
        char firstChar = char.ToLower(userId[0]);
        await FriendpasteClient.FriendpasteClient.PutDataAsyncWithTimeout(directoryUrl, $"AutolooUserDirectory{firstChar}", FriendpasteClient.FriendpasteClient.PrepareJSONStringForBodyArgument(updatedJSON));

        Debug.Log($"Player name set to {playerName} and paste is at {newPlayerPostResponse}");
        autolooPlayerData.PlayerName = playerName;
        autolooPlayerData.PlayerData = await FriendpasteClient.FriendpasteClient.GetDataAsync(newPlayerPostResponseUrl);
        autolooPlayerData.PlayerDataPasteURL = newPlayerPostResponseUrl;
        menuMessage = $"Logged in as {GetPlayerDisplayName()}";
        newUserSetup = false;
    }

    private string ExtractPlayerNameFromData(string playerData)
    {
        JObject mainObject = JObject.Parse(playerData);
        string snippetJsonString = mainObject["snippet"].ToString();
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
