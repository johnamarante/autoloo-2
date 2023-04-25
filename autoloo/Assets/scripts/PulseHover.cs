using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseHover : MonoBehaviour
{
    int i = 0;
    bool down = false;
    int regulate = 1;
    public float yValue = 10.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //probably need to get the framerate in order for this to look consistent across different devices and mchines
        regulate++;
        if (regulate % 8 == 0)
        {
            regulate = 1;
            if (down)
            {
                i--;
                yValue -= 0.1f;
            }
            else
            {
                i++;
                yValue += 0.1f;
            }
            if (i > 19)
            {
                down = true;
            }
            if (i < 1)
            {
                down = false;
            }
        }
        Debug.Log(yValue);
    }
}
