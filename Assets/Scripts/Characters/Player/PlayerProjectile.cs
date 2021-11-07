using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Deps
[RequireComponent(typeof(SpriteRenderer))]

public class PlayerProjectile : MonoBehaviour
{
  //=== Params
  [Tooltip("Which layers will the projectile hit")]
  public LayerMask contactLayers;

  [Tooltip("The range of it's damage")]
  public float damageRange;

  [Tooltip("Whether it's going to trigger damage on next collision")]
  public bool active;

  [Tooltip("The particles that are emitted on item collision")]
  public GameObject breakParticles;

  [Tooltip("How long it takes to erase self after collision")]
  public float eraseTime;

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
    // Trigger particles
    GameObject particles = Instantiate(breakParticles, transform.position, Quaternion.identity) as GameObject;

    // Delete particles
    Destroy(particles, eraseTime);

    Destroy(gameObject);
  }

  // Draw it's range
  private void OnDrawGizmosSelected()
  {
    Gizmos.DrawWireSphere(transform.position, damageRange);
  }

}
