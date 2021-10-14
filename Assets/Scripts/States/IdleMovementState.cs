using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

// TODO que tal fazer uma classe abstrata State que ja implementa esse enable e disable e fornece o isCurrentState

public class IdleMovementState : State
{
  [Header("Idle movement settings")]
  [SerializeField] float moveDurationMin = 0.5f;
  [SerializeField] float moveDurationMax = 2f;
  [SerializeField] float idleDurationMin = 1f;
  [SerializeField] float idleDurationMax = 5f;

  // Events
  [Serializable] public class FloatEvent : UnityEvent<float> { }
  [Tooltip("Gets invoked each frame the character must move. Provides a movement modifier float")]
  public FloatEvent OnIdleMove;
  public FloatEvent OnChangeDirection;

  // State

  // It's current movement
  float movement = 0f;

  protected override void OnAwake()
  {
    // Set up events
    if (OnIdleMove == null) OnIdleMove = new FloatEvent();
    if (OnChangeDirection == null) OnChangeDirection = new FloatEvent();
  }

  protected override void OnUpdate()
  {
    // Emit move event
    if (isCurrentState) OnIdleMove.Invoke(movement);
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
    OnIdleMove.Invoke(movement);
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
    movement = -movement;
    if (movement != 0) OnChangeDirection.Invoke(movement);
  }
}
