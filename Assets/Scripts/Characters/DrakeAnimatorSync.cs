using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Deps
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(ChaseState))]

public class DrakeAnimatorSync : MonoBehaviour
{
  // Refs
  Animator _animator;
  ChaseState _chaseState;
  Rigidbody2D _rigidbody;

  private void Awake()
  {
    _animator = GetComponent<Animator>();
    _chaseState = GetComponent<ChaseState>();
    _rigidbody = GetComponent<Rigidbody2D>();
  }

  private void Start()
  {
    // When chasing, change to chase animation
    _chaseState.OnStateEnabled.AddListener(() => _animator.SetBool("Chasing", true));
    _chaseState.OnStateDisabled.AddListener(() => _animator.SetBool("Chasing", false));
  }

  private void Update()
  {
    // If dead, keep track of vertical movement
    if (!_animator.GetBool("Dead")) return;

    // Detect y movement
    float yMovement = _rigidbody.velocity.y;
    int yDirection = 0;

    // Detect significant vertical movement
    if (yMovement > 0.1f) yDirection = 1;
    if (yMovement < -0.1f) yDirection = -1;

    _animator.SetInteger("DeadAirborneDirection", yDirection);
  }


  //=== Messages
  void OnDeathMessage() => _animator.SetBool("Dead", true);
}
