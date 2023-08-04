using UnityEngine;
using UnityEngine.UI;

public class LoginScreen : MonoBehaviour
{

    private string usernameOrEmail = "";
    private string password = "";

    void Start()
    {

    }
    private void Update()
    {
        
    }
    private void OnGUI()
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
            // Handle login button click here
        }

        if (GUI.Button(new Rect(centerX + buttonWidth + spacing, buttonRowY, buttonWidth, buttonHeight), "Register"))
        {
            // Handle register button click here
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

}
