using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Deps
[RequireComponent(typeof(Rigidbody2D))]

public class DeathSensor : MonoBehaviour
{
  //=== Params
  [Tooltip("Which layers trigger death on collision")]
  public LayerMask threatLayers;

  [Tooltip("How much character gets launched on death")]
  public float deathKick = 5f;

  [Tooltip("How much character gets launched up on death. Is applied in addition to normal kick force")]
  public float upwardsExtraKick = 2f;

  private void OnCollisionEnter2D(Collision2D other)
  {
    // Check if is a death layer
    if (threatLayers == (threatLayers | 1 << other.gameObject.layer))
    {
      Die();
      LaunchAwayFrom(other.transform);
    }
  }

  private void LaunchAwayFrom(Transform other)
  {
    // Ignore if no kick is to be applied
    if (deathKick < Mathf.Epsilon) return;

    // Get direction
    Vector3 sourceDirection = (other.position - transform.position).normalized;

    // Rotate direction
    Vector3 direction = Quaternion.Euler(0, 0, 180) * sourceDirection;

    // Apply force
    GetComponent<Rigidbody2D>().AddForce(direction * deathKick + Vector3.up * upwardsExtraKick, ForceMode2D.Impulse);
  }

  private void Die()
  {
    // Send message
    SendMessage("OnDeathMessage");
  }
}
