using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireState : State
{
  //=== Params

  [Header("Firing")]
  [Tooltip("How often to fire projectiles, in seconds")]
  public float fireCooldown = 3f;

  [Tooltip("The projectile's prefab")]
  public Projectile projectilePrefab;

  [Tooltip("Position from where to shoot")]
  public Transform launchSource;

  [Header("Moving")]
  [Tooltip("Distance to try to keep from target")]
  public float preferredDistance = 3f;

  [Tooltip("Speed multiplier at which to move")]
  public float moveSpeedMultiplier = 0.3f;


  //=== State

  // Whether is in cooldown
  bool inCooldown;

  // Current target
  Transform currentTarget;


  protected override void OnUpdate()
  {
    if (!isCurrentState) return;

    // Move slightly to target
    AdjustDistance();

    // If not on cooldown, fire!
    if (inCooldown) return;

    Fire();

    // Start cooldown
    StartCoroutine(Cooldown());
  }

  private void AdjustDistance()
  {
    Movement movement = GetComponent<Movement>();

    if (movement == null) return;

    // Face the target
    movement.SetFacingDirection(Mathf.Sign(currentTarget.position.x - transform.position.x));

    // Get target distance
    float distance = Vector2.Distance(currentTarget.position, transform.position);

    // print(distance);

    // If distance is about right, stay put
    if (Mathf.Abs(distance - preferredDistance) <= 1f)
    {
      movement.Move(Vector2.zero);
      return;
    }

    // Decide whether to approach or move away
    float moveDirection = distance > preferredDistance ? 1 : -1;

    // Get target direction
    Vector2 targetDirection = (currentTarget.position - transform.position).normalized;

    // Move
    movement.Move(
      targetDirection * moveSpeedMultiplier * moveDirection, faceMovementDirection: false
    );
  }

  // Fire a projectile in the target's direction
  void Fire()
  {
    // Instantiate projectile
    Projectile projectile = Instantiate<Projectile>(projectilePrefab, launchSource.position, Quaternion.identity);

    // Set it's target
    projectile.homingTarget = currentTarget;

    // Fire!
    projectile.LaunchTowards(currentTarget.position);
  }

  IEnumerator Cooldown()
  {
    inCooldown = true;

    // Wait fire cooldown
    yield return new WaitForSeconds(fireCooldown);

    inCooldown = false;
  }

  // Forget target
  protected override void OnStateDisable() => currentTarget = null;


  //=== State Setup

  protected override string GetDefaultStateKeyName() => "movement";


  //=== Interface

  // Start firing at the provided target
  public void SetTarget(Transform target)
  {
    currentTarget = target;

    // Ensure state is enabled
    if (!isCurrentState) Enable();
  }
}
