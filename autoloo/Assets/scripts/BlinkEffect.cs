using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer backgroundSpriteRenderer;
    private Color originalColor;
    private Color originalBackgroundColor;
    // Start is called before the first frame update
    void Start()
    {
        Transform child = transform.Find("svgsprite");
        if (child == null)
        {
            Debug.LogError("Child object 'svgsprite' not found!");
            return;
        }
        
        spriteRenderer = child.GetComponent<SpriteRenderer>();

        Transform background = transform.Find("svgbackgroundsprite");
        if (background != null)
        {
            backgroundSpriteRenderer = child.GetComponent<SpriteRenderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Blink(int blinkTimes = 4, float blinkDuration = 0.1f, Color? blinkColor = null)
    {
        if (blinkColor == null)
        {
            blinkColor = Color.white;
        }
        for (int i = 0; i < blinkTimes; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }
}
