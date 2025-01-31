using Unity.MLAgents;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;

    private PlayerAgent playerAgent;
    public Invaders invaders { get; private set; }
    private MysteryShip mysteryShip;
    private Bunker[] bunkers;

    public int score { get; private set; } = 0;
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
        if (lives <= 0 && Input.GetKeyDown(KeyCode.Return))
        {
            NewGame();
        }
    }

    public void NewGame()
    {
        gameOverUI.SetActive(false);
        SetScore(0);
        SetLives(3);
        NewRound();
    }

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

    private void Respawn()
    {
        Vector3 position = playerAgent.transform.position;
        position.x = 0f;
        playerAgent.transform.position = position;
        playerAgent.gameObject.SetActive(true);
    }

    private void GameOver()
    {
        gameOverUI.SetActive(true);
        invaders.gameObject.SetActive(false);
    }

    public void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(4, '0');
    }

    public void SetLives(int lives)
    {
        this.lives = Mathf.Max(lives, 0);
        livesText.text = this.lives.ToString();
    }

    public void OnPlayerKilled(PlayerAgent player)
    {
        SetLives(lives - 1);
        player.gameObject.SetActive(false);

        if (Academy.Instance.IsCommunicatorOn)
        {
            playerAgent.EndEpisode();
        }
        else
        {
            if (lives > 0)
            {
                Invoke(nameof(NewRound), 1f);
            }
            else
            { 
                GameOver();
            }
        }

        /*if (lives > 0)
        {
            Invoke(nameof(NewRound), 1f);
        }
        else
        {
            if (Academy.Instance.IsCommunicatorOn)
            {
                playerAgent.OnInvaderAtHome();
                playerAgent.EndEpisode();
            }
            else
            {
                GameOver();
            }
        }*/
    }

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
                playerAgent.EndEpisode();
            }
            else {
                NewRound();
            }
        }
    }

    public void OnMysteryShipKilled(MysteryShip mysteryShip)
    {
        SetScore(score + mysteryShip.score);
    }

    public void OnBoundaryReached()
    {
        if (invaders.gameObject.activeSelf)
        {
            invaders.gameObject.SetActive(false);
            playerAgent.OnInvaderAtHome();
            ; OnPlayerKilled(playerAgent);
        }
    }

    public void OnBunkerHit()
    {
        playerAgent.OnLaserHitBunker();
    }
}
