using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    // physics object, used to set gravity + velocity
    private Rigidbody2D rb;
	// box collider, snails hitbox
	private BoxCollider2D boxCollider;
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
    // raycast used to detect adjacent walls
    [SerializeField]
    private Transform wallCheck;
    // distance used to detect adjacent walls
    [SerializeField]
    private float wallCheckDistance;
    // offset used to adjust sprite when starting to wall climb
    [SerializeField]
    private float wallClimbOffset;
     // offset used to adjust sprite when ending wall climb (climb up ledge)
    [SerializeField]
    private float ledgeClimbOffset;
	// raycast used to detect slope angle
    [SerializeField]
    private Transform slopeCheck;
    // distance used to detect slope angle
    [SerializeField]
    private float slopeCheckDistance;
	private float slopeAngle;
	private float slopeAngleOld;
	private Vector2 slopeNormalPerpendicular;
    // global timer, currently unused
    private float globalTimer;

    //player vars
    private float direction;
    private float velocityX;
    private float velocityY;
    private bool isFacingRight = true;
    private bool inShell = true;
    private bool wallIsRight = true;
    private bool invertControls = false;

    // Enumeration type to keep track of if the player is moving on ground, walls or ceilings
    enum TerrainType {Ground, Wall, Ceiling};
    private TerrainType movingOn = TerrainType.Ground;

    // timer used to create higher jumps when holding
    private float jumpHoldTimer;
    // indicates whether the player is in the ascending phase of a jump
    private bool isJumping; 

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
        rb.gravityScale = PlayerConst.GRAVITY;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() {
        globalTimer += Time.deltaTime;
        UpdateDirection();
        UpdateVelocity();
        UpdateAnimations();
    }

    // updates the players horizontal direction and sprite orientation
    private void UpdateDirection() {
        // gets direction (-1. 0 or 1 on keyboard, any value between -1 and 1 on stick inputs)
        direction = Input.GetAxisRaw("Horizontal");
        // keep controls inverted until player changes direction (useful for ceiling movement with continous inputs)
        if (invertControls) {
            invertControls = direction != 0;
        // adjust sprite depending on directional input
        } else if (direction > 0 && !isFacingRight || direction < 0 && isFacingRight) {
            isFacingRight = !isFacingRight;
            transform.Rotate(0, 180, 0);
        }
    }
    
    // updates the players x and y velocity
    private void UpdateVelocity() {
        float acceleration = inShell ? PlayerConst.CRAWL_ACCEL_SHELL : PlayerConst.CRAWL_ACCEL;
        float deceleration = inShell ? PlayerConst.CRAWL_DECEL_SHELL : PlayerConst.CRAWL_DECEL;
        float maxSpeed = inShell ? PlayerConst.CRAWL_MAX_SPEED_SHELL : PlayerConst.CRAWL_MAX_SPEED;

        switch (movingOn) {
            // player is moving on the ground -> detect walls or adjust x velocity
            case TerrainType.Ground: {
                /*if (IsTouchingWall()) {
                    StartWallClimbing();
                } else {*/
					Vector2 checkSlopePos = transform.position;
					RaycastHit2D hit = Physics2D.Raycast(checkSlopePos, Vector2.down, slopeCheckDistance, groundLayer);
					slopeNormalPerpendicular = Vector2.Perpendicular(hit.normal).normalized;
					slopeAngle = Vector2.SignedAngle(hit.normal, Vector2.up);
					Debug.DrawRay(hit.point, slopeNormalPerpendicular, Color.red);
					Debug.DrawRay(hit.point, hit.normal, Color.green);
					if (slopeAngle != slopeAngleOld) {	
						transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, isFacingRight ? -slopeAngle : slopeAngle);
					}
					slopeAngleOld = slopeAngle;
                    velocityY = rb.velocity.y;
                    UpdateVelocityX(invertControls ? -acceleration : acceleration, deceleration, maxSpeed);
                //}
                break;
            }
            // player is moving on a wall -> detect ground or ceiling or adjust y velocity
            case TerrainType.Wall: {
                if (IsTouchingWall()) {
                    if (velocityY > 0) {
                        StartCeilingClimbing();
                    } else {
                        EndWallClimbingBottom();
                    }
                } else {
                     // player is fully touching the wall -> update y velocity depending on whether they're moving up or down
                    if (IsGrounded()) {          
                        UpdateVelocityY(invertControls ? -acceleration : acceleration, deceleration, maxSpeed);
                    // player reached the top of the wall, adjust rotation and position to end wall climbing
                    } else if (isFacingRight && wallIsRight || !isFacingRight && !wallIsRight) {
                        EndWallClimbingTop();
                    // avoids rotation issues if player reaches top of the wall with opposite directional input
                    } else {
                        velocityY = 0;
                    }
                }
                break;
            }
            // player is moving on the ceiling -> detect walls or adjust x velocity
            case TerrainType.Ceiling: {
                if (IsTouchingWall()) {
                    EndCeilingClimbingWall();
                } else if (IsGrounded()) {
                    velocityY = rb.velocity.y;
                    UpdateVelocityX(invertControls ? -acceleration : acceleration, deceleration, maxSpeed);
                } else {
                    EndCeilingClimbingFall();
                }
                break;
            }
        }

        // apply changes to velocity values
        rb.velocity = new Vector2(velocityX, velocityY);

         // reset key to make testing easier
        if (Input.GetKeyDown(KeyCode.F1)) {
            rb.velocity = new Vector2(0, 0);
            isFacingRight = true;
            inShell = true;
            transform.position = new Vector2(-12.06f, 2.37f);
            transform.rotation = Quaternion.identity;
            movingOn = TerrainType.Ground;
            rb.gravityScale = PlayerConst.GRAVITY;
            invertControls = false;
        }
    }

    // initiates wall climbing from the ground, stick to adjacent wall
    private void StartWallClimbing() {
        velocityY = Math.Abs(velocityX);
        velocityX = 0;
        wallIsRight = isFacingRight;
        transform.Rotate(0, 0, 90, Space.Self);
        transform.position = new Vector2(transform.position.x + (isFacingRight ? wallClimbOffset : -wallClimbOffset), transform.position.y);
        movingOn = TerrainType.Wall;
    }

    // ends wall climbing when reaching the bottom of the wall, stick to ground again
    private void EndWallClimbingBottom() {
        velocityX = isFacingRight ? -velocityY : velocityY;
        velocityY = 0;
        transform.Rotate(0, 0, 90, Space.Self);
        movingOn = TerrainType.Ground;
    }

    // ends wall climbing when reaching the top of the wall, move player up the ledge
    private void EndWallClimbingTop() {
        velocityX = isFacingRight ? velocityY : -velocityY;
        velocityY = 0;
        transform.Rotate(0, 0, -90, Space.Self);
        transform.position = new Vector2(transform.position.x + (isFacingRight ? ledgeClimbOffset : -ledgeClimbOffset), transform.position.y);
        movingOn = TerrainType.Ground;
    }

    // initiates ceiling climbing from a wall, stick to ceiling
    private void StartCeilingClimbing() {
        velocityX = isFacingRight ? -velocityY : velocityY;
        velocityY = 0;
        rb.gravityScale = -1;
        invertControls = invertControls ? false : direction != 0;
        transform.Rotate(0, 0, 90, Space.Self);
        transform.position = new Vector2(transform.position.x, transform.position.y + wallClimbOffset);
        isFacingRight = !isFacingRight;
        movingOn = TerrainType.Ceiling;
    }

    // ends ceiling climbing when reaching a wall, switch to wall climbing
    private void EndCeilingClimbingWall() {
        velocityY = -Math.Abs(velocityX);
        velocityX = 0;
        wallIsRight = isFacingRight;
        rb.gravityScale = PlayerConst.GRAVITY;
        invertControls = !invertControls;
        transform.Rotate(0, 0, 90, Space.Self);
        transform.position = new Vector2(transform.position.x + (isFacingRight ? wallClimbOffset : -wallClimbOffset), transform.position.y);
        isFacingRight = !isFacingRight;
        movingOn = TerrainType.Wall;
    }


    // ends ceiling climbing when reaching the end of the ceiling, player falls down
    private void EndCeilingClimbingFall() {
        rb.gravityScale = PlayerConst.GRAVITY;
        transform.Rotate(0, 0, 180, Space.Self);
        transform.Rotate(0, 180, 0);
        movingOn = TerrainType.Ground;
    }

    // updates y velocity when wall climbing
    private void UpdateVelocityY(float acceleration, float deceleration, float maxSpeed) {
        // adjust velocity depending on input direction and position of the wall, never exceeds max speed
        velocityY = Math.Max(Math.Min(velocityY + (wallIsRight ? direction : -direction) * acceleration * Time.deltaTime, maxSpeed), -maxSpeed);
        // no directional input -> decelarate
        if (direction == 0 && velocityY != 0) {
            velocityY = DeceleratedVelocity(velocityY, deceleration);
        }
    }

    // updates x velocity when moving on the ground or jumping
    private void UpdateVelocityX(float acceleration, float deceleration, float maxSpeed) {
        // adjust velocity depending on input direction, never exceeds max speed
        velocityX = Math.Max(Math.Min(velocityX + (direction * acceleration * Time.deltaTime), maxSpeed), -maxSpeed);
        // no directional input -> decelarate if grounded
        if (direction == 0 && IsGrounded() && velocityX != 0) {
            velocityX = DeceleratedVelocity(velocityX, deceleration);
        }
    }
   
    // calculate the decelerated velocity using the specified deceleration value
    private float DeceleratedVelocity(float velocity, float deceleration) {
        // avoids wiggeling
        if (Math.Abs(velocity) < 0.1) {
            return 0;
        } else if (velocity > 0) {
            return velocity - deceleration * Time.deltaTime;
        } else {
            return velocity + deceleration * Time.deltaTime;
        }
    }

    // handle jump input (WIP, not currently used)
    private void HandleJump() {
        if (Input.GetButtonDown("Jump") && IsGrounded() && !inShell && movingOn == TerrainType.Ground) {
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
    }

    // used to check if the player is touching the ground
    private bool IsGrounded() {
        // second box that checks if it overlaps with the ground
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
    }

    // used to check if the player is touching a wall
    private bool IsTouchingWall() {
        // line that extends to the side of the player to check collisions with walls
        return Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayer);
    }
    
    // updates the current animation state
    private void UpdateAnimations() {
        if (direction == 0f) {
            animator.SetBool("moving", false);
        } else {
            animator.SetBool("moving", true);
        }
    }

    // draw hitboxes in scene builder
    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + (isFacingRight ? wallCheckDistance : -wallCheckDistance), wallCheck.position.y, wallCheck.position.z));
		Gizmos.DrawLine(slopeCheck.position, new Vector3(slopeCheck.position.x, slopeCheck.position.y - slopeCheckDistance, slopeCheck.position.z));
    }
}
