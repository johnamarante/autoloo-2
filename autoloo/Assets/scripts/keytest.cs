using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class keytest : MonoBehaviour
{
    public Text vText;
    public string friendpasteUrl = "https://www.friendpaste.com/";
    public string postJsonString = "";
    public string postUrl = "";
    string expectedPost = "C";
    

    // Start is called before the first frame update
    async Task Start()
    {
        postJsonString = await FireItUp();
        postUrl = JObject.Parse(postJsonString)["url"].ToString() + "\n";
        vText.text = postUrl;
    }

    // Update is called once per frame
    async Task Update()
    {
        if (Input.GetKeyDown("a"))
        {
            vText.text = vText.text + "a";
            await FriendpasteClient.PutDataAsync(postUrl, "TEST", vText.text + "a");
        }
    }

    private async Task<string> FireItUp()
    {
        return await FriendpasteClient.PostDataAsync(friendpasteUrl, "TEST", expectedPost);
    }
}
