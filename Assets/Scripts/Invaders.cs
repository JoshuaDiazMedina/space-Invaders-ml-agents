using UnityEngine;

/// <summary>
/// Manages a grid of invaders, their movement, and missile attacks.
/// </summary>
public class Invaders : MonoBehaviour
{
    #region Public Fields

    [Header("Invaders")]
    /// <summary>
    /// Array of invader prefabs. Each element corresponds to a specific row.
    /// </summary>
    public Invader[] prefabs = new Invader[5];

    /// <summary>
    /// AnimationCurve that controls the invaders' speed based on the percentage of invaders killed.
    /// </summary>
    public AnimationCurve speed = new AnimationCurve();

    [Header("Grid")]
    /// <summary>
    /// Number of rows in the invader grid.
    /// </summary>
    public int rows = 5;

    /// <summary>
    /// Number of columns in the invader grid.
    /// </summary>
    public int columns = 11;

    [Header("Missiles")]
    /// <summary>
    /// Prefab for the missile projectile.
    /// </summary>
    public Projectile missilePrefab;

    /// <summary>
    /// The rate (in seconds) at which missiles are spawned.
    /// </summary>
    public float missileSpawnRate = 1f;

    #endregion

    #region Private Fields

    /// <summary>
    /// The direction in which the invader grid is currently moving.
    /// </summary>
    private Vector3 direction = Vector3.right;

    /// <summary>
    /// The initial position of the invader grid.
    /// </summary>
    private Vector3 initialPosition;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// It stores the initial position and creates the invader grid.
    /// </summary>
    private void Awake()
    {
        initialPosition = transform.position;
        CreateInvaderGrid();
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// It begins the missile attack routine.
    /// </summary>
    private void Start()
    {
        InvokeRepeating(nameof(MissileAttack), missileSpawnRate, missileSpawnRate);
    }

    /// <summary>
    /// Update is called once per frame.
    /// It updates the grid's movement based on the current speed and checks for screen edge collisions.
    /// </summary>
    private void Update()
    {
        // Calculate the percentage of invaders killed.
        int totalCount = rows * columns;
        int amountAlive = GetAliveCount();
        int amountKilled = totalCount - amountAlive;
        float percentKilled = amountKilled / (float)totalCount;

        // Evaluate the speed of the invaders based on how many have been killed.
        float currentSpeed = this.speed.Evaluate(percentKilled);
        transform.position += currentSpeed * Time.deltaTime * direction;

        // Convert the viewport edges to world coordinates.
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        // Check each invader to determine if the grid has reached the screen edge.
        foreach (Transform invader in transform)
        {
            // Skip invaders that are not active (i.e., already killed).
            if (!invader.gameObject.activeInHierarchy)
            {
                continue;
            }

            // If moving right and the invader reaches the right edge (with margin), advance the row.
            if (direction == Vector3.right && invader.position.x >= (rightEdge.x - 1f))
            {
                AdvanceRow();
                break;
            }
            // If moving left and the invader reaches the left edge (with margin), advance the row.
            else if (direction == Vector3.left && invader.position.x <= (leftEdge.x + 1f))
            {
                AdvanceRow();
                break;
            }
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Creates a grid of invaders based on the specified number of rows and columns.
    /// Each row uses the corresponding invader prefab.
    /// </summary>
    private void CreateInvaderGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            float width = 2f * (columns - 1);
            float height = 2f * (rows - 1);

            Vector2 centerOffset = new Vector2(-width * 0.5f, -height * 0.5f);
            Vector3 rowPosition = new Vector3(centerOffset.x, (2f * i) + centerOffset.y, 0f);

            for (int j = 0; j < columns; j++)
            {
                // Create a new invader from the corresponding prefab and set its parent.
                Invader invader = Instantiate(prefabs[i], transform);

                // Calculate and set the local position of the invader within the grid.
                Vector3 position = rowPosition;
                position.x += 2f * j;
                invader.transform.localPosition = position;
            }
        }
    }

    /// <summary>
    /// Determines which invader will launch a missile based on a random chance.
    /// The chance is inversely proportional to the number of invaders still alive.
    /// </summary>
    private void MissileAttack()
    {
        int amountAlive = GetAliveCount();

        // Do not spawn missiles if no invaders are alive.
        if (amountAlive == 0)
        {
            return;
        }

        foreach (Transform invader in transform)
        {
            // Skip any invaders that have been killed.
            if (!invader.gameObject.activeInHierarchy)
            {
                continue;
            }

            // Random chance to spawn a missile based on the number of invaders alive.
            if (Random.value < (1f / amountAlive))
            {
                Instantiate(missilePrefab, invader.position, Quaternion.identity);
                break;
            }
        }
    }

    /// <summary>
    /// Advances the invader grid by one row:
    /// Flips the horizontal movement direction and moves the grid downward.
    /// </summary>
    private void AdvanceRow()
    {
        // Reverse the horizontal direction.
        direction = new Vector3(-direction.x, 0f, 0f);

        // Move the grid down by one unit.
        Vector3 position = transform.position;
        position.y -= 1f;
        transform.position = position;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Resets the invader grid to its initial position and reactivates all invaders.
    /// </summary>
    public void ResetInvaders()
    {
        direction = Vector3.right;
        transform.position = initialPosition;

        foreach (Transform invader in transform)
        {
            invader.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Returns the number of invaders that are still alive.
    /// </summary>
    /// <returns>The count of active invaders.</returns>
    public int GetAliveCount()
    {
        int count = 0;

        foreach (Transform invader in transform)
        {
            if (invader.gameObject.activeSelf)
            {
                count++;
            }
        }

        return count;
    }

    #endregion
}
