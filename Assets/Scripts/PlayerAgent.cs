using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using System.Diagnostics;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerAgent : Agent
{
    public float speed = 10f;
    public Projectile laserPrefab;
    private Projectile laser;
    public override void OnEpisodeBegin()
    {
        if (Academy.Instance.IsCommunicatorOn)
        {
            Time.timeScale = 0.9f; // Ralentiza durante el entrenamiento
        }
        else
        {
            Time.timeScale = 1.0f; // Velocidad normal en inferencia
        }

        //Destruir cualquier misil activo al momento de empezar episodios
        Projectile[] activeProjectiles = FindObjectsOfType<Projectile>();
        foreach (Projectile projectile in activeProjectiles)
        {
            if (projectile.gameObject.layer == LayerMask.NameToLayer("Missile"))
            {
                Destroy(projectile.gameObject);
            }
        }
        // Destruir láser existente antes de empezar episodios
        if (laser != null) Destroy(laser.gameObject);

        // Reiniciar el entorno
        GameManager.Instance.NewRound();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Posición de la nave (2)
        sensor.AddObservation(transform.position.x / Camera.main.orthographicSize);
        sensor.AddObservation(transform.position.y / Camera.main.orthographicSize);

        Bunker[] bunkers = FindObjectsOfType<Bunker>();
        foreach (Bunker bunker in bunkers)
        {
            // Posición del bunker (8)
            sensor.AddObservation(bunker.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(bunker.transform.position.y / Camera.main.orthographicSize);
        }

        Invader[] invaders = FindObjectsOfType<Invader>();
        foreach (Invader invader in invaders)
        {
            // Posición del bunker (110)
            sensor.AddObservation(invader.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(invader.transform.position.y / Camera.main.orthographicSize);
        }
        // Información de los misiles más cercanos (hasta 1)
        Projectile[] activeMissiles = FindObjectsOfType<Projectile>();
        var closestMissiles = activeMissiles
            .Where(m => m.gameObject.layer == LayerMask.NameToLayer("Missile"))
            .OrderBy(m => Vector2.Distance(transform.position, m.transform.position))
            .Take(1);

        int missileCount = 0;
        foreach (var missile in closestMissiles)
        {
            // Posición del misil (2)
            sensor.AddObservation(missile.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(missile.transform.position.y / Camera.main.orthographicSize);
            missileCount++;
        }
        MysteryShip[] mysteryShips = FindObjectsOfType<MysteryShip>();
        foreach (var mystery in mysteryShips)
        {
            // Posición del misil (2)
            sensor.AddObservation(mystery.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(mystery.transform.position.y / Camera.main.orthographicSize);
        }


        //idealmente deberian ir las posiciones de los bunkers y manejar el comportamiento con los rewards cuando la nave dispare
    }



    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        //float moveInput = actions.ContinuousActions[0];
        //UnityEngine.Debug.Log(actions.ContinuousActions[0]);
        Vector3 position = transform.position;

        // Guardar posición anterior en X
        float previousX = position.x;

        position.x += moveInput * speed * Time.deltaTime;

        // Limitar movimiento a los bordes de la pantalla
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        position.x = Mathf.Clamp(position.x, leftEdge.x, rightEdge.x);

        transform.position = position;

        float distanceMoved = Mathf.Abs(position.x - previousX);
        if (distanceMoved == 0)
        {
            AddReward(-0.03f); // Aumentar recompensa si se mueve más
        }
        if (laser == null && actions.DiscreteActions[0] == 1)
        {
            laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            laser.shooter = this.gameObject;
        }
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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        UnityEngine.Debug.Log("Manual Reward: " + GetCumulativeReward());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Missile") ||
            other.gameObject.layer == LayerMask.NameToLayer("Invader"))
        {
            UnityEngine.Debug.Log("Me cayó un misil");
            SetReward(-1f);
            if (Academy.Instance.IsCommunicatorOn)
            {
                EndEpisode();
            }
            else
            {
                GameManager.Instance.OnPlayerKilled(this);
            }
            //EndEpisode();
        }
    }
    public void OnLaserHitInvader()
    {
        AddReward(0.018f); //Recompensa por disparar a un invader y destruirlo
    }

    public void OnLaserHitBunker()
    {
        AddReward(-0.01f); // Penalización por destruir un bunker
    }
    public void OnLaserMissed()
    {
        UnityEngine.Debug.Log("Laser perdido");
        AddReward(-0.05f); // Penalización por disparar sin impactar un invasor
    }
    public void OnHitAllInvader()
    {
        UnityEngine.Debug.Log("Terminé con todos los invaders");
        SetReward(1f);
        EndEpisode();
    }
    public void OnInvaderAtHome()
    {
        UnityEngine.Debug.Log("Pierdo una vida, invaders en casa");
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

    public void OnMysteryShipKilledReward()
    {
        AddReward(0.1f);
    }
}