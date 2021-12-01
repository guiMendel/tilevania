using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Deps
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AirborneMovement))]
[RequireComponent(typeof(ChaseState))]

public class DrakeAnimatorSync : MonoBehaviour
{
  // Refs
  Animator _animator;
  ChaseState _chaseState;

  private void Awake()
  {
    _animator = GetComponent<Animator>();
    _chaseState = GetComponent<ChaseState>();
  }

  private void Start()
  {
    // When chasing, change to chase animation
    _chaseState.OnStateEnabled.AddListener(() => _animator.SetBool("Chasing", true));
    _chaseState.OnStateDisabled.AddListener(() => _animator.SetBool("Chasing", false));
  }


  //=== Messages
  void OnDeathMessage() => _animator.SetBool("Dead", true);
}
