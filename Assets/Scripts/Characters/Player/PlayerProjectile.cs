using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
  //=== Params
  [Tooltip("Which layers will the projectile hit")]
  public LayerMask contactLayers;

  [Tooltip("The range of it's damage")]
  public float damageRange;

  [Tooltip("Whether it's going to trigger damage on next collision")]
  public bool active;

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
    // Delete self
    Destroy(gameObject);
  }

  // Draw it's range
  private void OnDrawGizmosSelected()
  {
    Gizmos.DrawWireSphere(transform.position, damageRange);
  }

}
