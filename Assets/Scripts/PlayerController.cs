using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float airWalkSpeed = 3f;
    public float jumpImpulse = 10f;
    Vector2 moveInput;
    TouchingDirections touchingDirections;
    Damageable damageable;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 0.5f;
    private SpriteRenderer _spriteRenderer;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.8f;
    public float attackDamage = 10f;

    public GameObject gameOverPanel;
    private Vector2 _spawnPosition;

    // Backwards compatibility properties for HealthUI and other scripts
    public float maxHealth => damageable != null ? damageable.MaxHealth : 100f;
    public float currentHealth => damageable != null ? damageable.Health : 100f;

    public float CurrentMoveSpeed
    {
        get
        {
            if (CanMove)
            {
                if (IsMoving && !touchingDirections.IsOnWall)
                {
                    if (touchingDirections.IsGrounded)
                    {
                        if (IsRunning)
                        {
                            return runSpeed;
                        }
                        else
                        {
                            return walkSpeed;
                        }
                    }
                    else
                    {
                        return airWalkSpeed;
                    }

                }
                else
                {
                    return 0;
                }
            }
            else
            {
                // Movement locked
                return 0;
            }
            
            
        }
    }

    [SerializeField]
    private bool _isMoving = false;
    public bool IsMoving { get 
        { 
            return _isMoving;
        } private set 
        {
            _isMoving = value;
            animator.SetBool(AnimationsStrings.isMoving, value);
        } 
    }

    [SerializeField]
    private bool _isRunnig = false;

    public bool IsRunning
    {
        get
        {
            return _isRunnig;
        }
        private set
        {
            _isRunnig = value;
            animator.SetBool(AnimationsStrings.isRunning, value);
        }
    }

    public bool _isFacingRight = true;

    public bool isFacingRight 
    { 
        get 
        { 
            return _isFacingRight; 
        }
        private set 
        {
            if (_isFacingRight != value)
            {
                //Flip the local scale to make the player face the opposite direction
                transform.localScale *= new Vector2(-1, 1);
            }
            _isFacingRight = value; 
        }
    }

    public bool CanMove
    {
        get
        {
            return IsAlive && animator.GetBool(AnimationsStrings.canMove);
        }
    }

    public bool IsAlive
    {
        get
        {
            return damageable != null ? damageable.IsAlive : false;
        }
    }

    Rigidbody2D rb;
    Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        damageable = GetComponent<Damageable>();
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.OnDeath += Die;
            damageable.OnHit += OnPlayerHit;
        }
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.OnDeath -= Die;
            damageable.OnHit -= OnPlayerHit;
        }
    }

    private void Start()
    {
        _spawnPosition = transform.position;
    }

    private void OnPlayerHit(int damage)
    {
        StartCoroutine(DamageFlashRoutine());
    }

    private void Die()
    {
        StopAllCoroutines(); // Stop flashing

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.white; // Restore color
        }

        Debug.Log("[Player] Died!");
        animator.SetTrigger(AnimationsStrings.death);
        
        // Stop player velocity
        rb.linearVelocity = Vector2.zero;
        
        // Show Game Over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        Color originalColor = _spriteRenderer.color;
        Color flashColor = new Color(1f, 0.3f, 0.3f, 0.8f); // Red flash

        float elapsed = 0f;
        float flashInterval = 0.1f;
        bool isFlashing = false;

        float duration = damageable != null ? damageable.invincibilityTime : invincibilityDuration;

        while (elapsed < duration)
        {
            _spriteRenderer.color = isFlashing ? originalColor : flashColor;
            isFlashing = !isFlashing;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        _spriteRenderer.color = originalColor;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2 (moveInput.x * CurrentMoveSpeed, rb.linearVelocity.y);

        animator.SetFloat(AnimationsStrings.yVelocity, rb.linearVelocity.y);
    }

    public void OnMove (InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (IsAlive)
        {
            IsMoving = moveInput != Vector2.zero;

            SetFacingDirection(moveInput);
        }
        else
        {
            IsMoving = false;
        }
    }

    private void SetFacingDirection(Vector2 moveInput)
    {
        if(moveInput.x > 0 && !isFacingRight)
        {
            // Face the Rigth
            isFacingRight = true;
        }
        else if(moveInput.x < 0 && isFacingRight)
        {
            // Face the left
            isFacingRight = false;
        }
    }

    public void OnRun(InputAction.CallbackContext context) 
    {
        if (!IsAlive)
        {
            IsRunning = false;
            return;
        }
        if (context.started)
        {
            IsRunning = true;
        }
        else if (context.canceled) 
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsAlive) return;
        
        if (context.started && touchingDirections.IsGrounded && CanMove)
        {
            animator.SetTrigger(AnimationsStrings.jumpTrigger);
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulse);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!IsAlive) return;
        if (context.started)
        {
            animator.SetTrigger(AnimationsStrings.attackTrigger);
            DealDamage();
        }
    }

    private void DealDamage()
    {
        // Calculate attack position in front of the player
        Vector2 attackCenter = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position + new Vector2(isFacingRight ? 1f : -1f, 0f);
        
        // Find all colliders in the attack circle
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackCenter, attackRange);
        
        foreach (var col in hitColliders)
        {
            // Do not damage ourselves
            if (col.gameObject == gameObject) continue;

            Damageable targetDamageable = col.GetComponent<Damageable>();
            if (targetDamageable != null)
            {
                targetDamageable.Hit((int)attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 attackCenter = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position + new Vector2(isFacingRight ? 1f : -1f, 0f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCenter, attackRange);
    }
}
