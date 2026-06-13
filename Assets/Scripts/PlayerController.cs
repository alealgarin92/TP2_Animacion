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

    [Header("Idle Speed Settings")]
    public float idleSpeedBoostDuration = 3f;
    private float _idleSpeedCooldownTimer = 0f;
    private bool _wasRunningOrAttacking = false;
    private bool _hasIdleSpeedMultiplierParam = false;
    private bool _paramChecked = false;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 0.5f;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _damageFlashCoroutine;
    private Color _originalColor;

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
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
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
        if (_damageFlashCoroutine != null)
        {
            StopCoroutine(_damageFlashCoroutine);
        }
        _damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private void Die()
    {
        if (_damageFlashCoroutine != null)
        {
            StopCoroutine(_damageFlashCoroutine);
            _damageFlashCoroutine = null;
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor; // Restore color
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
        Color flashColor = new Color(1f, 0.3f, 0.3f, 0.8f); // Red flash

        float elapsed = 0f;
        float flashInterval = 0.1f;
        bool isFlashing = false;

        float duration = damageable != null ? damageable.invincibilityTime : invincibilityDuration;

        while (elapsed < duration)
        {
            _spriteRenderer.color = isFlashing ? _originalColor : flashColor;
            isFlashing = !isFlashing;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        _spriteRenderer.color = _originalColor;
        _damageFlashCoroutine = null;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2 (moveInput.x * CurrentMoveSpeed, rb.linearVelocity.y);

        animator.SetFloat(AnimationsStrings.yVelocity, rb.linearVelocity.y);
    }

    private void Update()
    {
        if (!IsAlive) return;

        // Detect if player is running or attacking
        if (IsRunning && IsMoving && touchingDirections.IsGrounded)
        {
            _wasRunningOrAttacking = true;
        }
        else
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            int stateHash = stateInfo.shortNameHash;
            bool isAttackingState = stateHash == Animator.StringToHash("player_attack_1") || 
                                   stateHash == Animator.StringToHash("player_bow") ||
                                   stateHash == Animator.StringToHash("player_attack_2");
            
            if (isAttackingState)
            {
                _wasRunningOrAttacking = true;
            }
        }

        // Check if player is currently in idle state using shortNameHash for robust sub-state machine detection
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        bool isIdle = currentState.shortNameHash == Animator.StringToHash("player_idle");

        if (isIdle)
        {
            if (_wasRunningOrAttacking)
            {
                // Trigger speed 1.0 idle animation for configured duration
                _idleSpeedCooldownTimer = idleSpeedBoostDuration;
                _wasRunningOrAttacking = false;
            }

            if (_idleSpeedCooldownTimer > 0f)
            {
                _idleSpeedCooldownTimer -= Time.deltaTime;
                SetIdleSpeedMultiplier(1.0f);
            }
            else
            {
                SetIdleSpeedMultiplier(0.5f);
            }
        }
        else
        {
            // Reset parameter to default when not in idle to keep it clean
            SetIdleSpeedMultiplier(0.5f);
        }
    }

    private void SetIdleSpeedMultiplier(float value)
    {
        if (animator == null) return;

        if (!_paramChecked)
        {
            for (int i = 0; i < animator.parameterCount; i++)
            {
                if (animator.parameters[i].name == "idleSpeedMultiplier")
                {
                    _hasIdleSpeedMultiplierParam = true;
                    break;
                }
            }
            _paramChecked = true;
        }

        if (_hasIdleSpeedMultiplierParam)
        {
            animator.SetFloat("idleSpeedMultiplier", value);
        }
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

    public void OnRangedAttack(InputAction.CallbackContext context)
    {
        if (!IsAlive) return;
        if (context.started)
        {
            animator.SetTrigger(AnimationsStrings.RangedAttackTrigger);
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
