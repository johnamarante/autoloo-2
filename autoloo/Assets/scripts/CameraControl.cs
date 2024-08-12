using System;
using System.Collections;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    //Lerping START
    // Transforms to act as start and end markers for the journey.
    public Vector3 startMarker;
    public Vector3 endMarker;
    // Movement speed in units per second.
    public float speed = 200.0F;
    // Time when the movement started.
    private float startTime;
    // Total distance between the markers.
    private float journeyLength;
    private bool move = false;
    //Lerping END

    public GameManager gameManager;
    private SpriteRenderer fadeMask;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = (GameManager)FindObjectOfType(typeof(GameManager));
        fadeMask = gameObject.transform.Find("FadeMask").GetComponent<SpriteRenderer>();
        // Ensure the CanvasGroup is fully visible at the start
        Color color = fadeMask.color;
        color.a = 0; // Set the alpha value
        fadeMask.color = color;
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
        //ResourcePoints Icon
        this.transform.Find("ResourcePoints").gameObject.SetActive(!gameManager.InBattleModeAndNotDeploymentMode);
        //sign that says reinforcements Icon
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

    // Coroutine to fade out the screen
    public void FadeOut()
    {
        StartCoroutine(Fade(0f, 1f)); // Fade from transparent to black
    }

    // Coroutine to fade in the screen
    public void FadeIn()
    {
        StartCoroutine(Fade(1f, 0f)); // Fade from black to transparent
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color color1 = fadeMask.color;
            color1.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            fadeMask.color = color1;
            yield return null;
        }
        Color color2 = fadeMask.color;
        color2.a = endAlpha;
        fadeMask.color = color2;
    }

}
