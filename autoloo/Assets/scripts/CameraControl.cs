using System;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    //Lerping START
    // Transforms to act as start and end markers for the journey.
    public Vector3 startMarker;
    public Vector3 endMarker;
    // Movement speed in units per second.
    public float speed = 40.0F;
    // Time when the movement started.
    private float startTime;
    // Total distance between the markers.
    private float journeyLength;
    private bool move = false;
    //Lerping END

    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManager)FindObjectOfType(typeof(GameManager));
    }

    // Update is called once per frame
    void Update()
    {
        if (move)
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = (Time.time - startTime) * speed;

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = 0;
            if (journeyLength > 0)
            {
                fractionOfJourney = distCovered / journeyLength;
            }

            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(startMarker, endMarker, fractionOfJourney);
            if (fractionOfJourney > 0.95F)
            {
                move = false;
                transform.position = endMarker;
            }
        }
        //CommandPoints Icon
        this.transform.Find("CommandPoints").gameObject.SetActive(!gameManager.InBattleModeAndNotDeploymentMode);
    }

    public void Move(Vector3 dest)
    {
        startMarker = transform.position;
        endMarker = dest;
        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        journeyLength = Vector3.Distance(startMarker, endMarker);
        move = true;
    }
}
