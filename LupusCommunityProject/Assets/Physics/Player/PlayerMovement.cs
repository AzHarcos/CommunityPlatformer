using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    // physics object, used to set gravity + velocity
    private Rigidbody2D rb;
    // animation object, used to set animation flags
    private Animator animator;
    // sprite object, used to adjust sprite
    private SpriteRenderer spriteRenderer;
    // secondary hitbox for collision detection
    [SerializeField]
    private Transform groundCheck;
    // ground layer, everything you can jump of
    [SerializeField]
    private LayerMask groundLayer;
    // size of rectangle used to check collision
    [SerializeField]
    private Vector2 groundCheckSize;
    // global timer, currently unused
    private float globalTimer;

    //player vars
    private float direction;
    private float velocityX;
    private float velocityY;
    private bool isFacingRight = true;
    private bool inShell = true;

    // timer used to create higher jumps when holding
    private float jumpHoldTimer;
    // indicates whether the player is in the ascending phase of a jump
    private bool isJumping; 

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = PlayerConst.GRAVITY;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        globalTimer += Time.deltaTime;
        // gets direction (-1. 0 or 1 on keyboard, any value between -1 and 1 on stick inputs)
        direction = Input.GetAxisRaw("Horizontal");
        UpdateVelocity();
        UpdateAnimations();
    }

    // updates the players x and y velocity
    private void UpdateVelocity() {
        float acceleration = inShell ? PlayerConst.CRAWL_ACCEL_SHELL : PlayerConst.CRAWL_ACCEL;
        float deceleration = inShell ? PlayerConst.CRAWL_DECEL_SHELL : PlayerConst.CRAWL_DECEL;
        float maxSpeed = inShell ? PlayerConst.CRAWL_MAX_SPEED_SHELL : PlayerConst.CRAWL_MAX_SPEED;

        // velocity increases and isn't set
        velocityX = Math.Max(Math.Min(velocityX + (direction * acceleration * Time.deltaTime), maxSpeed), -maxSpeed);
        
        if (direction == 0 && IsGrounded() && velocityX != 0) {
            if (velocityX > 0) {
                velocityX -= deceleration * Time.deltaTime;
                //avoid wiggeling
                if (velocityX < 0.1)
                    velocityX = 0;
            } else if (velocityX < 0) {
                velocityX += deceleration * Time.deltaTime;
                //avoids wiggeling
                if (velocityX > 0.1)
                    velocityX = 0;
            }
        }

        velocityY = rb.velocity.y;

        if (Input.GetButtonDown("Jump") && IsGrounded() && !inShell) {
            isJumping = true;
        }

        if (isJumping) {
            jumpHoldTimer += Time.deltaTime;
            velocityY = PlayerConst.JUMP_ACCEL;
        }

        if (Input.GetButtonUp("Jump") || jumpHoldTimer >= PlayerConst.MAX_JUMP_HOLD_TIME) {
            isJumping = false;
            jumpHoldTimer = 0;
        }

        rb.velocity = new Vector2(velocityX, velocityY);
    }
   
    // used to check if the player is touching the ground
    private bool IsGrounded() {
        // second box that checks if it overlaps with the ground
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
    }
    
    // updates the current animation state
    private void UpdateAnimations() {
        if (direction == 0f) {
            animator.SetBool("moving", false);
        } else {
            if (direction > 0 && !isFacingRight || direction < 0 && isFacingRight) {
                isFacingRight = !isFacingRight;
                transform.Rotate(0, 180f, 0);
            }
            animator.SetBool("moving", true);
        }
    }

    // draw hitboxes in scene builder
    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
