using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

  //=== State

  // Stores how much movement was applied in this frame
  float frameMovement;

  // Stores how much movement was applied last frame
  float lastFrameMovement;

  //=== Refs
  Rigidbody2D _rigidbody;

  private void Awake()
  {
    // Get components
    GetComponentRefs();
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

  private bool IsGrounded()
  {
    // Layer mask
    int layerMask = groundLayers & ~(1 << gameObject.layer);

    // Cast downwards
    return Physics2D.Raycast(feet.position, Vector2.down, 0.1f, layerMask).collider != null;
  }

  private void GetComponentRefs()
  {
    _rigidbody = GetComponent<Rigidbody2D>();
  }

  //=== Interface

  // Apply movement to character. The movementModifier param can alter the direction as well as the speed
  public void Move(float movementModifier)
  {
    // Apply inertia
    float currentMovement = _rigidbody.velocity.x;
    float incomingMovement = baseSpeed * movementModifier;
    float movement = (inertia * currentMovement) + ((1f - inertia) * incomingMovement);

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
  public void Jump()
  {
    // Ensure it's grounded
    if (!IsGrounded()) return;

    // Add y velocity
    _rigidbody.velocity = new Vector2(_rigidbody.velocity.y, jumpPower);
  }
}
