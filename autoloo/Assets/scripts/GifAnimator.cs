using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GifAnimator : MonoBehaviour
{
    public Sprite[] frames; // Assign 18 PNG images in the Inspector
    public float frameRate = 0.1f; // Time per frame in seconds
    public float frameShift = -0.1f; 
    private SpriteRenderer imageComponent;
    private int currentFrame = 0;
    private Vector3 startingPosition;

    void Start()
    {
        startingPosition = transform.position;
        imageComponent = GetComponent<SpriteRenderer>(); // Assumes an Image component is attached to the GameObject
        if (frames.Length > 0)
        {
            StartCoroutine(PlayAnimation());
        }
    }

    IEnumerator PlayAnimation()
    {
        while (true)
        {
            imageComponent.sprite = frames[currentFrame];
            
            currentFrame = (currentFrame + 1) % frames.Length;
            if (currentFrame == 0)
            {
                transform.position = startingPosition;
            }
            else
            {
                transform.position += new Vector3(frameShift, 0, 0);
            }
            yield return new WaitForSeconds(frameRate);
            if (currentFrame == frames.Length - 1)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
