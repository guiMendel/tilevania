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
  public Vector2 launchForce = new Vector2(10f, 2f);

  [Tooltip("The launch torque possible value interval applied when throwing an item")]
  [SerializeReference] float launchTorqueMin = -20f;
  [SerializeReference] float launchTorqueMax = 20f;


  //=== State
  Transform grabbedItem;

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

    // Ensure we got an item & it has rigidbody
    if (
      grabable == null
      || grabable.GetComponent<Rigidbody2D>() == null
    ) return;

    // Ensure it's a projectile
    if (grabable.GetComponent<PlayerProjectile>() == null)
    {
      Debug.LogError("Tried to grab an item that doesn't contain a PlayerProjectile script");
      return;
    }

    // Grab it
    Grab(grabable.gameObject);
  }

  private void Grab(GameObject grabable)
  {
    // Move object to hand's position
    MoveToHand(grabable.transform);

    Rigidbody2D grabableRigidbody = grabable.GetComponent<Rigidbody2D>();

    // Avoid external forces while it's being held
    grabableRigidbody.bodyType = RigidbodyType2D.Kinematic;

    // Freeze it's rotation
    grabableRigidbody.freezeRotation = true;

    // Reset it's velocity
    grabableRigidbody.velocity = Vector2.zero;
    grabableRigidbody.angularVelocity = 0f;

    // Stick it to the hands
    grabable.transform.parent = hands;

    // Register it
    grabbedItem = grabable.transform;

    // Raise message
    SendMessage("OnGrabItemMessage", grabable);
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

    // Get force relative to facing direction
    Vector2 relativeLaunchForce = new Vector2(launchForce.x * Mathf.Sign(transform.localScale.x), launchForce.y);

    // Apply launch force
    itemRigidbody.AddForce(relativeLaunchForce, ForceMode2D.Impulse);

    // Get a random rotation speed (relative to facing direction)
    float launchTorque = Random.Range(launchTorqueMin, launchTorqueMax) * -Mathf.Sign(transform.localScale.x);

    // Apply random rotation
    itemRigidbody.AddTorque(launchTorque, ForceMode2D.Impulse);

    // Make it an active projectile
    grabbedItem.GetComponent<PlayerProjectile>().active = true;

    // Put it in the right layer
    grabbedItem.gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");

    // Forget it
    grabbedItem = null;

    // Send message
    SendMessage("OnItemThrownMessage");
  }
}
