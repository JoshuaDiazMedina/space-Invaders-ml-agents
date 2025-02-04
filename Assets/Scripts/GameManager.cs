using Unity.MLAgents;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Manages the overall game logic, such as score, lives, rounds, and interactions between game objects.
/// </summary>
[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    public static GameManager Instance { get; private set; }

    [SerializeField]
    [Tooltip("UI element displayed when the player wins.")]
    private GameObject playerWinUI;

    [SerializeField]
    [Tooltip("UI element displayed when the game is over.")]
    private GameObject gameOverUI;

    [SerializeField]
    [Tooltip("Text UI element to display the player's score.")]
    private Text scoreText;

    [SerializeField]
    [Tooltip("Text UI element to display the player's remaining lives.")]
    private Text livesText;

    private PlayerAgent playerAgent; ///< Reference to the PlayerAgent component.
    public Invaders invaders { get; private set; } ///< Reference to the Invaders manager.
    private MysteryShip mysteryShip; ///< Reference to the MysteryShip.
    private Bunker[] bunkers; ///< Array containing references to all Bunkers in the scene.

    /// <summary>
    /// Current game score.
    /// </summary>
    public int score { get; private set; } = 0;

    /// <summary>
    /// Player's remaining lives.
    /// </summary>
    public int lives { get; private set; } = 3;

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        playerAgent = FindObjectOfType<PlayerAgent>();
        invaders = FindObjectOfType<Invaders>();
        mysteryShip = FindObjectOfType<MysteryShip>();
        bunkers = FindObjectsOfType<Bunker>();

        NewGame();
    }

    private void Update()
    {
        // Restart the game if Enter is pressed and the game has ended
        if (Input.GetKeyDown(KeyCode.Return) && (lives <= 0 || invaders.GetAliveCount() == 0))
        {
            NewGame();
        }
    }

    /// <summary>
    /// Initializes a new game, resetting the score, lives, and starting a new round.
    /// </summary>
    public void NewGame()
    {
        gameOverUI.SetActive(false);
        playerWinUI.SetActive(false);
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    /// <summary>
    /// Starts a new round, resetting the invaders and bunkers, and respawning the player.
    /// </summary>
    public void NewRound()
    {
        invaders.ResetInvaders();
        invaders.gameObject.SetActive(true);

        foreach (var bunker in bunkers)
        {
            bunker.ResetBunker();
        }

        Respawn();
    }

    /// <summary>
    /// Respawns the player at a random horizontal position.
    /// </summary>
    private void Respawn()
    {
        Vector3 position = playerAgent.transform.position;
        float randomX = Random.Range(-12.0f, 12.0f);
        position.x = randomX;
        playerAgent.transform.position = position;
        playerAgent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Displays the "Player Wins" UI and stops the invaders.
    /// </summary>
    private void PlayerWin()
    {
        invaders.gameObject.SetActive(false);
        playerWinUI.SetActive(true);
    }

    /// <summary>
    /// Ends the game and displays the "Game Over" UI.
    /// </summary>
    private void GameOver()
    {
        gameOverUI.SetActive(true);
        invaders.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the score and displays it on the UI.
    /// </summary>
    /// <param name="score">New score value.</param>
    public void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(4, '0');
    }

    /// <summary>
    /// Updates the player's remaining lives and displays it on the UI.
    /// </summary>
    /// <param name="lives">New lives value.</param>
    public void SetLives(int lives)
    {
        this.lives = Mathf.Max(lives, 0);
        livesText.text = this.lives.ToString();
    }

    /// <summary>
    /// Handles the logic when the player is killed.
    /// Reduces lives and starts a new round or ends the game if no lives remain.
    /// </summary>
    /// <param name="player">Reference to the PlayerAgent.</param>
    public void OnPlayerKilled(PlayerAgent player)
    {
        SetLives(lives - 1);
        player.gameObject.SetActive(false);
        if (lives > 0)
        {
            Invoke(nameof(NewRound), 1f);
        }
        else
        {
            GameOver();
        }
    }

    /// <summary>
    /// Handles the logic when an invader is killed.
    /// Updates the score and checks if all invaders are defeated.
    /// </summary>
    /// <param name="invader">The killed invader.</param>
    public void OnInvaderKilled(Invader invader)
    {
        invader.gameObject.SetActive(false);
        playerAgent.OnLaserHitInvader();
        SetScore(score + invader.score);
        if (invaders.GetAliveCount() == 0)
        {
            if (Academy.Instance.IsCommunicatorOn)
            {
                playerAgent.OnHitAllInvader();
            }
            else
            {
                PlayerWin();
            }
        }
    }

    /// <summary>
    /// Handles the logic when the mystery ship is destroyed.
    /// Updates the score and rewards the player.
    /// </summary>
    /// <param name="mysteryShip">The destroyed mystery ship.</param>
    public void OnMysteryShipKilled(MysteryShip mysteryShip)
    {
        SetScore(score + mysteryShip.score);
        playerAgent.OnMysteryShipKilledReward();
    }

    /// <summary>
    /// Ends the round when an invader reaches the boundary.
    /// </summary>
    public void OnBoundaryReached()
    {
        if (invaders.gameObject.activeSelf)
        {
            invaders.gameObject.SetActive(false);
            playerAgent.OnInvaderAtHome();
        }
    }

    /// <summary>
    /// Handles the logic when a laser hits a bunker.
    /// Penalizes the player for damaging a bunker.
    /// </summary>
    public void OnBunkerHit()
    {
        playerAgent.OnLaserHitBunker();
    }
}
