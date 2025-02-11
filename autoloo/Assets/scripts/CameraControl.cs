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
        //TODO this needs to be refactored, this should not be in update()
        //have gamemanager call a method to set these based on the InBattleModeAndNotDeploymentMode property
        //ResourcePoints Icon
        this.transform.Find("ResourcePoints").gameObject.SetActive(!gameManager.InBattleModeAndNotDeploymentMode);
        this.transform.Find("Hearts").gameObject.SetActive(!gameManager.InBattleModeAndNotDeploymentMode);
        this.transform.Find("Wins").gameObject.SetActive(!gameManager.InBattleModeAndNotDeploymentMode);
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

    public IEnumerator FadeOutMoveFadeIn(Vector3 destination, bool returnToDeploy = false)
    {
        float fadeDuration = 0.5f;
        int initialRoundNumber = gameManager.roundNumber;

        // Fade out
        yield return Fade(0f, 1f, fadeDuration);

        // Move to the destination
        Move(destination);

        // Wait until movement is complete or round number changes
        yield return WaitForMovementOrRoundChange(returnToDeploy, initialRoundNumber);

        // Fade in
        yield return Fade(1f, 0f, fadeDuration);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color color = fadeMask.color;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            fadeMask.color = color;
            yield return null;
        }

        // Ensure the final color is set
        Color finalColor = fadeMask.color;
        finalColor.a = endAlpha;
        fadeMask.color = finalColor;
    }

    private IEnumerator WaitForMovementOrRoundChange(bool returnToDeploy, int initialRoundNumber)
    {
        if (returnToDeploy)
        {
            while (move || gameManager.roundNumber == initialRoundNumber)
            {
                yield return null;
            }
        }
        else
        {
            while (move)
            {
                yield return null;
            }
        }
    }

}
