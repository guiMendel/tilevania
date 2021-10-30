using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

// Component dependencies
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class GroundMovement : MonoBehaviour, MovementInterface
{
  //=== Params
  [Tooltip("Whether to invert the direction the character faces")]
  [SerializeField] bool invertFacingDirection;

  [Header("Movement")]
  [Tooltip("The base speed in which the character moves")]
  public float baseSpeed = 5f;

  [Tooltip("How much inertia affects changes to the direction")]
  [Range(0f, 1f)] public float inertia = 0f;

  [Header("Jumping")]
  [Tooltip("Vertical velocity to add on jump")]
  [SerializeField] float jumpPower = 10f;

  [Tooltip("Which layers will the character be able to jump off of")]
  [SerializeField] LayerMask groundLayers;

  [Tooltip("The position of the character's feet")]
  [SerializeField] Transform feet;

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

  // Stores how much movement was applied in this frame
  float frameMovement;

  // Stores how much movement was applied last frame
  float lastFrameMovement;

  // Stores the initial values of gravityscale
  float defaultGravityScale;

  // The climb coroutine
  Coroutine climbCoroutine;

  // The climb coroutine's move method
  Action<float> ClimbMove;

  //=== Refs
  Rigidbody2D _rigidbody;


  private void Awake()
  {
    // Get components
    GetComponentRefs();

    // Get gravity scale
    defaultGravityScale = _rigidbody.gravityScale;
  }

  private void LateUpdate()
  {
    lastFrameMovement = frameMovement;
    frameMovement = 0f;
  }

  private void UpdateFacingDirection(float movement)
  {
    // Ignore small movements
    if (Mathf.Abs(movement) > 0.1f)
    {
      // Keep facing direction updated
      SetFacingDirection(movement);
    }
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

    // Reset gravity
    _rigidbody.gravityScale = defaultGravityScale;

    // Reset coroutine
    StopCoroutine(climbCoroutine);
    climbCoroutine = null;

    // Reset climb move method
    ClimbMove = null;
  }

  private void GetComponentRefs()
  {
    _rigidbody = GetComponent<Rigidbody2D>();
  }

  //=== Interface
  // Whether the character is standing on a ground layer
  public bool IsGrounded()
  {
    // Layer mask
    int layerMask = groundLayers & ~(1 << gameObject.layer);

    // Cast downwards
    return Physics2D.Raycast(feet.position, Vector2.down, 0.1f, layerMask).collider != null;
  }

  // Apply movement to character. The movementModifier param can alter the direction as well as the speed
  public void Move(float movementModifier)
  {
    // Apply inertia
    float currentMovement = _rigidbody.velocity.x;
    float incomingMovement = baseSpeed * movementModifier;
    float movement = (inertia * currentMovement) + ((1f - inertia) * incomingMovement);

    // If climbing, stop climbing, maximize & sum speed to leave impulse
    if (IsClimbing() && Mathf.Abs(movement) > 0.1f)
    {
      StopClimbing();
      movement = Mathf.Max(movement, baseSpeed) + leaveClimbableImpulse * Mathf.Sign(movement);
    }

    // Apply it
    _rigidbody.velocity = new Vector2(movement, _rigidbody.velocity.y);

    // Adjust move direction
    UpdateFacingDirection(incomingMovement);

    // Record this movement
    frameMovement = incomingMovement;
  }

  // Informs how much movement was generated by the Move method in the last frame
  public float GetLastFrameMovement()
  {
    return lastFrameMovement;
  }

  // Update facing direction
  public void SetFacingDirection(float direction)
  {
    float directionModifier = invertFacingDirection ? -1 : 1;
    transform.localScale = new Vector2(Mathf.Sign(direction) * directionModifier, 1f);
  }

  // Jump method
  public void Jump(bool skipGroundCheck = false)
  {
    // Ensure it's grounded or climbing
    if (!skipGroundCheck && !IsGrounded() && !IsClimbing()) return;

    // Let go if climbing
    StopClimbing();

    // Add y velocity
    _rigidbody.velocity = new Vector2(_rigidbody.velocity.y, jumpPower);
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
