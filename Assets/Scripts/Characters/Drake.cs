using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using Random = UnityEngine.Random;

// Deps
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(ChaseState))]
[RequireComponent(typeof(IdleWanderState))]
[RequireComponent(typeof(Rigidbody2D))]

public class Drake : MonoBehaviour
{
  //=== Params
  [Tooltip("Multiplier applied to the movement baseSpeed when in idle state")]
  [SerializeField] float idleSpeed = 2f;

  [Tooltip("Multiplier applied to the movement inertia when in idle state")]
  [SerializeField] float idleInertia = 0.2f;

  [Header("Attack Patterns")]
  [Tooltip("Maximum distance from which drake can shoot")]
  [SerializeField] float fireRange = 6f;


  //=== State
  // Default value for groundMovement base speed
  float defaultBaseSpeed;

  // Default value for groundMovement inertia
  float defaultInertia;

  // Whether drake is currently firing at a target
  bool isFiring;


  //=== Refs
  Movement _movement;
  ChaseState _chaseState;
  FireState _fireState;
  IdleWanderState _idleWanderState;
  Rigidbody2D _rigidbody;

  private void Awake()
  {
    _movement = GetComponent<AirborneMovement>();
    _chaseState = GetComponent<ChaseState>();
    _fireState = GetComponent<FireState>();
    _idleWanderState = GetComponent<IdleWanderState>();
    _rigidbody = GetComponent<Rigidbody2D>();
  }

  private void Start()
  {
    SetUpIdleMovementModifiers();

    // // _chaseState.OnStateDisabled.AddListener(() => print("Stop chasing!"));
    // _chaseState.OnStateEnabled.AddListener(() => print("CHASE"));

    _fireState.OnStateDisabled.AddListener(() => isFiring = false);
    _fireState.OnStateEnabled.AddListener(() => isFiring = true);

    // // _idleWanderState.OnStateDisabled.AddListener(() => print("Stop wandering!"));
    // _idleWanderState.OnStateEnabled.AddListener(() => print("WANDER"));
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
    _fireState.enabled = false;

    // Disable threat
    gameObject.layer = LayerMask.NameToLayer("ImmovableObject");

    // Turn gravity back on
    _rigidbody.gravityScale = 1f;

    GroundMovement movement = gameObject.AddComponent<GroundMovement>();
    movement.groundLayers = LayerMask.GetMask("Ground", "Breakable");
    movement.groundCheckRange = 1.5f;

    Destroy(_movement);
    _movement = movement;
  }


  //=== Interface

  // Method that will receive the player position when it's spotted
  // Decides whether to fly in closer or to start shooting, based on distance
  public void SetTarget(Transform target)
  {
    // Distance from target
    float targetDistance = Vector2.Distance(target.position, transform.position);

    // If out of range, or is not firing, get in the preferred fire distance
    if (targetDistance > fireRange || !isFiring && targetDistance > _fireState.preferredDistance)
    {
      _chaseState.SetTarget(target);
    }
    // Otherwise, fire at target position
    else
    {
      _fireState.SetTarget(target);
      isFiring = true;
    }
  }
}
