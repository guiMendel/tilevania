using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using Unity.VisualScripting;
using UnityEngine;

// Deps
[RequireComponent(typeof(SpriteRenderer))]

public class PlayerProjectile : MonoBehaviour
{
  //=== Params
  [Header("Triggering")]
  [Tooltip("Which layers will the projectile hit")]
  public LayerMask contactLayers;

  [Tooltip("Whether it should trigger on collision")]
  public bool active;

  [Tooltip("The particles that are emitted on item collision")]
  public GameObject breakParticles;

  [Tooltip("How long it takes to erase self after collision")]
  public float eraseTime;

  [Header("Damage")]
  [Tooltip("The range of it's damage")]
  public float damageRange;

  [Tooltip("Which layers are to be damaged by the AOE")]
  public LayerMask damageLayers;

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

  private void Trigger()
  {
    // Trigger AOE damage
    TriggerAreaOfEffect();

    // Trigger particles
    GameObject particles = Instantiate(breakParticles, transform.position, Quaternion.identity) as GameObject;

    // Delete particles
    Destroy(particles, eraseTime);

    Destroy(gameObject);
  }

  private void TriggerAreaOfEffect()
  {
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

}
