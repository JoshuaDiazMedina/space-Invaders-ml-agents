using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

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
            Time.timeScale = 0.8f; // Ralentiza durante el entrenamiento
        }
        else
        {
            Time.timeScale = 1.0f; // Velocidad normal en inferencia
        }

        Projectile[] activeProjectiles = FindObjectsOfType<Projectile>();
        foreach (Projectile projectile in activeProjectiles)
        {
            if (projectile.gameObject.layer == LayerMask.NameToLayer("Missile"))
            {
                Destroy(projectile.gameObject);
            }
        }
        // Resetear posición del jugador
        Vector3 startPosition = new Vector3(0, transform.position.y, 0);
        transform.position = startPosition;

        // Destruir láser existente
        if (laser != null) Destroy(laser.gameObject);

        // Reiniciar el entorno
        GameManager.Instance.NewRound();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Posición del jugador (1)
        sensor.AddObservation(transform.position.x / Camera.main.orthographicSize);

        // Estado del láser (1)
        sensor.AddObservation(laser != null ? 1.0f : 0.0f);

        // Posiciones de los invasores (hasta un máximo de 55)
        int maxInvaders = 55;
        int invaderCount = 0;
        foreach (Transform invader in GameManager.Instance.invaders.transform)
        {
            if (invader.gameObject.activeSelf)
            {
                sensor.AddObservation(invader.position.x / Camera.main.orthographicSize);
                sensor.AddObservation(invader.position.y / Camera.main.orthographicSize);
                invaderCount++;
            }
        }

        // Rellenar con ceros si hay menos de 55 invasores
        for (int i = invaderCount; i < maxInvaders; i++)
        {
            sensor.AddObservation(0.0f); // Posición X
            sensor.AddObservation(0.0f); // Posición Y
        }

        // Posiciones de los misiles más cercanos (hasta 4)
        Projectile[] activeMissiles = FindObjectsOfType<Projectile>();
        var closestMissiles = activeMissiles
            .Where(m => m.gameObject.layer == LayerMask.NameToLayer("Missile"))
            .OrderBy(m => Vector2.Distance(transform.position, m.transform.position))
            .Take(4);

        int missileCount = 0;
        foreach (var missile in closestMissiles)
        {
            sensor.AddObservation(missile.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(missile.transform.position.y / Camera.main.orthographicSize);
            missileCount++;
        }

        // Rellenar con ceros si hay menos de 4 misiles
        for (int i = missileCount; i < 4; i++)
        {
            sensor.AddObservation(0.0f); // Posición X
            sensor.AddObservation(0.0f); // Posición Y
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        Vector3 position = transform.position;
        position.x += moveInput * speed * Time.deltaTime;

        // Limitar movimiento a los bordes de la pantalla
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);
        position.x = Mathf.Clamp(position.x, leftEdge.x, rightEdge.x);

        transform.position = position;

        if (laser == null)
        {
            laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        var discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Missile") ||
            other.gameObject.layer == LayerMask.NameToLayer("Invader"))
        {
            //Debug.Log("Lo matan, penaliza");
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public void OnLaserHitInvader()
    {
        //Debug.Log("Dispara a Invader, recompenza");
        AddReward(10.0f);
    }

    public void OnLaserHitBunker()
    {
        Debug.Log("Dispara a bunker, penaliza");
        AddReward(-1.0f); // Penalización por destruir un bunker
    }
    public void OnLaserMissed()
    {
        //Debug.Log("Disparo fallado, penaliza");
        AddReward(-1.0f); // Penalización por disparar sin impactar un invasor
    }
}
