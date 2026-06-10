using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(TouchingDirections))]
public class EnemyController : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float chaseSpeed = 2f;
    public float idleTime = 2f;
    public float maxHealth = 30f;
    public float detectRange = 6f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;

    [Header("Ledge & Wall Detection")]
    public float ledgeCheckDistance = 1f;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public Transform ledgeCheckPoint;
    public Transform wallCheckPoint;

    private float _currentHealth;
    private bool _isDead = false;
    private bool _isMoving = false;
    private float _attackCooldownTimer = 0f;
    private Transform _playerTransform;
    private Rigidbody2D _rb;
    private Animator _animator;
    private TouchingDirections _touchingDirections;
    private Coroutine _patrolWaitCoroutine;

    public enum State { Patrolling, Waiting, Chasing, Attacking, Dead }
    [SerializeField] private State _currentState = State.Patrolling;

    public bool IsMoving
    {
        get => _isMoving;
        private set
        {
            _isMoving = value;
            _animator.SetBool(AnimationsStrings.isMoving, value);
        }
    }

    // Determine if the enemy is facing right in world space based on local transform's orientation and scale
    public bool IsFacingRight => (transform.right.x * transform.localScale.x) > 0f;
    
    // World space direction the enemy is facing
    public Vector2 ForwardDirection => IsFacingRight ? Vector2.right : Vector2.left;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _touchingDirections = GetComponent<TouchingDirections>();
    }

    private void Start()
    {
        _currentHealth = maxHealth;
        
        // Find player controller in the scene
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (_isDead) return;

        if (_attackCooldownTimer > 0)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }

        EvaluateState();
    }

    private void FixedUpdate()
    {
        if (_isDead || _currentState == State.Dead)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            return;
        }

        switch (_currentState)
        {
            case State.Patrolling:
                PatrolMovement();
                break;
            case State.Chasing:
                ChaseMovement();
                break;
            case State.Waiting:
            case State.Attacking:
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                IsMoving = false;
                break;
        }
    }

    private void EvaluateState()
    {
        if (_playerTransform == null)
        {
            if (_currentState == State.Chasing || _currentState == State.Attacking)
            {
                _currentState = State.Patrolling;
            }
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
        float heightDifference = Mathf.Abs(transform.position.y - _playerTransform.position.y);

        bool canDetectPlayer = false;

        // Only detect the player if they are roughly on the same vertical level
        if (heightDifference <= 2.5f)
        {
            if (_currentState == State.Chasing || _currentState == State.Attacking)
            {
                // If already chasing or attacking, we maintain detection as long as they are within range
                canDetectPlayer = distanceToPlayer <= detectRange;
            }
            else
            {
                // If patrolling or waiting, we must see them (they are in front of our gaze)
                canDetectPlayer = distanceToPlayer <= detectRange && IsPlayerInSight();
            }
        }

        if (canDetectPlayer)
        {
            // Interrupt waiting/patrol coroutine immediately if it was running
            if (_patrolWaitCoroutine != null)
            {
                StopCoroutine(_patrolWaitCoroutine);
                _patrolWaitCoroutine = null;
            }

            // Always perform the attack when the player is at the attack distance (both horizontally and vertically)
            if (distanceToPlayer <= attackRange && heightDifference <= 1.0f)
            {
                _currentState = State.Attacking;
                TryAttack();
            }
            else
            {
                _currentState = State.Chasing;
            }
        }
        else
        {
            // If we lose the player, resume patrolling
            if (_currentState == State.Chasing || _currentState == State.Attacking)
            {
                _currentState = State.Patrolling;
            }
        }
    }

    private bool IsPlayerInSight()
    {
        // Vector pointing to player
        Vector2 dirToPlayer = (_playerTransform.position - transform.position).normalized;
        
        // Check if player is on the side we are facing in world space
        float faceDirection = IsFacingRight ? 1f : -1f;
        
        // If player is in front (dot product > 0.1f)
        return (dirToPlayer.x * faceDirection) > 0.1f;
    }

    private void PatrolMovement()
    {
        IsMoving = true;
        float speed = walkSpeed;
        float direction = IsFacingRight ? 1f : -1f;

        _rb.linearVelocity = new Vector2(direction * speed, _rb.linearVelocity.y);

        // Check for walls and ledges
        if (DetectObstacleOrLedge() && _patrolWaitCoroutine == null)
        {
            _patrolWaitCoroutine = StartCoroutine(WaitAndTurnAround());
        }
    }

    private void ChaseMovement()
    {
        IsMoving = true;
        float speed = chaseSpeed;
        
        // Determine direction to player in world space
        float directionToPlayer = _playerTransform.position.x - transform.position.x;
        bool shouldFaceRight = directionToPlayer > 0f;

        if (IsFacingRight != shouldFaceRight)
        {
            TurnAround();
        }

        float walkDir = IsFacingRight ? 1f : -1f;
        _rb.linearVelocity = new Vector2(walkDir * speed, _rb.linearVelocity.y);

        // Stop at ledges or walls if chasing
        if (DetectObstacleOrLedge())
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            IsMoving = false;
        }
    }

    private bool DetectObstacleOrLedge()
    {
        // Wall detection
        Vector2 wallOrigin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position + new Vector2(0, 0.5f);
        Vector2 direction = ForwardDirection;
        
        RaycastHit2D wallHit = Physics2D.Raycast(wallOrigin, direction, wallCheckDistance, groundLayer);
        if (wallHit.collider != null)
        {
            return true;
        }

        // Ledge detection
        Vector2 ledgeOrigin = ledgeCheckPoint != null ? (Vector2)ledgeCheckPoint.position : (Vector2)transform.position + new Vector2(direction.x * 0.5f, -0.5f);
        RaycastHit2D ledgeHit = Physics2D.Raycast(ledgeOrigin, Vector2.down, ledgeCheckDistance, groundLayer);
        if (ledgeHit.collider == null)
        {
            return true;
        }

        return false;
    }

    private IEnumerator WaitAndTurnAround()
    {
        _currentState = State.Waiting;
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        IsMoving = false;

        yield return new WaitForSeconds(idleTime);

        if (!_isDead)
        {
            TurnAround();
            _currentState = State.Patrolling;
        }
        _patrolWaitCoroutine = null;
    }

    private void TurnAround()
    {
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void TryAttack()
    {
        if (_attackCooldownTimer <= 0f)
        {
            _animator.SetTrigger(AnimationsStrings.attackTrigger);
            _attackCooldownTimer = attackCooldown;
            Debug.Log("[Skeleton] Attacked Player!");
        }
    }

    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        _animator.SetTrigger(AnimationsStrings.takeHitTrigger);

        Debug.Log("[Skeleton] Took damage: " + damage + ", Health remaining: " + _currentHealth);

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        _currentState = State.Dead;
        _animator.SetBool("isDeath", true);
        
        // Disable physics/colliders
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        
        var colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        Debug.Log("[Skeleton] Died.");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw Raycasts
        Vector2 direction = ForwardDirection;
        
        Vector2 wallOrigin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position + new Vector2(0, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallOrigin, wallOrigin + direction * wallCheckDistance);

        Vector2 ledgeOrigin = ledgeCheckPoint != null ? (Vector2)ledgeCheckPoint.position : (Vector2)transform.position + new Vector2(direction.x * 0.5f, -0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(ledgeOrigin, ledgeOrigin + Vector2.down * ledgeCheckDistance);
    }
}
