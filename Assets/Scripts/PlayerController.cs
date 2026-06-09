using System;
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
            return animator.GetBool(AnimationsStrings.canMove);
        }
    }

    Rigidbody2D rb;
    Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2 (moveInput.x * CurrentMoveSpeed, rb.linearVelocity.y);

        animator.SetFloat(AnimationsStrings.yVelocity, rb.linearVelocity.y);
    }

    public void OnMove (InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput != Vector2.zero;

        SetFacingDirection(moveInput);
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
        // TODO check if alive as well
        if (context.started && touchingDirections.IsGrounded)
        {
            animator.SetTrigger(AnimationsStrings.jumpTrigger);
            
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulse);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            animator.SetTrigger(AnimationsStrings.attackTrigger);
        }
    }
}
