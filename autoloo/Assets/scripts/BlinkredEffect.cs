using System.Collections;
using UnityEngine;

public class BlinkredEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public int blinkTimes = 4; // Number of blinks
    public float blinkDuration = 0.1f; // Time between blinks

    void Start()
    {
        Transform child = transform.Find("svgsprite");
        if (child == null)
        {
            Debug.LogError("Child object 'svgsprite' not found!");
            return;
        }

        spriteRenderer = child.GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse click
        {
            StartCoroutine(BlinkRed());
        }
    }

    IEnumerator BlinkRed()
    {
        for (int i = 0; i < blinkTimes; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }
}
