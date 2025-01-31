using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Projectile : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    public Vector3 direction = Vector3.up;
    public float speed = 20f;
    public GameObject shooter;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        transform.position += speed * Time.deltaTime * direction;

        // Destruir si está fuera de los límites de la pantalla
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPosition.y > 1)
        {
            PlayerAgent player = shooter.GetComponent<PlayerAgent>();
            player.OnLaserMissed();
            Destroy(gameObject);
        }
        else if (viewportPosition.y < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckCollision(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        CheckCollision(other);
    }

    private void CheckCollision(Collider2D other)
    {
        Bunker bunker = other.gameObject.GetComponent<Bunker>();
        Invader invader = other.gameObject.GetComponent<Invader>();
        PlayerAgent agent = other.gameObject.GetComponent<PlayerAgent>();
        if (bunker != null && bunker.CheckCollision(boxCollider, transform.position))
        {
            if (transform.position.y > bunker.transform.position.y)
            {
                // hacer algo para el caso adicional de misil enemigo a bunker
            }
            else
            {
                GameManager.Instance.OnBunkerHit();
            }
            Destroy(gameObject);
        }
        else if (invader != null)
        {
            GameManager.Instance.OnInvaderKilled(invader);
            Destroy(gameObject);
        }
    }
}
