using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public Vector2 moveSpeed = new Vector2(3f, 0);
    public Vector2 knockback = new Vector2(3f, 0);
    [HideInInspector] public GameObject owner;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.linearVelocity = new Vector2(moveSpeed.x * transform.localScale.x, moveSpeed.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore self-collision with the owner who fired the projectile
        if (owner != null && (collision.gameObject == owner || collision.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        Damageable damageable = collision.GetComponent<Damageable>();

        if (damageable != null)
        {
            // Determine scale direction from self (or parent if available) to avoid null reference exceptions
            float directionSign = transform.parent != null ? Mathf.Sign(transform.parent.localScale.x) : Mathf.Sign(transform.localScale.x);
            Vector2 deliveredKnockback = directionSign > 0 ? knockback : new Vector2(-knockback.x, knockback.y);

            // Hit the target
            bool gotHit = damageable.Hit(damage);
            Debug.Log(collision.name + " hit for " + damage);
            if (gotHit)
            {
                Rigidbody2D targetRb = collision.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    targetRb.linearVelocity = new Vector2(deliveredKnockback.x, targetRb.linearVelocity.y);
                }
                Destroy(gameObject);
            }
        }
    }
}
