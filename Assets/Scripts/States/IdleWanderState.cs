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

  // It's current movement
  float movement = 0f;

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
    if (isCurrentState) movementComponent.Move(Vector2.right * movement);
  }

  private IEnumerator Wander()
  {
    // Loop until state changes
    while (isCurrentState)
    {
      // Perform wandering action
      WanderNextIteration();

      // Decide when to change state
      float min = movement != 0f ? moveDurationMin : idleDurationMin;
      float max = movement != 0f ? moveDurationMax : idleDurationMax;

      float stateChangeTimeout = Random.Range(min, max);

      // Wait this time
      yield return new WaitForSeconds(stateChangeTimeout);
    }

    Disable();
  }

  private void WanderNextIteration()
  {
    // Change movement
    movement = movement == 0f ? 1f : 0f;

    // Randomly switch direction
    if (Random.value < 0.5f) FlipMovementDirection();
  }

  private void Disable()
  {
    // When done, ensure movement has stopped
    movement = 0f;

    // Erase movement
    movementComponent.Move(Vector2.right * movement);
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
  public void FlipMovementDirection()
  {
    if (!isCurrentState) return;

    movement = -movement;
    // if (movement != 0) movementComponent.SetFacingDirection(movement);
  }
}
