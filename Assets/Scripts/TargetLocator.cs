using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Whenever the target gameObject walks into sight range, raises target located events
public class TargetLocator : MonoBehaviour
{
  // Params
  [Tooltip("The angle of vision, in degrees. It corresponds to half the vision cone")]
  [SerializeField] float sightAngle = 45f;

  [Tooltip("How far the sight goes")]
  [SerializeField] float sightRange = 6f;

  [Tooltip("Which target will trigger a target located event")]
  [SerializeField] GameObject target;

  [Tooltip("Which layers should block the vision")]
  [SerializeField] LayerMask blockingLayers;

  // Refs
  Collider2D _collider2D;

  private void Awake()
  {
    GetRefs();
  }

  private void Update()
  {
    // Get target direction
    Vector2 targetDirection = (target.transform.position - transform.position).normalized;

    // Perform a raycast check
    RaycastHit2D hit = Physics2D.Raycast(transform.position, targetDirection, sightRange, getRaycastLayers());
    Debug.DrawRay(transform.position, targetDirection * sightRange);

    // Check if it hit the target
    if (hit.collider)
    // if (hit.collider && GameObject.ReferenceEquals(hit.collider.gameObject, target))
    {
      print(hit.collider);
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
