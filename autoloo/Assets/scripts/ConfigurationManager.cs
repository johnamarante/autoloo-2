using UnityEngine;
using System.IO;

[System.Serializable]
public class Configuration
{
    public string version;
    public bool devMode;
}

public class ConfigurationManager : MonoBehaviour
{
    public Configuration config = new Configuration();

    public void Awake()
    {
        LoadConfiguration();
        Debug.Log("Version Number: " + config.version);
        Debug.Log("Dev Mode: " + config.devMode);
    }

    private void LoadConfiguration()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "config.json");
        try
        {
            string json = File.ReadAllText(filePath);
            Debug.Log(json);
            config = JsonUtility.FromJson<Configuration>(json);
        }
        catch
        {
            Debug.LogError("Config file not found at path: " + filePath);
        }
    }
}
