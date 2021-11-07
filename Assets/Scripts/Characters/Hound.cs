using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Deps
[RequireComponent(typeof(SharedState))]
[RequireComponent(typeof(CollisionSensor))]
[RequireComponent(typeof(GroundMovement))]
[RequireComponent(typeof(ChaseState))]
[RequireComponent(typeof(IdleWanderState))]

public class Hound : MonoBehaviour
{
  //=== Params
  [Tooltip("Multiplier applied to the groundMovement baseSpeed when in idle state")]
  [SerializeField] float idleSpeed = 2f;

  [Tooltip("Multiplier applied to the groundMovement inertia when in idle state")]
  [SerializeField] float idleInertia = 0.2f;

  //=== State
  // Default value for groundMovement base speed
  float defaultBaseSpeed;

  // Default value for groundMovement inertia
  float defaultInertia;

  //=== Refs
  SharedState _sharedState;
  CollisionSensor _collisionSensor;
  GroundMovement _groundMovement;
  ChaseState _chaseState;
  IdleWanderState _idleWanderState;

  private void Awake()
  {
    _sharedState = GetComponent<SharedState>();
    _collisionSensor = GetComponent<CollisionSensor>();
    _groundMovement = GetComponent<GroundMovement>();
    _chaseState = GetComponent<ChaseState>();
    _idleWanderState = GetComponent<IdleWanderState>();
  }

  private void Start()
  {
    SetUpChasingWallJump();

    SetUpIdleMovementModifiers();
  }

  private void SetUpIdleMovementModifiers()
  {
    defaultBaseSpeed = _groundMovement.baseSpeed;
    defaultInertia = _groundMovement.inertia;

    // When idle, reduce speed & inertia
    _idleWanderState.OnStateEnabled.AddListener(() =>
    {
      _groundMovement.baseSpeed = idleSpeed;
      _groundMovement.inertia = idleInertia;
    });

    // When not idle, return speed & inertia back to normal
    _idleWanderState.OnStateDisabled.AddListener(() =>
    {
      _groundMovement.baseSpeed = defaultBaseSpeed;
      _groundMovement.inertia = defaultInertia;
    });
  }

  private void SetUpChasingWallJump()
  {
    // When chasing, jump whenever facing a wall
    var wallSensor = _collisionSensor.GetSensorByGameObjectName("Wall Sensor");

    wallSensor.OnSensorStay.AddListener(() =>
    {
      if (_sharedState.IsStateActive(_chaseState)) _groundMovement.Jump();
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
