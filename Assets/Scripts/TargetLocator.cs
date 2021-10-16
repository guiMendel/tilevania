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
  [Tooltip("Whether to invert the direction the character faces")]
  [SerializeField] bool invertFacingDirection;

  [Header("Sight parameters")]
  [Tooltip("The angle of vision, in degrees. It corresponds to half the vision cone")]
  [Min(0f)] [SerializeField] float sightAngle = 45f;

  [Tooltip("How far the sight goes")]
  [Min(0f)] [SerializeField] float sightRange = 6f;

  [Header("Detection")]
  [Tooltip("Which target will trigger a target located event")]
  [SerializeField] GameObject target;

  [Tooltip("Which layers should block the vision")]
  [SerializeField] LayerMask blockingLayers;

  [Tooltip("For how long the target's position will still be known after it's no longer in sight\nTarget lost event will only be triggered after this time has been elapsed")]
  [Min(0f)] [SerializeField] float predictionTime = 0.5f;

  //=== State

  // Whether the target was spotted last frame
  bool targetSpottedLastFrame;

  // Which direction the character is facing
  Vector2 facingDirection;

  // Whether it's currently predicting the target's position
  Coroutine prediction;

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
    // Update facing direction
    facingDirection = new Vector2(
      invertFacingDirection ? -transform.localScale.x : transform.localScale.x,
      0
    ).normalized;

    // Try to spot the target
    LocateTarget();

    // Print the debug sight cone
    PrintSightCone();
  }

  private void PrintSightCone()
  {
    // First ray
    Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, sightAngle) * facingDirection * sightRange);

    // Second ray
    Debug.DrawRay(transform.position, Quaternion.Euler(0, 0, -sightAngle) * facingDirection * sightRange);
  }

  private void LocateTarget()
  {
    // Get target direction
    Vector2 targetDirection = (target.transform.position - transform.position).normalized;

    // Perform a raycast check
    RaycastHit2D hit = Physics2D.Raycast(transform.position, targetDirection, sightRange, getRaycastLayers());

    // Check if it hit the target
    bool targetInRange = hit.collider && (
      // Either it hit the target
      GameObject.ReferenceEquals(hit.collider.gameObject, target) ||
      // Or one of it's children
      hit.collider.transform.IsChildOf(target.transform)
    );

    // Check if it's inside the sight cone
    bool targetSpotted = targetInRange && TargetInSightCone(targetDirection);

    // Debug ray
    DrawSightRay(targetDirection * hit.distance, targetInRange, targetSpotted);

    if (targetSpotted)
    {
      // Raise event
      OnTargetSpotted.Invoke(hit.point);

      // Register hit
      targetSpottedLastFrame = true;

      // Reset prediction, if it's active
      if (prediction != null) {
        StopCoroutine(prediction);
        prediction = null;
      }
    }

    // If not spotted, see if target was lost
    else if (targetSpottedLastFrame)
    {
      // Check if there's prediction time, to start prediction coroutine
      if (predictionTime > 0f) prediction = StartCoroutine(PredictTargetLocation());
      else LoseTarget();

      targetSpottedLastFrame = false;
    }

    // If is currently predicting, only raise the event
    else if (prediction != null) OnTargetSpotted.Invoke(hit.point);
  }

  private IEnumerator PredictTargetLocation()
  {
    // Wait prediction time
    yield return new WaitForSeconds(predictionTime);

    // Coroutine hasn't been cancelled until now, so lose target
    LoseTarget();
  }

  private void LoseTarget()
  {
    // Register target loss
    targetSpottedLastFrame = false;
    prediction = null;

    // Raise loss event 
    OnTargetLost.Invoke();
  }

  private void DrawSightRay(Vector2 targetDirection, bool targetInRange, bool targetSpotted)
  {
    // The sight ray color logic
    Color sightRayColor;
    if (targetSpotted) sightRayColor = Color.green;
    else if (targetInRange) sightRayColor = Color.yellow;
    else sightRayColor = Color.red;

    // Visualize the sight ray
    Debug.DrawRay(transform.position, targetDirection, sightRayColor);
  }

  private bool TargetInSightCone(Vector2 targetDirection)
  {
    // This dot product is equal to the cosine of the vector's angle
    float targetAngleCosine = Vector2.Dot(facingDirection, targetDirection);
    float targetAngle = Mathf.Acos(targetAngleCosine) * Mathf.Rad2Deg;

    // Check if it's under the sight angle
    return targetAngle <= sightAngle;
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
