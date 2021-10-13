using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Deps
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(GroundMovement))]

public class HoundAnimatorSync : MonoBehaviour
{
  // Refs
  Animator _animator;
  Rigidbody2D _rigidbody;
  GroundMovement _groundMovement;

  private void Awake()
  {
    _animator = GetComponent<Animator>();
    _rigidbody = GetComponent<Rigidbody2D>();
    _groundMovement = GetComponent<GroundMovement>();
  }

  private void Update()
  {
    DetectMovement();
    DetectAirborne();
  }

  private void DetectMovement()
  {
    // Detect x movement (exerted by the hound, not external entities)
    float xMovement = Mathf.Abs(_groundMovement.GetLastFrameMovement());

    // Update animation
    _animator.SetBool("Running", xMovement > 0.05f);
  }

  private void DetectAirborne()
  {
    // Detect y movement
    float yMovement = Mathf.Abs(_rigidbody.velocity.y);

    // Detect significant vertical movement
    _animator.SetBool("Airborne", yMovement > 0.1f);
  }
}
