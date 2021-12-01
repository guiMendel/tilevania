using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Deps
[RequireComponent(typeof(AirborneMovement))]
[RequireComponent(typeof(ChaseState))]
[RequireComponent(typeof(IdleWanderState))]

public class Drake : MonoBehaviour
{
  //=== Params
  [Tooltip("Multiplier applied to the movement baseSpeed when in idle state")]
  [SerializeField] float idleSpeed = 2f;

  [Tooltip("Multiplier applied to the movement inertia when in idle state")]
  [SerializeField] float idleInertia = 0.2f;

  //=== State
  // Default value for groundMovement base speed
  float defaultBaseSpeed;

  // Default value for groundMovement inertia
  float defaultInertia;

  //=== Refs
  AirborneMovement _movement;
  ChaseState _chaseState;
  IdleWanderState _idleWanderState;

  private void Awake()
  {
    _movement = GetComponent<AirborneMovement>();
    _chaseState = GetComponent<ChaseState>();
    _idleWanderState = GetComponent<IdleWanderState>();
  }

  private void Start()
  {
    SetUpIdleMovementModifiers();
  }

  private void SetUpIdleMovementModifiers()
  {
    defaultBaseSpeed = _movement.baseSpeed;
    defaultInertia = _movement.inertia;

    // When idle, reduce speed & inertia
    _idleWanderState.OnStateEnabled.AddListener(() =>
    {
      _movement.baseSpeed = idleSpeed;
      _movement.inertia = idleInertia;
    });

    // When not idle, return speed & inertia back to normal
    _idleWanderState.OnStateDisabled.AddListener(() =>
    {
      _movement.baseSpeed = defaultBaseSpeed;
      _movement.inertia = defaultInertia;
    });
  }

  //=== Messages

  void OnDeathMessage()
  {
    // Disable all scripts
    _chaseState.enabled = false;
    _idleWanderState.enabled = false;

    // Disable threat
    gameObject.layer = LayerMask.NameToLayer("ImmovableObject");
  }
}
