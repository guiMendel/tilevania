using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

// Deps
[RequireComponent(typeof(SharedState))]

public class IdleMovementState : MonoBehaviour
{
  // Params
  [Header("State management")]
  [Tooltip("Whether this should be the starting state")]
  [SerializeField] bool startingState;
  [Tooltip("The state key used by this state")]
  [SerializeField] string stateKey = "movement";

  [Header("Idle movement settings")]
  [SerializeField] float moveDurationMin = 0.5f;
  [SerializeField] float moveDurationMax = 2f;
  [SerializeField] float idleDurationMin = 1f;
  [SerializeField] float idleDurationMax = 5f;

  // Events
  [Serializable] public class FloatEvent : UnityEvent<float> { }
  [Tooltip("Gets invoked each frame the character must move. Provides a movement modifier float")]
  public FloatEvent OnIdleMove;

  // State

  // It's current movement
  float movement = 0f;

  // Refs
  SharedState _sharedState;

  private void Start()
  {
    // Get refs
    _sharedState = GetComponent<SharedState>();

    // Set up events
    if (OnIdleMove == null) OnIdleMove = new FloatEvent();

    // Initialize if starting state
    if (startingState) Enable();
  }

  private void Update()
  {
    // Emit move event if moving
    if (movement != 0f) OnIdleMove.Invoke(movement);
  }

  private IEnumerator Wander()
  {
    // Loop until state changes
    bool isCurrentState = _sharedState.GetState(stateKey) == this.GetType().Name;

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

      // Check state condition
      isCurrentState = _sharedState.GetState(stateKey) == this.GetType().Name;
    }

    // When done, ensure movement has stopped
    movement = 0f;
  }

  private void WanderNextIteration()
  {
    // Change movement
    movement = movement == 0f ? 1f : 0f;

    // Randomly switch direction
    if (Random.value < 0.5f) movement = -movement;
  }

  // Interface

  // Sets this as the current state and starts emitting events
  public void Enable()
  {
    // Set state
    _sharedState.SetState(stateKey, this.GetType().Name);

    // Start coroutine
    StartCoroutine(Wander());
  }
}
