using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseHover : MonoBehaviour
{
    //int regulate = 1;
    public float yValue = 10.0f;
    private float lastTime;

    float startTime;
    float floatSpeed;
    float floatAmplitude;
    float floatOffset;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        floatSpeed = 5f; // Speed of the floating animation
        floatAmplitude = 5f; // Amplitude of the floating animation
        floatOffset = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        yValue = floatOffset + (Mathf.Sin((Time.time - startTime) * floatSpeed) * floatAmplitude);
    }
}
