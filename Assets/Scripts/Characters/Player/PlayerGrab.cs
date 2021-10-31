using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Deps
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerGrab : MonoBehaviour
{
  //=== Params
  [Tooltip("Layers of items that can be picked up by the player")]
  public LayerMask grabableLayers;

  [Tooltip("How far the player can grab items from")]
  public float grabRange;

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
    // Detect grab command
    DetectGrab();
  }

  private void DetectGrab()
  {
    if (!Input.GetButtonDown("Grab")) return;
    // Check if there's any grabable items in range
    Collider2D grabable = Physics2D.OverlapBox(
      transform.position,
      Vector2.one * grabRange,
      0f,
      grabableLayers
    );

    // Ensure we got an item and it has rigidbody
    if (grabable == null || grabable.GetComponent<Rigidbody2D>() == null) return;

    // Grab it
    Grab(grabable.gameObject);
  }

  private void Grab(GameObject grabable)
  {
    // Move object to hand's position: try to get a GrabAnchor position from it
    Transform grabableAnchorTransform = grabable.transform.Find("GrabAnchor");

    // If can't find this child, use own object's position
    if (grabableAnchorTransform == null) grabableAnchorTransform = grabable.transform;

    // Get distance from hand
    Vector3 handDistance = grabableAnchorTransform.position - hands.position;

    // Move item this distance
    grabable.transform.position -= handDistance;

    // Create a joint between the grabable and player's hands
    // handsJoint.connectedBody = grabable;
    grabable.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
    grabable.transform.parent = hands;
  }

  // Draw grab range
  private void OnDrawGizmosSelected()
  {
    Gizmos.DrawWireCube(transform.position, Vector3.one * grabRange);
  }
}
