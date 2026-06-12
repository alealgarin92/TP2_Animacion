using System;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    Animator animator;

    [SerializeField]
    private int _maxHealth = 100;

    public int MaxHealth
    {
        get 
        {
            return _maxHealth;
        }
        set 
        {
            _maxHealth = value;
        }
    }

    [SerializeField]
    private int _health = 100;

    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;

            if(_health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    [SerializeField]
    private bool _isAlive = true;

    [SerializeField]
    private bool isInvincible = false;
    private float timeSinceHit = 0;
    public float invincibilityTime = 0.25f;

    // C# Events
    public event Action<int> OnHit;
    public event Action OnDeath;

    public bool IsAlive 
    { 
        get 
        { 
            return _isAlive;
        }
        private set 
        { 
            _isAlive = value;
            animator.SetBool(AnimationsStrings.isAlive, value);
            Debug.Log("IsAlive set " + value);
            if (!_isAlive)
            {
                OnDeath?.Invoke();
            }
        } 
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        Health = MaxHealth;
    }

    private void Update()
    {
        if (isInvincible) 
        {
            if (timeSinceHit > invincibilityTime)
            {
                // Remove invincibility
                isInvincible = false;
                timeSinceHit = 0;
            }

            timeSinceHit += Time.deltaTime;
        }
    }

    public void Hit(int damage)
    {
        if (_isAlive && !isInvincible) 
        {
            Health -= damage;
            isInvincible = true;
            OnHit?.Invoke(damage);
        }
    }
    
}
