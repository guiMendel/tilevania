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
    DetectMovement();
    DetectAirborne();
  }

  private void DetectMovement()
  {
    // Detect x movement (exerted by the player, not external entities)
    float xMovement = Mathf.Abs(_groundMovement.GetLastFrameMovement());

    // Update animation
    _animator.SetBool("Walking", xMovement > 0.05f);

    // Detect sprinting
    _animator.SetBool("Sprinting", xMovement > _groundMovement.baseSpeed);
  }

  private void DetectAirborne()
  {
    // Detect y movement
    float yMovement = _rigidbody.velocity.y;

    // Detect almost stable
    if (Mathf.Abs(yMovement) <= 0.1f)
    {
      _animator.SetInteger("AirborneDirection", 0);
      return;
    }

    // Detect significant vertical movement
    _animator.SetInteger("AirborneDirection", (int)Mathf.Sign(yMovement));
  }

  //=== Interface
  public void JumpAnimation()
  {
    // Set jump animation
    _animator.SetTrigger("Jump");
  }
}
