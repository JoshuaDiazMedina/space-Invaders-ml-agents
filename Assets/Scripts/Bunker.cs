using UnityEngine;

/// <summary>
/// Represents a bunker that can be visually damaged by applying a "splat" effect.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Bunker : MonoBehaviour
{
    /// <summary>
    /// The texture used to apply the "splat" effect (to simulate damage).
    /// </summary>
    public Texture2D splat;

    /// <summary>
    /// Holds the original sprite texture for resetting the bunker.
    /// </summary>
    private Texture2D originalTexture;

    /// <summary>
    /// Reference to the SpriteRenderer component.
    /// </summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Reference to the BoxCollider2D component.
    /// </summary>
    private BoxCollider2D boxCollider;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// It initializes the necessary components and resets the bunker.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalTexture = spriteRenderer.sprite.texture;

        ResetBunker();
    }

    /// <summary>
    /// Resets the bunker by creating a unique instance of the sprite texture.
    /// This allows the texture to be modified independently for each bunker.
    /// </summary>
    public void ResetBunker()
    {
        // Each bunker requires a unique texture instance since we modify it at the source.
        CopyTexture(originalTexture);

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Creates a copy of the original texture and assigns it to the sprite.
    /// </summary>
    /// <param name="source">The source texture to copy.</param>
    private void CopyTexture(Texture2D source)
    {
        // Create a new texture with the same dimensions and format as the source.
        Texture2D copy = new Texture2D(source.width, source.height, source.format, false)
        {
            filterMode = source.filterMode,
            anisoLevel = source.anisoLevel,
            wrapMode = source.wrapMode
        };

        // Copy the pixels from the source texture to the copy.
        copy.SetPixels32(source.GetPixels32());
        copy.Apply();

        // Create a new sprite using the copied texture and assign it to the SpriteRenderer.
        Sprite sprite = Sprite.Create(copy, spriteRenderer.sprite.rect, new Vector2(0.5f, 0.5f), spriteRenderer.sprite.pixelsPerUnit);
        spriteRenderer.sprite = sprite;
    }

    /// <summary>
    /// Checks for a collision between the bunker and another object (represented by its BoxCollider2D)
    /// at a given hit point. It examines the central hit point and the edges for more accurate collision detection.
    /// </summary>
    /// <param name="other">The BoxCollider2D of the colliding object.</param>
    /// <param name="hitPoint">The world-space point where the collision occurred.</param>
    /// <returns>
    /// True if the splat effect was applied (collision detected) at any checked point, false otherwise.
    /// </returns>
    public bool CheckCollision(BoxCollider2D other, Vector3 hitPoint)
    {
        Vector2 offset = other.size / 2;

        // Check the central hit point and the edges (down, up, left, right).
        return Splat(hitPoint) ||
               Splat(hitPoint + (Vector3.down * offset.y)) ||
               Splat(hitPoint + (Vector3.up * offset.y)) ||
               Splat(hitPoint + (Vector3.left * offset.x)) ||
               Splat(hitPoint + (Vector3.right * offset.x));
    }

    /// <summary>
    /// Applies the splat effect at the specified hit point by modifying the texture's transparency.
    /// </summary>
    /// <param name="hitPoint">The world-space hit point.</param>
    /// <returns>True if the splat effect was applied; otherwise, false.</returns>
    private bool Splat(Vector3 hitPoint)
    {
        // Only proceed if the hit point maps to a non-empty (non-transparent) pixel.
        if (!CheckPoint(hitPoint, out int px, out int py))
        {
            return false;
        }

        Texture2D texture = spriteRenderer.sprite.texture;

        // Offset the point by half the splat texture's size to center the splat around the hit point.
        px -= splat.width / 2;
        py -= splat.height / 2;

        int startX = px;

        // Loop through all coordinates of the splat texture to apply an alpha mask to the bunker texture.
        for (int y = 0; y < splat.height; y++)
        {
            px = startX;

            for (int x = 0; x < splat.width; x++)
            {
                // Retrieve the current pixel from the bunker texture.
                Color pixel = texture.GetPixel(px, py);
                // Multiply the bunker pixel's alpha by the splat pixel's alpha,
                // simulating damage in that area.
                pixel.a *= splat.GetPixel(x, y).a;
                texture.SetPixel(px, py, pixel);
                px++;
            }

            py++;
        }

        // Apply the texture changes.
        texture.Apply();

        return true;
    }

    /// <summary>
    /// Checks if the given hit point maps to a non-empty (non-transparent) pixel of the texture.
    /// The hit point is converted from world space to local space, and then to texture (UV) coordinates.
    /// </summary>
    /// <param name="hitPoint">The world-space hit point.</param>
    /// <param name="px">Output parameter for the x-coordinate of the pixel in the texture.</param>
    /// <param name="py">Output parameter for the y-coordinate of the pixel in the texture.</param>
    /// <returns>
    /// True if the pixel at the given coordinates is not completely transparent; otherwise, false.
    /// </returns>
    private bool CheckPoint(Vector3 hitPoint, out int px, out int py)
    {
        // Transform the hit point from world space to the object's local space.
        Vector3 localPoint = transform.InverseTransformPoint(hitPoint);

        // Adjust the point so that the origin is the corner of the object instead of its center.
        localPoint.x += boxCollider.size.x / 2;
        localPoint.y += boxCollider.size.y / 2;

        Texture2D texture = spriteRenderer.sprite.texture;

        // Convert the local point to texture (UV) coordinates.
        px = (int)(localPoint.x / boxCollider.size.x * texture.width);
        py = (int)(localPoint.y / boxCollider.size.y * texture.height);

        // Return true if the pixel is not empty (i.e., not fully transparent).
        return texture.GetPixel(px, py).a != 0f;
    }

    /// <summary>
    /// Called when another collider enters the trigger attached to the bunker.
    /// If the colliding object belongs to the "Invader" layer, the bunker is deactivated.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Invader"))
        {
            gameObject.SetActive(false);
        }
    }
}
