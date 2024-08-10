using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ScoutReportItemRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
        AdjustSpriteSize();
    }

    private void AdjustSpriteSize()
    {
        if (spriteRenderer.sprite == null)
            return;

        // Assuming the ScoutReportItem is 1x1 units in size
        Vector2 scoutReportItemSize = GetComponent<RectTransform>().sizeDelta;

        // Calculate the aspect ratio of the sprite
        float spriteAspectRatio = spriteRenderer.sprite.bounds.size.x / spriteRenderer.sprite.bounds.size.y;
        float scoutReportItemAspectRatio = scoutReportItemSize.x / scoutReportItemSize.y;

        Vector2 newSize = Vector2.zero;

        if (spriteAspectRatio > scoutReportItemAspectRatio)
        {
            newSize.x = scoutReportItemSize.x;
            newSize.y = scoutReportItemSize.x / spriteAspectRatio;
        }
        else
        {
            newSize.y = scoutReportItemSize.y;
            newSize.x = scoutReportItemSize.y * spriteAspectRatio;
        }

        spriteRenderer.size = newSize;
    }
}
