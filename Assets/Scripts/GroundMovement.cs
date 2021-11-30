using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

// Component dependencies
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class GroundMovement : Movement
{
  //=== Params
  [Header("Jumping")]
  [Tooltip("Vertical velocity to add on jump")]
  [SerializeField] float jumpPower = 10f;

  [Tooltip("Which layers will the character be able to jump off of")]
  [SerializeField] LayerMask groundLayers;

  [Tooltip("The position of the character's feet")]
  [SerializeField] Transform feet;

  [Tooltip("Whether to change inertia value when airborne")]
  [SerializeField] bool airborneInertiaEffect = true;

  [Tooltip("Movement inertia when airborne")]
  [Range(0f, 1f)] [SerializeField] float airborneInertia = 0.7f;

  [Header("Climbing")]
  [Tooltip("Climb speed")]
  [SerializeField] float climbSpeed = 2f;

  [Tooltip("Which layers will the character be able to climb on")]
  [SerializeField] LayerMask climbableLayers;

  [Tooltip("The speed applied to character as he leaves a climbable object")]
  [SerializeField] float leaveClimbableImpulse = 5f;

  [Tooltip("If this collider isn't in contact with a climbable layer, character won't be able to climb up")]
  [SerializeField] Collider2D upperClimbBoundCollider;

  //=== State

  // Stores the initial values of gravityscale
  float defaultGravityScale;

  // Holds the default inertia
  float defaultInertia;

  // The climb coroutine
  Coroutine climbCoroutine;

  // The climb coroutine's move method
  Action<float> ClimbMove;


  protected override void OnAwake()
  {
    // Get gravity scale
    defaultGravityScale = _rigidbody.gravityScale;

    defaultInertia = inertia;
  }

  private void Update()
  {
    // Detect airborne
    DetectAirborne();
  }

  private void DetectAirborne()
  {
    // When airborne, increase inertia
    if (airborneInertiaEffect) inertia = IsGrounded(allowClimbing: true) ? defaultInertia : airborneInertia;
  }

  // Rotates movement to the sides and only then applies inertia
  protected override Vector2 ApplyInertia(Vector2 movement)
  {
    Vector2 rotatedMovement = movement.magnitude * GetFacingDirection() * Vector2.right;

    // Now we apply the inertia
    return base.ApplyInertia(rotatedMovement);
  }

  // Shifts the character's position to the first climbable layer in contact
  // Returns false if no climbable layer is reachable, true otherwise
  private bool HangToClimbable()
  {
    // Contact filter used to detect climbable layers
    ContactFilter2D contactFilter2D = new ContactFilter2D();
    contactFilter2D.NoFilter();
    contactFilter2D.SetLayerMask(climbableLayers);

    // Will hold the first climbable in contact with this object
    Collider2D[] climbables = new Collider2D[1];

    // Check for climbable contact
    if (_rigidbody.GetContacts(contactFilter2D, climbables) > 0)
    {
      // Shift character's position to the climbable object
      gameObject.transform.position = new Vector2(
        climbables[0].bounds.center.x,
        gameObject.transform.position.y
      );

      // Disable gravity
      _rigidbody.gravityScale = 0;

      // Announce success
      return true;
    }

    return false;
  }

  // Starts the climb coroutine
  private IEnumerator StartClimbing()
  {
    // Announce climbing
    SendMessage("OnStartClimbingMessage");

    // Will hold the climb movement for each frame
    float climbMovement = 0f;

    // Register climb move action
    ClimbMove = (float movementModifier) =>
    {
      // Check if has reached upper bound
      if (!upperClimbBoundCollider.IsTouchingLayers(climbableLayers))
      {
        // Since it did, forbid climbing up
        movementModifier = Mathf.Min(0f, movementModifier);
      }

      climbMovement = movementModifier * climbSpeed;
    };

    while (true)
    {
      // Slide up or down
      _rigidbody.velocity = new Vector2(0f, climbMovement);

      // Reset climb movement
      climbMovement = 0f;

      // Wait next frame
      yield return null;

      // Make sure still has contact to climbable
      if (!HangToClimbable()) StopClimbing();
    }
  }

  // Lets go of the climbable
  private void StopClimbing()
  {
    // Ignore redundant calls
    if (!IsClimbing()) return;

    // Announce climbing stopped
    SendMessage("OnStopClimbingMessage");

    // Reset gravity
    _rigidbody.gravityScale = defaultGravityScale;

    // Reset coroutine
    StopCoroutine(climbCoroutine);
    climbCoroutine = null;

    // Reset climb move method
    ClimbMove = null;
  }


  //=== Interface
  // Whether the character is standing on a ground layer
  public bool IsGrounded(bool allowClimbing = false)
  {
    if (allowClimbing && IsClimbing()) return true;

    // Layer mask
    int layerMask = groundLayers & ~(1 << gameObject.layer);

    // Cast downwards
    return Physics2D.Raycast(feet.position, Vector2.down, 0.1f, layerMask).collider != null;
  }

  // Apply movement to character. The movementModifier param can alter the direction as well as the speed
  protected override void MoveTowards(Vector2 movementWithInertia, Vector2 originalMovement)
  {
    // We only care about the magnitude and the facing direction here
    // float movement = Math.Sign(direction.x) * incomingMovement.magnitude;
    float movement = movementWithInertia.x;

    // If climbing, stop climbing, maximize & sum speed to leave impulse
    if (IsClimbing() && Mathf.Abs(movement) > 0.1f)
    {
      StopClimbing();
      movement *= leaveClimbableImpulse;
    }

    // Apply it
    _rigidbody.velocity = new Vector2(movement, _rigidbody.velocity.y);
  }

  // Jump method
  public void Jump(float powerModifier = 1f, bool skipGroundCheck = false)
  {
    // Ensure it's grounded or climbing
    if (!skipGroundCheck && !IsGrounded(allowClimbing: true)) return;

    // Let go if climbing
    StopClimbing();

    // Add y velocity
    _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpPower * powerModifier);
  }

  // Climb method
  public void Climb(float movementModifier)
  {
    // If not climbing, try to start
    if (!IsClimbing())
    {
      // Ensure that there's a climbable layer
      if (!HangToClimbable()) return;

      // Start coroutine
      climbCoroutine = StartCoroutine(StartClimbing());
    }

    // Move
    if (ClimbMove != null) ClimbMove(movementModifier);
  }

  // Checks if is climbing
  public bool IsClimbing() => climbCoroutine != null;
}
