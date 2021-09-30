using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  // State
  [SerializeField] float moveSpeed = 3f;
  [SerializeField] float jumpSpeed = 15f;
  [SerializeField] float climbSpeed = 3f;

  // How much movement was applied in the current frame
  float frameMovement;

  // Whether player hit jump in this frame
  bool frameJump;

  // How much movement was put into climbing in this frame
  float frameClimb;

  // Whether the player is in climbing state
  bool climbing;

  // The initial gravity scale
  float gravityScale;

  // Refs
  Rigidbody2D _rigidBody;
  Animator _animator;
  Collider2D _collider2d;

  List<Component> refs;

  // Start is called before the first frame update
  void Start()
  {
    GetComponentRefs();

    // Store gravity scale
    gravityScale = _rigidBody.gravityScale;
  }

  // Update is called once per frame
  void Update()
  {
    // Get the frame's movement
    frameMovement = Input.GetAxis("Horizontal") * moveSpeed;

    // Keep frameJump if it's already set to true
    frameJump = frameJump || Input.GetButtonDown("Jump");

    // Detect climbing
    frameClimb = Input.GetAxis("Vertical") * climbSpeed;
  }

  private void FixedUpdate()
  {
    // Apply frame's movement
    Move();
    StartJump();
    Climb();

    // Detect if player is rising in the air / falling
    DetectAirborne();

    // Flip sprite
    FlipSprite();
  }

  private void DetectAirborne()
  {
    // If he's climbing or grounded, he isn't airborne
    if (!IsTouching("Ground") && !climbing)
    {
      // Detect y movement
      float yMovement = _rigidBody.velocity.y;

      // Detect rising
      if (yMovement > Mathf.Epsilon)
      {
        _animator.SetInteger("AirborneDirection", 1);
        return;
      }
      // Detect falling
      else if (yMovement < -Mathf.Epsilon)
      {
        _animator.SetInteger("AirborneDirection", -1);
        return;
      }
    }

    // Not airborne
    _animator.SetInteger("AirborneDirection", 0);
  }

  private void Climb()
  {
    // If not touching climbable, shouldn't be climbing
    if (!IsTouching("Climbable"))
    {
      if (climbing) StopClimbing();
      return;
    }

    // If player is actively climbing
    if (Mathf.Abs(frameClimb) > Mathf.Epsilon)
    {
      // Ensure climbing state
      climbing = true;

      // Apply movement to body
      _rigidBody.velocity = new Vector2(0, frameClimb);

      // Disable gravity
      _rigidBody.gravityScale = 0f;

      // Update animation
      _animator.SetBool("Climbing", true);
      // This one makes the animation freeze when false
      _animator.SetFloat("ClimbingSpeedMultiplier", 1f);

    }
    // If player is in climbing state but isn't going up or down, freeze animation
    else if (climbing)
    {
      // Freeze y movement
      _rigidBody.velocity = new Vector2(0, 0);

      // Freeze animation
      _animator.SetFloat("ClimbingSpeedMultiplier", 0f);
    }
  }

  // Exits climbing state
  void StopClimbing()
  {
    climbing = false;
    // Update animation
    _animator.SetBool("Climbing", false);
    _animator.SetFloat("ClimbingSpeedMultiplier", 0f);

    // Reset gravity
    _rigidBody.gravityScale = gravityScale;
  }

  // Starts jump animation
  void StartJump()
  {
    // Make sure it's reset
    bool jump = frameJump;
    frameJump = false;

    // Can only jump in these 2 conditions
    if (!IsTouching("Ground") && !IsTouching("Climbable")) return;

    if (jump)
    {
      // Set the jump param
      _animator.SetTrigger("Jump");
    }
  }

  // Actually performs the jump
  public void Jump()
  {
    _rigidBody.velocity = new Vector2(_rigidBody.velocity.x, jumpSpeed);

    // If climbing, stop climbing
    if (climbing) StopClimbing();
  }

  // Whether or not the player is currently touching the layer
  bool IsTouching(String layer)
  {
    // Get layer
    LayerMask layerMask = LayerMask.GetMask(layer);

    // See if player is touching it
    return _collider2d.IsTouchingLayers(layerMask);
  }

  // Applies horizontal movement to the body
  void Move()
  {
    // Update animation
    bool isMoving = Mathf.Abs(frameMovement) > Mathf.Epsilon;
    _animator.SetBool("Walking", isMoving);

    // Forbid moving while climbing
    if (climbing) return;

    // Apply movement to body
    _rigidBody.velocity = new Vector2(frameMovement, _rigidBody.velocity.y);
  }

  // Flips sprite x scale based on horizontal movement
  void FlipSprite()
  {
    // Checks for h. movement
    float movement = _rigidBody.velocity.x;

    if (Mathf.Abs(movement) > Mathf.Epsilon)
    {
      // Flip sprite
      transform.localScale = new Vector2(Mathf.Sign(movement), 1f);
    }
  }

  private void GetComponentRefs()
  {
    // Get refs
    _rigidBody = GetComponent<Rigidbody2D>();
    _animator = GetComponent<Animator>();
    _collider2d = GetComponent<Collider2D>();

    // Report dependencies
    if (_rigidBody == null)
    {
      Debug.LogError(gameObject.name + " is missing " + _rigidBody.GetType().Name + " component");
    }
    if (_animator == null)
    {
      Debug.LogError(gameObject.name + " is missing " + _animator.GetType().Name + " component");
    }
    if (_collider2d == null)
    {
      Debug.LogError(gameObject.name + " is missing " + _collider2d.GetType().Name + " component");
    }
  }
}
