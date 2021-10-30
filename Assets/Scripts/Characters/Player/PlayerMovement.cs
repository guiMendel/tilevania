using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diagnostics = System.Diagnostics;

// Deps
[RequireComponent(typeof(GroundMovement))]

public class PlayerMovement : MonoBehaviour
{
  //=== Params
  [Header("Dash Parameters")]
  [Tooltip("How many milliseconds may be tolerated between the two key presses of a dash")]
  [SerializeField] int dashTolerance = 500;

  [Tooltip("How much the walking speed gets multiplied by when player is dashing")]
  [SerializeField] float dashSpeedMultiplier = 2f;

  [Header("Jumping")]
  [Tooltip("Time elapsed between player command to jump and actual jump, in seconds")]
  [SerializeField] float jumpAnticipation = 0.1f;


  //=== State
  // The dash multiplier application. If dashing, will be = dashSpeedMultiplier. If not, will be 1
  float activeDashModifier = 1f;

  //=== Refs
  GroundMovement _groundMovement;
  PlayerAnimationSync _animationSync;

  void Awake()
  {
    _groundMovement = GetComponent<GroundMovement>();
    _animationSync = GetComponent<PlayerAnimationSync>();
  }

  // Update is called once per frame
  void Update()
  {
    // Detect walking & dashing
    InputWalkAndDash();

    // Detect jumping
    InputJump();

    // Detect climbing
    InputClimb();
  }

  private void InputWalkAndDash()
  {
    // Get the frame's movement
    float frameMovement = Input.GetAxis("Horizontal");

    // Handle dashing
    frameMovement = HandleDashing(frameMovement);

    // Apply movement
    _groundMovement.Move(frameMovement);
  }

  private float HandleDashing(float frameMovement)
  {
    // If movement has ceased, reset dash modifier
    if (Mathf.Abs(frameMovement) < Mathf.Epsilon) activeDashModifier = 1f;

    // If a movement key was pressed in this exact frame, trigger a coroutine that will detect a double press
    if (Input.GetButtonDown("Horizontal")) StartCoroutine(DetectDoublePress());

    return frameMovement * activeDashModifier;
  }

  private IEnumerator DetectDoublePress()
  {
    // Get direction of movement
    float direction = Mathf.Sign(Input.GetAxisRaw("Horizontal"));

    // Start counting live time
    Diagnostics.Stopwatch liveTimeCounter = Diagnostics.Stopwatch.StartNew();

    // Keep waiting
    while (true)
    {
      // Wait next frame
      yield return null;

      // Check for the double tap
      if (Input.GetButtonDown("Horizontal"))
      {
        float secondDirection = Mathf.Sign(Input.GetAxisRaw("Horizontal"));

        // Check if directions match
        if (direction == secondDirection) activeDashModifier = dashSpeedMultiplier;

        // Stop coroutine after second tap
        yield break;
      }

      // Check live time. If timer is due, die
      if (liveTimeCounter.ElapsedMilliseconds > dashTolerance) yield break;
    }
  }

  private void InputJump()
  {
    if (Input.GetButtonDown("Jump")) StartCoroutine(Jump());
  }

  private IEnumerator Jump()
  {
    // Ground check
    if (!_groundMovement.IsGrounded()) yield break;

    // Start animation
    _animationSync.JumpAnimation();

    // Wait anticipation time
    yield return new WaitForSeconds(jumpAnticipation);

    // Jump
    _groundMovement.Jump(skipGroundCheck: true);
  }

  private void InputClimb()
  {
    // See if player inputs climbing keys
    float frameClimb = Input.GetAxisRaw("Vertical");

    // Forward them
    if (Mathf.Abs(frameClimb) > Mathf.Epsilon) _groundMovement.Climb(frameClimb);
  }
}
