using System;
using System.Collections;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D), typeof(TouchingDirections))]
public class Knight : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float walkStopRate = 0.05f;
    public DetectionZone attackZone;

    [Header("Ledge & Wall Detection")]
    public float ledgeCheckDistance = 1f;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public Transform ledgeCheckPoint;
    public Transform wallCheckPoint;

    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public float attackDelay = 0.4f;
    private float attackTimer = 0f;

    Rigidbody2D rb;
    TouchingDirections touchingDirections;
    Animator animator;
    Damageable _damageable;

    public enum WalkableDirection { Right, Left }

    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector = Vector2.right; // Set default so it starts moving!

    public WalkableDirection WalkDirection 
    {  
        get 
        { 
            return _walkDirection; 
        }
        set 
        {
            if (_walkDirection != value)
            {
                // Direction flipped
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);
                if (value == WalkableDirection.Right) 
                {
                    walkDirectionVector = Vector2.right;
                }else if(value == WalkableDirection.Left) 
                {
                    walkDirectionVector = Vector2.left;
                }
            }

            _walkDirection = value;
        }
    }

    public bool _hasTarget = false;
    public bool HasTarget 
    { 
        get 
        { 
            return _hasTarget; 
        } 
        private set
        {
            _hasTarget = value;
            animator.SetBool(AnimationsStrings.hasTarget, value);
        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationsStrings.canMove);
        }
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        _damageable = GetComponent<Damageable>();
    }

    private void OnEnable()
    {
        if (_damageable != null)
        {
            _damageable.OnDeath += Die;
            _damageable.OnHit += OnKnightHit;
        }
    }

    private void OnDisable()
    {
        if (_damageable != null)
        {
            _damageable.OnDeath -= Die;
            _damageable.OnHit -= OnKnightHit;
        }
    }

    private void Start()
    {
        if (_damageable != null)
        {
            _damageable.MaxHealth = 20;
            _damageable.Health = 20;
        }

        // If groundLayer is not configured, default to checking "Ground" and "Default" layers
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground", "Default");
        }

        // Initialize walk direction vector based on starting scale
        if (transform.localScale.x > 0)
        {
            _walkDirection = WalkableDirection.Right;
            walkDirectionVector = Vector2.right;
        }
        else
        {
            _walkDirection = WalkableDirection.Left;
            walkDirectionVector = Vector2.left;
        }
    }

    private void OnKnightHit(int damage)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("knight_hit") || 
            animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.knight_hit"))
        {
            return;
        }
        animator.Play("knight_hit");
        Debug.Log("[Knight] Took damage: " + damage + ", Health remaining: " + _damageable.Health);
    }

    void Update()
    {
        if (_damageable != null && !_damageable.IsAlive) return;

        HasTarget = attackZone.detectedColliders.Count > 0;

        if (HasTarget)
        {
            if (attackTimer <= 0f)
            {
                StartCoroutine(DealDamageWithDelayRoutine(attackDelay));
                attackTimer = attackCooldown;
            }
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    private IEnumerator DealDamageWithDelayRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Verify knight is still alive, active in hierarchy, and not interrupted
        if (this != null && gameObject.activeInHierarchy && _damageable != null && _damageable.IsAlive)
        {
            // Verify if still in the attack animation state (to prevent damage if we were interrupted by taking a hit)
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isAttacking = stateInfo.IsName("knight_attack") || 
                               stateInfo.IsName("Base Layer.knight_attack") ||
                               stateInfo.IsName("Base Layer.Attack.knight_attack") ||
                               stateInfo.IsName("Attack.knight_attack");

            if (isAttacking)
            {
                // Re-evaluate targets in the attack zone at the moment of the hit
                foreach (Collider2D col in attackZone.detectedColliders)
                {
                    Damageable playerDamageable = col.GetComponent<Damageable>();
                    if (playerDamageable != null)
                    {
                        playerDamageable.Hit((int)attackDamage);
                        Debug.Log("[Knight] Attack hit player after delay of " + delay + "s!");
                    }
                }
            }
            else
            {
                Debug.Log("[Knight] Attack cancelled: Animator is no longer in the attack state.");
            }
        }
    }
    private void FixedUpdate()
    {
        if (_damageable != null && !_damageable.IsAlive)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (touchingDirections.IsGrounded && (touchingDirections.IsOnWall || DetectObstacleOrLedge())) 
        {
            FlipDirection();
        }
        if(CanMove) 
            rb.linearVelocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, walkStopRate), rb.linearVelocity.y);
    }

    private void Die()
    {
        animator.Play("knight_death");
        
        // Disable physics/colliders
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        var colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        Debug.Log("[Knight] Died.");

        // Start blinking and destruction sequence
        StartCoroutine(BlinkAndDestroyRoutine());
    }

    private IEnumerator BlinkAndDestroyRoutine()
    {
        // 1. Wait for death animation to complete
        yield return new WaitForSeconds(1.5f);

        // 2. Blink effect
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            int blinkCount = 12;
            float blinkDuration = 0.08f;
            
            for (int i = 0; i < blinkCount; i++)
            {
                sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(blinkDuration);
            }
            
            sr.enabled = false;
        }

        yield return new WaitForSeconds(0.2f);

        // 3. Destroy object
        Destroy(gameObject);
    }

    private bool DetectObstacleOrLedge()
    {
        Vector2 direction = walkDirectionVector;
        Collider2D myCol = GetComponent<Collider2D>();
        float feetY = myCol != null ? myCol.bounds.min.y : transform.position.y;

        // Wall detection via raycast
        Vector2 wallOrigin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position + new Vector2(0, 0.5f);
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, direction, wallCheckDistance, groundLayer);
        if (wallHit.collider != null)
        {
            return true;
        }

        // Ledge detection
        Vector2 ledgeOrigin;
        if (ledgeCheckPoint != null)
        {
            // Position origin slightly above the feet level to avoid starting inside the platform
            ledgeOrigin = new Vector2(ledgeCheckPoint.position.x, feetY + 0.1f);
        }
        else
        {
            // Fallback: 0.45f forward from center
            ledgeOrigin = new Vector2(transform.position.x + direction.x * 0.45f, feetY + 0.1f);
        }

        // Cast down for a short distance (0.5f is deep enough to reach below ground level safely)
        RaycastHit2D ledgeHit = Physics2D.Raycast(ledgeOrigin, Vector2.down, 0.5f, groundLayer);
        if (ledgeHit.collider == null)
        {
            return true; // No ground -> Ledge detected!
        }

        return false;
    }

    private void FlipDirection()
    {
        if (WalkDirection == WalkableDirection.Right)
        {
            WalkDirection = WalkableDirection.Left;
        }
        else if (WalkDirection == WalkableDirection.Left)
        {
            WalkDirection = WalkableDirection.Right;
        }
        else 
        {
            Debug.Log("Current walkable direction is not set to legal values of right or left");
        }
    }   
}
