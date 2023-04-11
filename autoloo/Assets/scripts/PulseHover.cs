using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseHover : MonoBehaviour
{
    int i = 0;
    bool down = false;
    int regulate = 1;
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
                transform.position -= new Vector3(0f, 0.1f, 0f);
            }
            else
            {
                i++;
                transform.position += new Vector3(0f, 0.1f, 0f);
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
    }
}
