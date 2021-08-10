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
    public Transform groundCheck;
    // ground layer, everything you can jump of
    public LayerMask groundLayer;
    // radius used to check collision, value is set in Unity
    public float groundCheckRadius;

    // global timer, currently unused
    private float globalTimer;

    //player vars
    private float velocityX;


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
        float direction = Input.GetAxisRaw("Horizontal");
        UpdateVelocity(direction);
        UpdateAnimations(direction);
    }

    // updates the players x and y velocity
    private void UpdateVelocity(float direction) {

        
        // velocity increases and isn't set
        velocityX = Math.Max(Math.Min(velocityX + (direction * PlayerConst.SNEAKING_ACCEL * Time.deltaTime), PlayerConst.SNEAKING_MAX_SPEED), -PlayerConst.SNEAKING_MAX_SPEED);
        
        if (direction == 0 && IsGrounded() && velocityX != 0) {
            if (velocityX > 0) {
                velocityX -= PlayerConst.SNEAKING_DECEL * Time.deltaTime;
                //avoid wiggeling
                if (velocityX < 0.1)
                    velocityX = 0;
            } else if (velocityX < 0) {
                velocityX += PlayerConst.SNEAKING_DECEL * Time.deltaTime;
                //avoids wiggeling
                if (velocityX > 0.1)
                    velocityX = 0;
            }
        }

        float velocityY = rb.velocity.y;


        if (Input.GetButtonDown("Jump") && IsGrounded()) {
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
        //TODO: change to OverlapBox
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) && rb.velocity.y <= 0.01f;
    }
    
    // updates the current animation state
    private void UpdateAnimations(float direction) {
        if (direction == 0f) {
            animator.SetBool("moving", false);
        } else {
            animator.SetBool("moving", true);
            spriteRenderer.flipX = direction < 0f;
        }
    }

    // draw hitbox sphere in scene builder
    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
