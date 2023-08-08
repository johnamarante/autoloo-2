using UnityEngine;
using UnityEngine.UI;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LoginScreen : MonoBehaviour
{
    private string usernameOrEmail = "";
    private string password = "";

    private static string loginMessage = "";

    static string domain = "dev-shghx702iga4r13k.us.auth0.com";
    static string clientId = "by9THw2o1yHJpJksTstQlvesHk8IbMW1";
    static string clientSecret = "UvYtVbH6Zc1d__pWJaB454t6Jg95YFL4mG-tjMksOFyVVRoPCrSeCX5laY-SFZAL";
    static AuthenticationApiClient authApiClient = new AuthenticationApiClient(new Uri($"https://{domain}/"));
    private static UserInfo userInfo;

    void Start()
    {

    }
    private void Update()
    {
        if (userInfo != null && !string.IsNullOrEmpty(userInfo.UserId))
        {
            loginMessage = $"logged in as {userInfo.UserId} with email {userInfo.Email}";
            Debug.Log($"logged in as {userInfo.UserId} with email {userInfo.Email}");
            SceneManager.LoadScene("mainmenu");
        }
    }
    private async Task OnGUI()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Set the dimensions of UI elements
        float labelWidth = 250f; // Increased label width
        float fieldWidth = 800f;
        float fieldHeight = 100f;
        float buttonWidth = 400f;
        float buttonHeight = 120f;
        float spacing = 40f;

        // Calculate the total width of UI elements and spacings
        float totalWidth = (3 * buttonWidth) + (2 * spacing);

        // Calculate positions for UI elements
        float centerX = (screenWidth - totalWidth) * 0.5f + 50f; // Shifted to the right
        float centerY = screenHeight * 0.5f;
        float labelY = centerY - fieldHeight - buttonHeight - spacing;
        float usernameY = centerY - fieldHeight - buttonHeight - spacing;
        float passwordY = usernameY + fieldHeight + spacing;
        float buttonRowY = passwordY + fieldHeight + spacing;

        // Set the larger font size for text in input fields and buttons
        int fontSize = 48;
        Color fontColor = Color.black;
        GUI.skin.textField.fontSize = fontSize;
        GUI.skin.button.fontSize = fontSize;
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.label.normal.textColor = fontColor;

        // Draw UI elements
        GUI.Label(new Rect(centerX - labelWidth - spacing, labelY, labelWidth, fieldHeight), "Email");
        usernameOrEmail = GUI.TextField(new Rect(centerX, usernameY, fieldWidth, fieldHeight), usernameOrEmail);
        GUI.Label(new Rect(centerX - labelWidth - spacing, passwordY, labelWidth, fieldHeight), "Password");
        password = GUI.PasswordField(new Rect(centerX, passwordY, fieldWidth, fieldHeight), password, "*"[0]);
        if (GUI.Button(new Rect(centerX, buttonRowY, buttonWidth, buttonHeight), "Login"))
        {
            userInfo =  await Login(usernameOrEmail, password);
        }

        if (GUI.Button(new Rect(centerX + buttonWidth + spacing, buttonRowY, buttonWidth, buttonHeight), "Register"))
        {
            userInfo = await RegisterUser(usernameOrEmail, password);
        }

        if (GUI.Button(new Rect(centerX + buttonWidth + spacing, buttonRowY + buttonHeight + spacing, buttonWidth, buttonHeight), "Forgot Password"))
        {
            var forgotPasswordSuccess = await ForgotPassword(usernameOrEmail);
            if (forgotPasswordSuccess)
            {
                loginMessage = "Password reset instructions sent to the provided email address.";
                Debug.Log("Password reset instructions sent to the provided email address.");
            }
            else
            {
                loginMessage = "Password reset instructions sent to the provided email address.";
                Debug.Log("An error occurred while processing the forgot password request.");
            }
        }

        if (GUI.Button(new Rect(centerX, buttonRowY + buttonHeight + spacing, buttonWidth, buttonHeight), "Play as Guest"))
        {
            // Handle play as guest button click here
        }

        GUI.Label(new Rect(10, Screen.height - 60, 30, 1000), loginMessage, new GUIStyle() { normal = new GUIStyleState() { textColor = Color.black }, fontSize = fontSize });
    }

    static async Task<UserInfo> Login(string username, string password)
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
            var userInfo = await authApiClient.GetUserInfoAsync(tokenResponse.AccessToken);
            return userInfo;
        }
        catch (Exception ex)
        {
            loginMessage = $"Login error: {ex.Message}";
            Debug.Log($"Login error: {ex.Message}");
            return null;
        }
    }

    static async Task<UserInfo> RegisterUser(string email, string password)
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
            var signupResponse = await authApiClient.SignupUserAsync(signupUserRequest);

            if (signupResponse.Id != null)
            {
                loginMessage = $"New user has been registered with ID {signupResponse.Id}";
                Debug.Log($"New user has been registered with ID {signupResponse.Id}");
                return await Login(email, password);
            }
        }
        catch (Exception ex)
        {
            loginMessage = $"Registration error: {ex.Message}"; 
            Debug.Log($"Registration error: {ex.Message}");
        }
        loginMessage = $"There was a problem registering a new user";
        Debug.Log($"There was a problem registering a new user");
        return null;
    }

    static async Task<bool> ForgotPassword(string email)
    {
        var request = new ChangePasswordRequest
        {
            ClientId = clientId,
            Connection = "Username-Password-Authentication",
            Email = email
        };

        try
        {
            var changePasswordResponse = await authApiClient.ChangePasswordAsync(request);
            if (changePasswordResponse != null)
            {
                loginMessage = "Password reset instructions sent to the provided email address.";
                Debug.Log("Password reset instructions sent to the provided email address.");
                return true;
            }
        }
        catch (Exception ex)
        {
            loginMessage = $"Forgot password error: {ex.Message}";
            Debug.Log($"Forgot password error: {ex.Message}");
        }
        loginMessage = "An error occurred while processing the forgot password request.";
        Debug.Log("An error occurred while processing the forgot password request.");
        return false;
    }

}
