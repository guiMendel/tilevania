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
  public float launchImpulse = 10f;

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
  public float homingControl;


  //=== Refs

  Rigidbody2D _rigidbody;

  private void Awake()
  {
    _rigidbody = GetComponent<Rigidbody2D>();
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
      print(enemy.gameObject.name);

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
    _rigidbody.AddForce(direction * launchImpulse, ForceMode2D.Impulse);

    // Activate
    if (activate) active = true;
  }
}
