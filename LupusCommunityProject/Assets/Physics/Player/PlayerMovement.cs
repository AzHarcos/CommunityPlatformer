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

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = PlayerConst.GRAVITY;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        // gets direction (-1. 0 or 1 on keyboard, any value between -1 and 1 on stick inputs)
        float direction = Input.GetAxisRaw("Horizontal");
        UpdateVelocity(direction);
        UpdateAnimations(direction);
    }

    // updates the players x and y velocity
    private void UpdateVelocity(float direction) {
        float velocityX = direction * PlayerConst.SNEAKING_ACCEL;
        float velocityY = rb.velocity.y;
        if (Input.GetButtonDown("Jump")) {
            velocityY = PlayerConst.JUMP_ACCEL;
        }
        rb.velocity = new Vector2(velocityX, velocityY);
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
}
