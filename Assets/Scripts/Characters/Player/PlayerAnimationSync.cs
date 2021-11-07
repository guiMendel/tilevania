using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationSync : MonoBehaviour
{
  //=== Refs
  Animator _animator;
  GroundMovement _groundMovement;
  Rigidbody2D _rigidbody;

  private void Awake()
  {
    _animator = GetComponent<Animator>();
    _groundMovement = GetComponent<GroundMovement>();
    _rigidbody = GetComponent<Rigidbody2D>();
  }

  private void Update()
  {
    DetectHorizontalMovement();
    DetectVerticalMovement();
  }

  private void DetectHorizontalMovement()
  {
    // Detect x movement (exerted by the player, not external entities)
    float xMovement = Mathf.Abs(_groundMovement.GetLastFrameMovement());

    // Update animation
    _animator.SetBool("Walking", xMovement > 0.05f);

    // Detect sprinting
    _animator.SetBool("Sprinting", xMovement > _groundMovement.baseSpeed);
  }

  private void DetectVerticalMovement()
  {
    // If climbing, set climbing direction. If not, set airborne direction
    Action<float> setAnimatorParameter;

    if (_animator.GetBool("Climbing"))
    {
      setAnimatorParameter = (float direction) => _animator.SetFloat("ClimbingSpeedMultiplier", direction);

      // Reset the other one
      _animator.SetInteger("AirborneDirection", (0));
    }

    else
    {
      setAnimatorParameter = (float direction) => _animator.SetInteger("AirborneDirection", (int)direction);

      // Reset the other one
      _animator.SetFloat("ClimbingSpeedMultiplier", 0f);
    }

    // Detect y movement
    float yMovement = _rigidbody.velocity.y;

    // Detect almost stable
    if (Mathf.Abs(yMovement) <= 0.1f)
    {
      setAnimatorParameter(0f);
      return;
    }

    // Detect significant vertical movement
    setAnimatorParameter(Mathf.Sign(yMovement));
  }

  //=== Message Hooks

  // Climbing states
  void OnStartClimbingMessage() => _animator.SetBool("Climbing", true);

  void OnStopClimbingMessage() => _animator.SetBool("Climbing", false);

  // Carry state
  void OnGrabItemMessage() => _animator.SetBool("Carrying", true);

  void OnThrowItemMessage() => _animator.SetTrigger("Throw");

  void OnItemThrownMessage() => _animator.SetBool("Carrying", false);

  // Death
  void OnDeathMessage() => _animator.SetBool("Dead", true);

  //=== Interface
  public void JumpAnimation()
  {
    // Set jump animation
    _animator.SetTrigger("Jump");
  }
}
