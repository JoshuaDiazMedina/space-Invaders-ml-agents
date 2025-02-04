using UnityEngine;

/// <summary>
/// Represents an invader that animates through a set of sprites and interacts with lasers and boundaries.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Invader : MonoBehaviour
{
    /// <summary>
    /// The array of sprites used for the invader's animation.
    /// </summary>
    public Sprite[] animationSprites = new Sprite[0];

    /// <summary>
    /// The time interval (in seconds) between animation frames.
    /// </summary>
    public float animationTime = 1f;

    /// <summary>
    /// The score awarded when this invader is destroyed.
    /// </summary>
    public int score = 10;

    /// <summary>
    /// Reference to the SpriteRenderer component used to display the current sprite.
    /// </summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// The current animation frame index.
    /// </summary>
    private int animationFrame;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Initializes the SpriteRenderer and sets the initial sprite.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = animationSprites[0];
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Begins the repeating animation cycle.
    /// </summary>
    private void Start()
    {
        InvokeRepeating(nameof(AnimateSprite), animationTime, animationTime);
    }

    /// <summary>
    /// Advances the animation frame, looping back to the start if necessary,
    /// and updates the sprite displayed by the SpriteRenderer.
    /// </summary>
    private void AnimateSprite()
    {
        animationFrame++;

        // Loop back to the start if the animation frame exceeds the number of sprites.
        if (animationFrame >= animationSprites.Length)
        {
            animationFrame = 0;
        }

        spriteRenderer.sprite = animationSprites[animationFrame];
    }

    /// <summary>
    /// Called when another collider enters the trigger attached to this invader.
    /// Determines the type of collision and notifies the GameManager accordingly.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Laser"))
        {
            GameManager.Instance.OnInvaderKilled(this);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Boundary"))
        {
            GameManager.Instance.OnBoundaryReached();
        }
    }
}
