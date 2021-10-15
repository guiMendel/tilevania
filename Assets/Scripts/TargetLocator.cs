using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

// Whenever the target gameObject walks into sight range, raises target located events
public class TargetLocator : MonoBehaviour
{
  //=== Params
  [Tooltip("The angle of vision, in degrees. It corresponds to half the vision cone")]
  [SerializeField] float sightAngle = 45f;

  [Tooltip("How far the sight goes")]
  [SerializeField] float sightRange = 6f;

  [Tooltip("Which target will trigger a target located event")]
  [SerializeField] GameObject target;

  [Tooltip("Which layers should block the vision")]
  [SerializeField] LayerMask blockingLayers;

  //=== State

  // Whether the target was spotted last frame
  bool targetSpottedLastFrame;

  //=== Events

  // Event type
  [Serializable] public class Vector2Event : UnityEvent<Vector2> { }

  // Target spotted event
  public Vector2Event OnTargetSpotted;

  // Target lost event
  public UnityEvent OnTargetLost;

  //=== Refs
  Collider2D _collider2D;

  private void Awake()
  {
    GetRefs();

    // Init
    if (OnTargetSpotted == null) OnTargetSpotted = new Vector2Event();
    if (OnTargetLost == null) OnTargetLost = new UnityEvent();
  }

  private void Update()
  {
    // Get target direction
    Vector2 targetDirection = (target.transform.position - transform.position).normalized;

    // Perform a raycast check
    RaycastHit2D hit = Physics2D.Raycast(transform.position, targetDirection, sightRange, getRaycastLayers());

    // Check if it hit the target
    bool targetSpotted = hit.collider && (
      // Either it hit the target
      GameObject.ReferenceEquals(hit.collider.gameObject, target) ||
      // Or one of it's children
      hit.collider.transform.IsChildOf(target.transform)
    );

    // Visualize the sight ray
    Debug.DrawRay(transform.position, targetDirection * hit.distance, targetSpotted ? Color.green : Color.red);

    if (targetSpotted)
    {
      // Raise event
      OnTargetSpotted.Invoke(hit.point);

      // Register hit
      targetSpottedLastFrame = true;
    }

    // If not spotted, see if target was lost
    else if (targetSpottedLastFrame)
    {
      // Register target loss
      targetSpottedLastFrame = false;

      // Raise loss event 
      OnTargetLost.Invoke();
    }
  }

  private int getRaycastLayers()
  {
    // Count in the blocking layers and the target's layer, count out the gameObject's layer
    return blockingLayers | (1 << target.layer) & ~(1 << gameObject.layer);
  }

  private void GetRefs()
  {
    _collider2D = GetComponent<Collider2D>();

    // Report dependencies
    if (_collider2D == null)
    {
      Debug.LogError(gameObject.name + " is missing " + _collider2D.GetType().Name + " component");
    }
  }
}
