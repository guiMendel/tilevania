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
  [Header("Idle movement settings")]
  [SerializeField] float moveDurationMin = 0.5f;
  [SerializeField] float moveDurationMax = 2f;
  [SerializeField] float idleDurationMin = 1f;
  [SerializeField] float idleDurationMax = 5f;

  // State

  // It's current movement direction
  Quaternion movementDirection;

  // Whether is moving or not
  bool moving;

  // Refs
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
    if (isCurrentState) movementComponent.Move(
      moving
        ? (Vector2)(movementDirection * (Vector2.right))
        : Vector2.zero
      );
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

    Disable();
  }

  private void WanderNextIteration()
  {
    // Change movement
    moving = !moving;

    // Change direction
    if (moving) ChangeDirection();
  }

  private void Disable()
  {
    // When done, ensure movement has stopped
    moving = false;

    // Erase movement
    movementComponent.Move(Vector2.zero);
  }

  // Sets this as the current state and starts emitting events
  protected override void OnStateEnable()
  {
    // Start coroutine
    StartCoroutine(Wander());
  }

  protected override string GetDefaultStateKeyName() => "movement";

  // Interface

  // Switches movement direction
  public void ChangeDirection()
  {
    if (!isCurrentState) return;

    // Pick a random angle to move in
    float moveAngle = Random.Range(0, 360);

    // Register this direction
    movementDirection = Quaternion.Euler(0, 0, moveAngle);
  }
}
