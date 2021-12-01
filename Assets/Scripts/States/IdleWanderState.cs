using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class IdleWanderState : State
{
  //=== Params
  [Header("Wander settings")]
  [SerializeField] float moveDurationMin = 0.5f;
  [SerializeField] float moveDurationMax = 2f;
  [SerializeField] float idleDurationMin = 1f;
  [SerializeField] float idleDurationMax = 5f;

  [Header("Direction flipping")]
  [Tooltip("When the character is at most this distance away from a collidable, it will flip it's direction")]
  [SerializeField] float flipDirectionRange = 0.3f;
  [Tooltip("These are the layers that will cause the character to flip")]
  [SerializeField] LayerMask flipDirectionLayers;
  [Tooltip("The angle variation to the new direction after the flip. If set to 0, character will always move the opposite direction when flipping")]
  [Range(0, 180)] [SerializeField] int flipDirectionAngleVariation = 90;
  [Tooltip("How often to perform the wall check, in seconds")]
  [SerializeField] float wallCheckFrequency = 0.1f;


  //=== State

  // It's current movement direction
  Quaternion movementDirection;

  // Whether is moving or not
  bool moving;

  //=== Refs
  Movement movementComponent;

  protected override void OnAwake()
  {
    // Get refs
    movementComponent = GetComponent<Movement>();

    if (movementComponent == null)
    {
      Debug.LogError(gameObject.name + " is missing " + movementComponent.GetType().Name + " component");
    }
  }

  protected override void OnUpdate()
  {
    // Emit move event
    if (!isCurrentState) return;

    // Move
    if (moving)
    {
      movementComponent.Move(movementDirection * Vector2.right);
    }
    else movementComponent.Move(Vector2.zero);
  }

  // Sets this as the current state and starts emitting events
  protected override void OnStateEnable()
  {
    // Start coroutines
    StartCoroutine(Wander());

    StartCoroutine(DetectWallsCoroutine());
  }

  // Stop all coroutines on disable
  protected override void OnStateDisable()
  {
    StopAllCoroutines();

    // When done, ensure movement has stopped
    moving = false;

    // Erase movement
    movementComponent.Move(Vector2.zero);

  }

  private IEnumerator DetectWallsCoroutine()
  {
    while (isCurrentState)
    {
      // Check for walls ahead
      DetectWalls();

      // Wait some time
      yield return new WaitForSeconds(wallCheckFrequency);
    }
  }

  // Detects collidables ahead. If any are detected, flips movement direction
  private void DetectWalls()
  {
    Collider2D collider = GetComponent<Collider2D>();
    if (collider)
    {
      // Get contact filter
      ContactFilter2D contactFilter2D = new ContactFilter2D().NoFilter();
      contactFilter2D.SetLayerMask(flipDirectionLayers);

      bool colliderAhead = collider.Cast(
        movementComponent.GetLastFrameMovement(), contactFilter2D, new RaycastHit2D[1], flipDirectionRange
      ) > 0;

      if (colliderAhead) FlipDirection();
    }
  }

  private IEnumerator Wander()
  {
    // Loop until state changes
    while (isCurrentState)
    {
      // Perform wandering action
      WanderNextIteration();

      // Decide when to change state
      float min = moving ? moveDurationMin : idleDurationMin;
      float max = moving ? moveDurationMax : idleDurationMax;

      float stateChangeTimeout = Random.Range(min, max);

      // Wait this time
      yield return new WaitForSeconds(stateChangeTimeout);
    }
  }

  private void WanderNextIteration()
  {
    // Change movement
    moving = !moving;

    // Change direction
    if (moving) ChangeDirection();
  }

  protected override string GetDefaultStateKeyName() => "movement";

  //=== Gizmos

  private void OnDrawGizmosSelected()
  {
    if (movementComponent == null) return;

    Gizmos.DrawLine(
      transform.position,
      transform.position + (Vector3)movementComponent.GetLastFrameMovement().normalized * (flipDirectionRange + 0.5f)
    );
  }

  //=== Interface

  // Switches movement direction
  public void ChangeDirection()
  {
    if (!isCurrentState) return;

    // Pick a random angle to move in
    float moveAngle = Random.Range(0, 360);

    // Register this direction
    movementDirection = Quaternion.Euler(0, 0, moveAngle);
  }

  public void FlipDirection()
  {
    // Get movement direction
    float movementAngle =
      // Arccosine of adjacent side
      Mathf.Acos(movementComponent.GetLastFrameMovement().normalized.x)
      // From radian to degree
      * Mathf.Rad2Deg
      // Restore angle sign, which is dropped from arccosine
      * Mathf.Sign(movementComponent.GetLastFrameMovement().y);

    // Get the opposite direction to the current one
    int oppositeDirectionAngle = ((int)movementAngle + 180) % 360;

    // Get an angle variation
    int angleVariation = flipDirectionAngleVariation > 0
      ? Random.Range(-flipDirectionAngleVariation, flipDirectionAngleVariation)
      : 0;

    // Apply the new direction
    movementDirection = Quaternion.Euler(0, 0, oppositeDirectionAngle + angleVariation);
  }
}
