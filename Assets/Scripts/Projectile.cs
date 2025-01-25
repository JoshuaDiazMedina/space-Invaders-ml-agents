using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Projectile : MonoBehaviour
{
    private bool hitSomething = false; // Indica si el l�ser impact� algo
    private BoxCollider2D boxCollider;
    public Vector3 direction = Vector3.up;
    public float speed = 20f;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        transform.position += speed * Time.deltaTime * direction;

        // Destruir si est� fuera de los l�mites de la pantalla
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPosition.y < 0 || viewportPosition.y > 1)
        {
            NotifyMiss(); // Notifica que fall�
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

        if (bunker != null && bunker.CheckCollision(boxCollider, transform.position))
        {
            hitSomething = true;
            GameManager.Instance.OnBunkerHit();
            Destroy(gameObject);
        }
        else if (invader != null)
        {
            hitSomething = true;
            GameManager.Instance.OnInvaderKilled(invader);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (!hitSomething) NotifyMiss();
    }
    private void NotifyMiss()
    {
        PlayerAgent agent = FindObjectOfType<PlayerAgent>();
        if (agent != null)
        {
            agent.OnLaserMissed();
        }
    }
}
