using UnityEngine;
using UnityEngine.UI;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using System;
using System.Threading.Tasks;

public class LoginScreen : MonoBehaviour
{

    private string usernameOrEmail = "";
    private string password = "";

    static string domain = "dev-shghx702iga4r13k.us.auth0.com";
    static string clientId = "by9THw2o1yHJpJksTstQlvesHk8IbMW1";
    static string clientSecret = "UvYtVbH6Zc1d__pWJaB454t6Jg95YFL4mG-tjMksOFyVVRoPCrSeCX5laY-SFZAL";
    static AuthenticationApiClient authApiClient = new AuthenticationApiClient(new Uri($"https://{domain}/"));

    void Start()
    {

    }
    private void Update()
    {
        
    }
    private async Task OnGUI()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Set the dimensions of UI elements
        float fieldWidth = 400f;
        float fieldHeight = 50f;
        float buttonWidth = 200f;
        float buttonHeight = 60f;
        float spacing = 20f;

        // Calculate the total width of UI elements and spacings
        float totalWidth = (3 * buttonWidth) + (2 * spacing);

        // Calculate positions for UI elements
        float centerX = (screenWidth - totalWidth) * 0.5f;
        float centerY = screenHeight * 0.5f;
        float usernameY = centerY - fieldHeight - buttonHeight - spacing;
        float passwordY = usernameY + fieldHeight + spacing;
        float buttonRowY = passwordY + fieldHeight + spacing;

        // Set the larger font size for text in input fields
        GUI.skin.textField.fontSize = 24;

        // Draw UI elements
        usernameOrEmail = GUI.TextField(new Rect(centerX, usernameY, fieldWidth, fieldHeight), usernameOrEmail);
        password = GUI.PasswordField(new Rect(centerX, passwordY, fieldWidth, fieldHeight), password, "*"[0]);
        if (GUI.Button(new Rect(centerX, buttonRowY, buttonWidth, buttonHeight), "Login"))
        {
            var ag = await Login("john.amarante8888@gmail.com", "assgrenade");
            Debug.Log(ag.ToString());
        }

        if (GUI.Button(new Rect(centerX + buttonWidth + spacing, buttonRowY, buttonWidth, buttonHeight), "Register"))
        {
            var hupu = RegisterUser(usernameOrEmail, password);
            Debug.Log(hupu.ToString());
        }

        if (GUI.Button(new Rect(centerX + buttonWidth + spacing, buttonRowY + buttonHeight + spacing, buttonWidth, buttonHeight), "Forgot Password"))
        {
            // Handle forgot password button click here
        }

        if (GUI.Button(new Rect(centerX, buttonRowY + buttonHeight + spacing, buttonWidth, buttonHeight), "Play as Guest"))
        {
            // Handle play as guest button click here
        }
    }

    static async Task<string> Login(string username, string password)
    {
        var request = new ResourceOwnerTokenRequest
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scope = "openid",
            Audience = $"https://{domain}/api/v2/",
            Realm = "Username-Password-Authentication",
            Username = username,
            Password = password
        };

        try
        {
            var tokenResponse = await authApiClient.GetTokenAsync(request);
            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            Debug.Log($"Login error: {ex.Message}");
            return null;
        }
    }

    static async Task<SignupUserResponse> RegisterUser(string email, string password)
    {
        var signupUserRequest = new SignupUserRequest
        {
            ClientId = clientId,
            Email = email,
            Password = password,
            Connection = "Username-Password-Authentication" // Replace with the desired connection name
        };

        try
        {
            //var result = await authApiClient.SignupUserAsync(signupUserRequest);
            //return result;
            return await authApiClient.SignupUserAsync(signupUserRequest);
        }
        catch (Exception ex)
        {
            Debug.Log($"Registration error: {ex.Message}");
            return null;
        }
    }

}
