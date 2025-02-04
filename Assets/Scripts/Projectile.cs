using System;
using UnityEngine;

/// <summary>
/// Controls the behavior of a projectile, including movement, collisions, and screen boundary detection.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Projectile : MonoBehaviour
{
    private BoxCollider2D boxCollider; ///< The BoxCollider2D attached to the projectile.

                                       /// <summary>
                                       /// The direction in which the projectile moves.
                                       /// Default is upward.
                                       /// </summary>
    public Vector3 direction = Vector3.up;

    /// <summary>
    /// The speed at which the projectile travels.
    /// </summary>
    public float speed = 20f;

    /// <summary>
    /// The GameObject that fired this projectile.
    /// Used for tracking ownership and applying rewards or penalties.
    /// </summary>
    public GameObject shooter;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the BoxCollider2D component.
    /// </summary>
    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// Moves the projectile in its specified direction and checks if it leaves the screen boundaries.
    /// </summary>
    private void Update()
    {
        // Move the projectile
        transform.position += speed * Time.deltaTime * direction;

        // Check if the projectile is out of screen bounds
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPosition.y > 1) // Exceeds the top of the screen
        {
            PlayerAgent player = shooter.GetComponent<PlayerAgent>();
            player.OnLaserMissed(); // Notify the shooter that the projectile missed
            Destroy(gameObject);
        }
        else if (viewportPosition.y < 0) // Exceeds the bottom of the screen
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Handles collisions when the projectile enters a trigger collider.
    /// </summary>
    /// <param name="other">The collider that the projectile has entered.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckCollision(other);
    }

    /// <summary>
    /// Handles collisions while the projectile remains in a trigger collider.
    /// </summary>
    /// <param name="other">The collider that the projectile is staying in.</param>
    private void OnTriggerStay2D(Collider2D other)
    {
        CheckCollision(other);
    }

    /// <summary>
    /// Checks for specific collisions with bunkers, invaders, or other relevant objects.
    /// </summary>
    /// <param name="other">The collider to check for collisions.</param>
    private void CheckCollision(Collider2D other)
    {
        Bunker bunker = other.gameObject.GetComponent<Bunker>();
        Invader invader = other.gameObject.GetComponent<Invader>();
        PlayerAgent agent = other.gameObject.GetComponent<PlayerAgent>();

        if (bunker != null && bunker.CheckCollision(boxCollider, transform.position))
        {
            if (transform.position.y > bunker.transform.position.y)
            {
                // Additional case: enemy missile hitting the bunker
                // Handle specific logic here if needed
            }
            else
            {
                GameManager.Instance.OnBunkerHit(); // Notify GameManager of bunker hit
            }
            Destroy(gameObject);
        }
        else if (invader != null)
        {
            GameManager.Instance.OnInvaderKilled(invader); // Notify GameManager of invader killed
            Destroy(gameObject);
        }
    }
}
