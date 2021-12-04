using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using Unity.VisualScripting;
using UnityEngine;

// Deps
[RequireComponent(typeof(Rigidbody2D))]

public class Projectile : MonoBehaviour
{
  //=== Params
  [Tooltip("Projectile's launch impulse")]
  public float launchVelocity = 10f;

  [Header("Triggering")]
  [Tooltip("Which layers will the projectile hit")]
  public LayerMask contactLayers;

  [Tooltip("Whether it should trigger on the next collision")]
  public bool active;

  [Tooltip("The particles that are emitted on collision")]
  public GameObject breakParticles;

  [Tooltip("How long it takes to erase particles after collision")]
  [Min(0f)] public float eraseTime = 3f;

  [Header("Damage")]
  [Tooltip("The range of it's damage")]
  [Min(0f)] public float damageRange = 1f;

  [Tooltip("Which layers are to be damaged by the AOE")]
  public LayerMask damageLayers;

  [Header("Homing")]
  [Tooltip("Target to chase")]
  public Transform homingTarget;

  [Tooltip("Amount of homing control. 0 means no homing, 1 means perfect stirring")]
  [Range(0, 1)] public float homingControl;

  [Header("Misc")]
  [Tooltip("Whether to constantly adjust rotation to match movement direction")]
  public bool fixRotationToMovementDirection;


  //=== Refs

  Rigidbody2D _rigidbody;

  private void Awake()
  {
    _rigidbody = GetComponent<Rigidbody2D>();
  }

  private void Update()
  {
    ChaseTarget();

    // Fix rotation
    AdjustRotation();
  }

  private void AdjustRotation()
  {
    if (!fixRotationToMovementDirection) return;

    // Get movement direction angle
    float movementAngle = Mathf.Acos(_rigidbody.velocity.normalized.x) * Mathf.Rad2Deg * Mathf.Sign(_rigidbody.velocity.y);

    // Set object rotation
    transform.rotation = Quaternion.Euler(0, 0, movementAngle);
  }

  // Adjust direction to track target
  private void ChaseTarget()
  {
    if (homingTarget == null || homingControl < Mathf.Epsilon) return;

    // Get target direction
    Vector2 targetDirection = homingTarget.position - transform.position;

    // Get  direction difference, and make it proportional to homing control
    Vector2 directionDifference = (_rigidbody.velocity.normalized - targetDirection.normalized) * homingControl;

    // Get the new direction
    Vector2 newDirection = _rigidbody.velocity.normalized - directionDifference;

    // Set the new speed
    _rigidbody.velocity = newDirection.normalized * _rigidbody.velocity.magnitude;
  }

  private void OnCollisionEnter2D(Collision2D other)
  {
    // Don't do anything if not active
    if (!active) return;

    // Check if object is in target layer mask
    if (((int)contactLayers) == (contactLayers | (1 << other.gameObject.layer)))
    {
      Trigger();
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    // Don't do anything if not active
    if (!active) return;

    // Check if object is in target layer mask
    if (((int)contactLayers) == (contactLayers | (1 << other.gameObject.layer)))
    {
      Trigger();
    }
  }

  private void Trigger()
  {
    // Trigger AOE damage
    TriggerAreaOfEffect();

    // Trigger particles
    if (breakParticles != null)
    {
      GameObject particles = Instantiate(breakParticles, transform.position, Quaternion.identity) as GameObject;

      // Delete particles
      Destroy(particles, eraseTime);
    }

    // If projectile fathers any particle systems, make them independent
    ParticleSystem particleSystem = GetComponentInChildren<ParticleSystem>();
    if (particleSystem)
    {
      particleSystem.transform.parent = null;
      particleSystem.Stop();
      Destroy(particleSystem.gameObject, eraseTime);
    }

    Destroy(gameObject);
  }

  private void TriggerAreaOfEffect()
  {
    if (damageRange <= Mathf.Epsilon) return;

    // Find all targets in range
    Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, damageRange, damageLayers);

    // For each target hit, get it's death sensor component and trigger it's damage
    foreach (Collider2D enemy in enemiesInRange)
    {
      // Get it's death sensor
      DeathSensor enemyDeathSensor = enemy.GetComponent<DeathSensor>();

      if (!enemyDeathSensor) continue;

      // Trigger it's damage
      enemyDeathSensor.GetKilledBy(transform);
    }
  }

  // Draw it's range
  private void OnDrawGizmosSelected()
  {
    Gizmos.DrawWireSphere(transform.position, damageRange);
  }

  //=== Interface

  // Launch towards this point
  public void LaunchTowards(Vector2 direction, bool directionIsRelative = false, bool activate = true)
  {
    // Get target direction, if not yet provided relative to current position
    if (!directionIsRelative) direction -= (Vector2)transform.position;

    // Apply launch force
    _rigidbody.velocity = direction.normalized * launchVelocity;

    // Activate
    if (activate) active = true;
  }

  // Launch towards this object, allow for movement prediction
  public void LaunchTowards(Transform target, float predictionAccuracy = 0f, bool activate = true)
  {
    Vector2 direction;

    // Don't bother with accuracy close to 0
    if (predictionAccuracy > Mathf.Epsilon)
    {
      // Predict target movement
      direction = MovementPrediction.PredictTargetMovement(transform, target, launchVelocity);

      // Lerp with accuracy
      direction = Vector2.Lerp(
        (target.position - transform.position).normalized, direction, predictionAccuracy
      );
    }
    else direction = (target.position - transform.position).normalized;

    // Apply launch velocity
    _rigidbody.velocity = direction.normalized * launchVelocity;

    // Activate
    if (activate) active = true;
  }
}
