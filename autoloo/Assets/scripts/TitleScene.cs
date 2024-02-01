using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    public static float PlannedSceneLengthInSeconds = 5;
    float camStartZ;
    Material maskingPlaneMaterial;
    AudioSource titleAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1920, 1080, true);
        camStartZ = Camera.main.transform.position.z;
        maskingPlaneMaterial = GameObject.Find("Plane1").GetComponent<Renderer>().material; // .material.color.a = 0.5f
        titleAudioSource = GetComponent<AudioSource>();
        titleAudioSource.volume = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, camStartZ + Time.realtimeSinceStartup);
        var loHiLoAlpha = CalculateLoHiLoAlphaBasedOnPlannedSceneLength();
        ChangeAlpha(maskingPlaneMaterial, loHiLoAlpha);
        //volume and title alpha should be synched
        titleAudioSource.volume = 1 - loHiLoAlpha;
        if (Time.timeSinceLevelLoad > PlannedSceneLengthInSeconds)
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
        return Math.Abs(1 - (float)(Time.timeSinceLevelLoad / (PlannedSceneLengthInSeconds / 2)));
    }
}
