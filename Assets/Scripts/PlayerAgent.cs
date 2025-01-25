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
        Projectile[] activeProjectiles = FindObjectsOfType<Projectile>();
        foreach (Projectile projectile in activeProjectiles)
        {
            if (projectile.gameObject.layer == LayerMask.NameToLayer("Missile"))
            {
                Destroy(projectile.gameObject);
            }
        }
        // Resetear posici�n del jugador
        Vector3 startPosition = new Vector3(0, transform.position.y, 0);
        transform.position = startPosition;

        // Destruir l�ser existente
        if (laser != null) Destroy(laser.gameObject);

        // Reiniciar el entorno
        GameManager.Instance.NewRound();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Posici�n del jugador normalizada
        sensor.AddObservation(transform.position.x / Camera.main.orthographicSize);

        // Estado del l�ser (1: activo, 0: no activo)
        sensor.AddObservation(laser != null ? 1.0f : 0.0f);

        // Posici�n de los invasores
        foreach (Transform invader in GameManager.Instance.invaders.transform)
        {
            if (invader.gameObject.activeSelf)
            {
                sensor.AddObservation(invader.position.x / Camera.main.orthographicSize);
                sensor.AddObservation(invader.position.y / Camera.main.orthographicSize);
            }
        }

        // Detectar los cuatro misiles m�s cercanos
        Projectile[] activeMissiles = FindObjectsOfType<Projectile>();
        var closestMissiles = activeMissiles
            .Where(m => m.gameObject.layer == LayerMask.NameToLayer("Missile"))
            .OrderBy(m => Vector2.Distance(transform.position, m.transform.position))
            .Take(4);

        foreach (var missile in closestMissiles)
        {
            sensor.AddObservation(missile.transform.position.x / Camera.main.orthographicSize);
            sensor.AddObservation(missile.transform.position.y / Camera.main.orthographicSize);
        }

        // Rellenar observaciones faltantes si hay menos de 4 misiles
        int missingMissiles = 4 - closestMissiles.Count();
        for (int i = 0; i < missingMissiles; i++)
        {
            sensor.AddObservation(0.0f); // Posici�n X vac�a
            sensor.AddObservation(0.0f); // Posici�n Y vac�a
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

        // Disparar l�ser si la acci�n discreta lo indica
        if (laser == null && actions.DiscreteActions[0] == 1)
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
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    public void OnLaserHitInvader()
    {
        AddReward(10.0f);
    }

    public void OnLaserHitBunker()
    {
        AddReward(-0.5f); // Penalizaci�n por destruir un bunker
    }
}
