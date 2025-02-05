using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System.Diagnostics;
using Unity.Barracuda;
using UnityEditor.Timeline.Actions;

/// <summary>
/// Represents a Player Agent controlled by ML-Agents.
/// The agent can move horizontally, shoot lasers, and interact with invaders, bunkers, and missiles.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerAgent : Agent
{
    /// <summary>
    /// Movement speed of the player.
    /// </summary>
    public float speed = 10f;

    /// <summary>
    /// Prefab of the laser projectile.
    /// </summary>
    public Projectile laserPrefab;

    /// <summary>
    /// Active laser projectile.
    /// </summary>
    private Projectile laser;

    /// <summary>
    /// Called at the beginning of an episode to reset the environment.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Adjust time scale depending on training or inference.
        if (Academy.Instance.IsCommunicatorOn)
        {
            Time.timeScale = 0.9f; // Slower during training
        }
        else
        {
            Time.timeScale = 1.0f; // Normal speed during inference
        }

        // Destroy all active missiles at the start of an episode.
        Projectile[] activeProjectiles = FindObjectsOfType<Projectile>();
        foreach (Projectile projectile in activeProjectiles)
        {
            if (projectile.gameObject.layer == LayerMask.NameToLayer("Missile"))
            {
                Destroy(projectile.gameObject);
            }
        }

        // Destroy active laser if it exists.
        if (laser != null) Destroy(laser.gameObject);

        // Reset the game round.
        GameManager.Instance.NewRound();
    }

    /// <summary>
    /// Collects observations for the ML-Agent.
    /// Adds positional data of the player, bunkers, invaders, and missiles to the observation vector.
    /// </summary>
    /// <param name="sensor">The VectorSensor to store observations.</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Player position (normalized).
        sensor.AddObservation(transform.position.x / Camera.main.orthographicSize);
        sensor.AddObservation(transform.position.y / Camera.main.orthographicSize);

        // Add positions of all bunkers.
        Bunker[] bunkers = FindObjectsOfType<Bunker>();
        foreach (Bunker bunker in bunkers)
        {
            sensor.AddObservation(bunker.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(bunker.transform.position.y / Camera.main.orthographicSize);
        }

        // Add positions of all invaders.
        Invader[] invaders = FindObjectsOfType<Invader>();
        foreach (Invader invader in invaders)
        {
            sensor.AddObservation(invader.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(invader.transform.position.y / Camera.main.orthographicSize);
        }

        // Add position of the closest missile.
        Projectile[] activeMissiles = FindObjectsOfType<Projectile>();
        var closestMissiles = activeMissiles
            .Where(m => m.gameObject.layer == LayerMask.NameToLayer("Missile"))
            .OrderBy(m => Vector2.Distance(transform.position, m.transform.position))
            .Take(1);

        foreach (var missile in closestMissiles)
        {
            sensor.AddObservation(missile.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(missile.transform.position.y / Camera.main.orthographicSize);
        }

        // Add positions of mystery ships.
        MysteryShip[] mysteryShips = FindObjectsOfType<MysteryShip>();
        foreach (var mystery in mysteryShips)
        {
            sensor.AddObservation(mystery.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(mystery.transform.position.y / Camera.main.orthographicSize);
        }
    }

    /// <summary>
    /// Processes the actions decided by the ML-Agent and updates the player's behavior.
    /// </summary>
    /// <param name="actions">The actions chosen by the agent.</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Read horizontal movement input.
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        Vector3 position = transform.position;

        // Save previous X position.
        float previousX = position.x;

        // Update position based on input.
        position.x += moveInput * speed * Time.deltaTime;

        // Clamp position within screen boundaries.
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        position.x = Mathf.Clamp(position.x, leftEdge.x, rightEdge.x);

        // Apply position update.
        transform.position = position;

        // Penalize if no movement occurred.
        float distanceMoved = Mathf.Abs(position.x - previousX);
        if (distanceMoved == 0)
        {
            AddReward(-0.03f);
        }

        // Handle laser firing.
        if (laser == null && actions.DiscreteActions[0] == 1)
        {
            laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            laser.shooter = this.gameObject;
        }

        // Penalize proximity to missiles.
        Projectile[] activeMissil = FindObjectsOfType<Projectile>();
        var closestMissil = activeMissil
            .Where(m => m.gameObject.layer == LayerMask.NameToLayer("Missile"))
            .OrderBy(m => Vector2.Distance(transform.position, m.transform.position))
            .Take(1);

        foreach (var missile in closestMissil)
        {
            if (Mathf.Abs(missile.transform.position.x - transform.position.x) <= 1)
            {
                AddReward(-0.1f);
            }
        }
    }


    // Call decision request by events

    private void Update()
    {
        // También puedes agregar eventos en Update()

        /*float  NaveXpos =  (transform.position.x);
        UnityEngine.Debug.Log("Nave x:"+ NaveXpos);
        Bunker[] bunkers = FindObjectsOfType<Bunker>();
        foreach (Bunker bunker in bunkers)
        {
            if (Mathf.Abs(NaveXpos - (bunker.transform.position.x)) <= 1) {
                UnityEngine.Debug.Log("Decision requester for bunker");
                RequestDecision();
            }
        }

        Invader[] invaders = FindObjectsOfType<Invader>();
        foreach (Invader invader in invaders)
        {
            if (Mathf.Abs(NaveXpos - (invader.transform.position.x)) <= 0.6)
            {
                UnityEngine.Debug.Log("Decision requester for invader");
                RequestDecision();
            }
        }*/
        float NaveXpos = (transform.position.x);
        // Add position of the closest missile.
        Projectile[] activeMissiles = FindObjectsOfType<Projectile>();
        var closestMissiles = activeMissiles
            .Where(m => m.gameObject.layer == LayerMask.NameToLayer("Missile"))
            .Take(1);
        foreach (var missile in closestMissiles)
        {
            if (Mathf.Abs(NaveXpos - (missile.transform.position.x)) <= 0.5)
            {
                UnityEngine.Debug.Log("Decision requester for missile");
                RequestDecision();
            }
        }
    }




    /// <summary>
    /// Provides manual control for debugging and testing.
    /// </summary>
    /// <param name="actionsOut">Output action buffer for manual input.</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        UnityEngine.Debug.Log("Manual Reward: " + GetCumulativeReward());
    }

    /// <summary>
    /// Handles collision events with missiles and invaders.
    /// </summary>
    /// <param name="other">The collider that triggered the event.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Missile") ||
            other.gameObject.layer == LayerMask.NameToLayer("Invader"))
        {
            UnityEngine.Debug.Log("Hit by a missile or invader.");
            SetReward(-1f);
            if (Academy.Instance.IsCommunicatorOn)
            {
                EndEpisode();
            }
            else
            {
                GameManager.Instance.OnPlayerKilled(this);
            }
        }
    }

    /// <summary>
    /// Adds a reward for hitting an invader with a laser.
    /// </summary>
    public void OnLaserHitInvader()
    {
        AddReward(0.018f);
    }

    /// <summary>
    /// Penalizes for hitting a bunker with a laser.
    /// </summary>
    public void OnLaserHitBunker()
    {
        AddReward(-0.01f);
    }

    /// <summary>
    /// Penalizes for missing with a laser shot.
    /// </summary>
    public void OnLaserMissed()
    {
        UnityEngine.Debug.Log("Missed laser shot.");
        AddReward(-0.05f);
    }

    /// <summary>
    /// Rewards for destroying all invaders.
    /// </summary>
    public void OnHitAllInvader()
    {
        UnityEngine.Debug.Log("Destroyed all invaders.");
        SetReward(1f);
        EndEpisode();
    }

    /// <summary>
    /// Penalizes for allowing an invader to reach the home base.
    /// </summary>
    public void OnInvaderAtHome()
    {
        UnityEngine.Debug.Log("Invaders reached home base.");
        SetReward(-1f);
        if (Academy.Instance.IsCommunicatorOn)
        {
            EndEpisode();
        }
        else
        {
            GameManager.Instance.OnPlayerKilled(this);
        }
    }

    /// <summary>
    /// Rewards for destroying a mystery ship.
    /// </summary>
    public void OnMysteryShipKilledReward()
    {
        AddReward(0.1f);
    }
}
