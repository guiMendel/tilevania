using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

// Deps
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerGrab : MonoBehaviour
{
  //=== Params
  [Tooltip("Layers of items that can be picked up by the player")]
  public LayerMask grabableLayers;

  [Tooltip("How far the player can grab items from")]
  public float grabRange = 1f;

  [Tooltip("The launch force of throwing an item")]
  public float launchImpulse = 10f;

  [Tooltip("The launch angle when throwing an item")]
  public float launchAngle = 30;

  [Tooltip("The launch torque possible value interval applied when throwing an item")]
  [SerializeReference] float launchTorqueMin = -20f;
  [SerializeReference] float launchTorqueMax = 20f;


  //=== State
  Projectile grabbedItem;

  //=== Refs
  // Where the player's hands are
  Transform hands;
  Rigidbody2D _rigidbody;
  FixedJoint2D handsJoint;

  private void Awake()
  {
    hands = transform.Find("Hands");
    _rigidbody = GetComponent<Rigidbody2D>();

    // Get hands' joint
    handsJoint = hands.GetComponent<FixedJoint2D>();

    if (handsJoint == null)
    {
      // handsJoint = hands.AddComponent<FixedJoint2D>() as FixedJoint2D;
    }
  }

  private void Update()
  {
    // Detect grab & throw commands
    DetectInteraction();
  }

  private void DetectInteraction()
  {
    // Detect input
    if (!Input.GetButtonDown("Grab")) return;

    // Throw if holding an item, otherwise grab
    if (grabbedItem != null) Throw();
    else AttemptGrab();
  }

  // Send message and let animation trigger call LaunchItem
  private void Throw() => SendMessage("OnThrowItemMessage", grabbedItem);

  private void AttemptGrab()
  {
    // Check if there's any grabable items in range
    Collider2D grabable = Physics2D.OverlapBox(
      transform.position,
      Vector2.one * grabRange,
      0f,
      grabableLayers
    );

    // Get projectile instance
    Projectile itemProjectile = grabable?.GetComponent<Projectile>();

    // Ensure we got an item & it has rigidbody
    if (
      grabable == null
      || itemProjectile == null
      || grabable.GetComponent<Rigidbody2D>() == null
    ) return;

    // Grab it
    Grab(itemProjectile);
  }

  private void Grab(Projectile grabable)
  {
    // Register it
    grabbedItem = grabable;

    // Move object to hand's position
    MoveToHand(grabbedItem.transform);

    Rigidbody2D grabableRigidbody = grabbedItem.GetComponent<Rigidbody2D>();

    // Avoid external forces while it's being held
    grabableRigidbody.bodyType = RigidbodyType2D.Kinematic;

    // Freeze it's rotation
    grabableRigidbody.freezeRotation = true;

    // Reset it's velocity
    grabableRigidbody.velocity = Vector2.zero;
    grabableRigidbody.angularVelocity = 0f;

    // Stick it to the hands
    grabbedItem.transform.parent = hands;

    // Raise message
    SendMessage("OnGrabItemMessage", grabbedItem);
  }

  private void MoveToHand(Transform grabable)
  {
    // Reset item rotation
    grabable.transform.rotation = Quaternion.identity;

    // Try to get anchor point
    Transform grabableAnchorTransform = grabable.Find("GrabAnchor");

    // If can't find this child, use own object's position
    if (grabableAnchorTransform == null) grabableAnchorTransform = grabable;

    // Get distance from hand
    Vector3 handDistance = grabableAnchorTransform.position - hands.position;

    // Move item this distance
    grabable.position -= handDistance;

  }

  // Draw grab range
  private void OnDrawGizmosSelected()
  {
    Gizmos.DrawWireCube(transform.position, Vector3.one * grabRange);
  }


  //=== Interface

  // Launch the item being currently held
  public void LaunchItem()
  {
    // Get it's rigidbody
    Rigidbody2D itemRigidbody = grabbedItem.GetComponent<Rigidbody2D>();

    // Make the item dynamic again
    itemRigidbody.bodyType = RigidbodyType2D.Dynamic;

    // Unfreeze it's rotation
    itemRigidbody.freezeRotation = false;

    // Release it from the hand
    grabbedItem.transform.parent = null;

    // Set it's impulse
    grabbedItem.launchImpulse = launchImpulse;

    // Launch it in the facing direction
    float relativeLaunchAngle = Mathf.Sign(transform.localScale.x) == 1 ? launchAngle : 180 - launchAngle;
    grabbedItem.LaunchTowards(
      Quaternion.Euler(0, 0, relativeLaunchAngle) * Vector2.right, directionIsRelative: true
    );

    // Get a random rotation speed (relative to facing direction)
    float launchTorque = Random.Range(launchTorqueMin, launchTorqueMax) * -Mathf.Sign(transform.localScale.x);

    // Apply random rotation
    itemRigidbody.AddTorque(launchTorque, ForceMode2D.Impulse);

    // Make it an active projectile
    grabbedItem.active = true;

    // Put it in the right layer
    grabbedItem.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");

    // Forget it
    grabbedItem = null;

    // Send message
    SendMessage("OnItemThrownMessage");
  }
}
