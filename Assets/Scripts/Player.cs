using System;
using Diagnostics = System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  // State
  [SerializeField] float moveSpeed = 3f;
  [SerializeField] float jumpSpeed = 15f;
  [SerializeField] float climbSpeed = 3f;
  [Tooltip("How much time can pass between the two key presses of a dash, in milliseconds")]
  [SerializeField] int dashTolerance = 500;
  [Tooltip("How much the walking speed gets multiplied by when player is dashing")]
  [SerializeField] float dashSpeedMultiplier = 2f;

  // How much movement was applied in the current frame
  float frameMovement;

  // Whether player hit jump in this frame
  bool frameJump;

  // How much movement was put into climbing in this frame
  float frameClimb;

  // Whether the player is in climbing state
  bool climbing;

  // Whether the player is sprinting
  bool sprinting;

  // The initial gravity scale
  float gravityScale;

  // Refs
  Rigidbody2D _rigidBody;
  Animator _animator;
  Collider2D _collider2d;
  Collider2D feetCollider;

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
    DetectMovement();

    // Keep frameJump if it's already set to true
    frameJump = frameJump || Input.GetButtonDown("Jump");

    // Detect climbing
    frameClimb = Input.GetAxis("Vertical") * climbSpeed;
  }

  private void DetectMovement()
  {
    // Get the frame's movement
    frameMovement = Input.GetAxis("Horizontal") * moveSpeed;

    // Detect double click (dash)
    HandleDash();
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

  // Checks if the player has pressed the same move kew twice in a quick succession to perform a dash
  private void HandleDash()
  {
    // If stopped moving, make sure sprinting turns false
    bool isMoving = Mathf.Abs(frameMovement) > Mathf.Epsilon;
    if (!isMoving)
    {
      ToggleSprinting(false);
      return;
    }

    // If already sprinting, apply bonus speed
    if (sprinting)
    {
      frameMovement *= dashSpeedMultiplier;
      return;
    }

    // Check if a movement key was pressed in this frame. If not, do nothing
    if (!Input.GetButtonDown("Horizontal")) return;

    // Detect direction
    int direction = (int)Mathf.Sign(Input.GetAxisRaw("Horizontal"));

    // Start a coroutine that will wait for the double tap for some time
    StartCoroutine(DetectDoubleKey(direction));
  }

  private void ToggleSprinting(bool value)
  {
    sprinting = value;
    _animator.SetBool("Sprinting", value);
  }

  IEnumerator DetectDoubleKey(int direction)
  {
    // Start counting live time
    Diagnostics.Stopwatch liveTimeCounter = Diagnostics.Stopwatch.StartNew();

    // Keep waiting
    while (true)
    {
      // Wait next frame
      yield return null;

      // Check for the double tap
      if (Input.GetButtonDown("Horizontal"))
      {
        int doubleDirection = (int)Mathf.Sign(Input.GetAxisRaw("Horizontal"));

        // Check if directions match
        if (direction == doubleDirection) ToggleSprinting(true);

        // Stop coroutine after second tap
        yield break;
      }

      // Check live time. If timer is due has passed, die
      if (liveTimeCounter.ElapsedMilliseconds > dashTolerance) yield break;
    }
  }

  private void DetectAirborne()
  {
    // If he's climbing or grounded, he isn't airborne
    if (!IsTouching("Ground", withFeet: true) && !climbing)
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
    if (!IsTouching("Ground", withFeet: true) && !IsTouching("Climbable")) return;

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
  bool IsTouching(String layer, bool withFeet = false)
  {
    // Get layer
    LayerMask layerMask = LayerMask.GetMask(layer);

    // Get which collider to use
    Collider2D collider = withFeet ? feetCollider : _collider2d;

    // See if player is touching it
    return collider.IsTouchingLayers(layerMask);
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
    feetCollider = transform.Find("Feet").GetComponent<Collider2D>();

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
    if (feetCollider == null)
    {
      Debug.LogError(gameObject.name + " is missing Feet child with rigidbody2D component");
    }
  }
}
