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

  [Tooltip("Whether to try to reach out to a movement component and make it stand still, after death")]
  public bool holdPositionAfterDeath = true;

  [Tooltip("Offset center of mass up by this amount when calculating death kick direction")]
  public float kickHeightOffest = 0f;

  //=== State
  bool triggered;

  private void Update()
  {
    if (triggered && holdPositionAfterDeath) HoldPosition();
  }

  private void HoldPosition()
  {
    Movement movement = GetComponent<Movement>();

    // If it's ground movement, ensure it's grounded
    if (movement is GroundMovement)
    {
      if (!(movement as GroundMovement).IsGrounded()) return;
    }

    // Stay put
    movement.Move(Vector2.zero);
  }

  private void OnCollisionEnter2D(Collision2D other)
  {
    // Check if is a death layer
    if (threatLayers == (threatLayers | 1 << other.gameObject.layer))
    {
      print(other.gameObject.name);
      GetKilledBy(other.transform);
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    // Check if is a death layer
    if (threatLayers == (threatLayers | 1 << other.gameObject.layer))
    {
      print(other.gameObject.name);
      GetKilledBy(other.transform);
    }
  }

  private void LaunchAwayFrom(Transform other)
  {
    // Ignore if no kick is to be applied
    if (deathKick < Mathf.Epsilon) return;

    // Treat other's position as a little below it's actual position
    Vector3 position = new Vector3(other.position.x, other.position.y - kickHeightOffest);

    // Get direction
    Vector3 sourceDirection = (position - transform.position).normalized;

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

  //=== Interface

  public void GetKilledBy(Transform other)
  {
    triggered = true;
    Die();
    LaunchAwayFrom(other);
  }
}
