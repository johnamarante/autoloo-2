using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    public static float PlannedSceneLengthInSeconds = 4;
    float camStartZ;
    Material maskingPlaneMaterial;
    // Start is called before the first frame update
    void Start()
    {
        camStartZ = Camera.main.transform.position.z;
        maskingPlaneMaterial = GameObject.Find("Plane1").GetComponent<Renderer>().material; // .material.color.a = 0.5f
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, camStartZ + Time.realtimeSinceStartup);
        ChangeAlpha(maskingPlaneMaterial, CalculateLoHiLoAlphaBasedOnPlannedSceneLength());
        if (Time.timeSinceLevelLoad > PlannedSceneLengthInSeconds)
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    static void ChangeAlpha(Material mat, float alphaValue)
    {
        Color oldColor = mat.color;
        Color newColor = new Color(oldColor.r, oldColor.g, oldColor.b, alphaValue);
        Debug.Log(alphaValue);
        mat.SetColor("_Color", newColor);
    }

    //fade in, fade out
    static float CalculateLoHiLoAlphaBasedOnPlannedSceneLength()
    {
        return Math.Abs(1 - (float)(Time.timeSinceLevelLoad / (PlannedSceneLengthInSeconds / 2)));
    }
}
