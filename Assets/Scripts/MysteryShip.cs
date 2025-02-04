using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))] // Ensures that the object has a BoxCollider2D attached
public class MysteryShip : MonoBehaviour
{
    // Speed at which the ship moves
    public float speed = 5f;
    // Time taken for the complete cycle before the ship respawns
    public float cycleTime = 30f;
    // Points awarded when the ship is killed
    public int score = 300;

    // Internal variables to control the destination positions and direction
    private Vector2 leftDestination;
    private Vector2 rightDestination;
    private int direction = -1;  // Initial direction: left
    private bool spawned; // Determines if the ship is active or not

    private void Start()
    {
        // Convert viewport coordinates to world coordinates to set the ship's destination points
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        // Offset each destination by 1 unit to ensure the ship is fully out of sight
        leftDestination = new Vector2(leftEdge.x - 1f, transform.position.y);
        rightDestination = new Vector2(rightEdge.x + 1f, transform.position.y);

        // Initially call the method to despawn the ship
        Despawn();
    }

    private void Update()
    {
        // If the ship is not active, do nothing
        if (!spawned) return;

        // If the direction is positive, move the ship right, otherwise move left
        if (direction == 1)
        {
            MoveRight();
        }
        else
        {
            MoveLeft();
        }
    }

    // Moves the ship to the right
    private void MoveRight()
    {
        transform.position += speed * Time.deltaTime * Vector3.right;

        // If the ship reaches the right destination, despawn it
        if (transform.position.x >= rightDestination.x)
        {
            Despawn();
        }
    }

    // Moves the ship to the left
    private void MoveLeft()
    {
        transform.position += speed * Time.deltaTime * Vector3.left;

        // If the ship reaches the left destination, despawn it
        if (transform.position.x <= leftDestination.x)
        {
            Despawn();
        }
    }

    // Spawns the ship and places it at the start position depending on direction
    private void Spawn()
    {
        direction *= -1; // Flip the direction

        // If the direction is positive, spawn the ship on the left, otherwise on the right
        if (direction == 1)
        {
            transform.position = leftDestination;
        }
        else
        {
            transform.position = rightDestination;
        }

        spawned = true; // Mark the ship as active
    }

    // Despawns the ship and schedules its respawn after a cycle
    private void Despawn()
    {
        spawned = false; // Mark the ship as inactive

        // Place the ship outside of the view depending on the direction
        if (direction == 1)
        {
            transform.position = rightDestination;
        }
        else
        {
            transform.position = leftDestination;
        }

        // Invoke Spawn after a delay defined by cycleTime
        Invoke(nameof(Spawn), cycleTime);
    }

    // Detects when the ship is hit by a laser
    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the object colliding is a laser
        if (other.gameObject.layer == LayerMask.NameToLayer("Laser"))
        {
            Despawn(); // Despawn the ship
            GameManager.Instance.OnMysteryShipKilled(this); // Notify the GameManager
        }
    }
}
