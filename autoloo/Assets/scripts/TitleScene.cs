using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    public static float plannedSceneLengthInSeconds = 5;
    float camStartZ;
    Material maskingPlaneMaterial;
    public GameObject autolooMenuMusic;
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        camStartZ = Camera.main.transform.position.z;
        maskingPlaneMaterial = GameObject.Find("Plane1").GetComponent<Renderer>().material; // .material.color.a = 0.5f
        DontDestroyOnLoad(autolooMenuMusic);
        //set scene to fully "faded-in"
        ChangeAlpha(maskingPlaneMaterial, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //Camera move effect - uncomment below line (1) to re-enable
        //Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, camStartZ + Time.realtimeSinceStartup);

        //screen fade in-out effect - uncomment below lines (2) to re-enable
        //var loHiLoAlpha = CalculateLoHiLoAlphaBasedOnPlannedSceneLength();
        //ChangeAlpha(maskingPlaneMaterial, loHiLoAlpha);
        if (Time.timeSinceLevelLoad > plannedSceneLengthInSeconds || Input.GetMouseButton(0))
        {
            SceneManager.LoadScene("login");
        }
    }

    static void ChangeAlpha(Material mat, float alphaValue)
    {
        Color oldColor = mat.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alphaValue);
        mat.SetColor("_Color", newColor);
    }

    //fade in, fade out
    static float CalculateLoHiLoAlphaBasedOnPlannedSceneLength()
    {
        return Math.Abs(1 - (float)(Time.timeSinceLevelLoad / (plannedSceneLengthInSeconds / 2)));
    }
}
