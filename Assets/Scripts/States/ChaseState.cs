using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
  //=== Params
  [Tooltip("How long the character stays still before chasing the target, when the state is enabled")]
  [Min(0f)] [SerializeField] float headsUpDelay = 0.5f;

  //=== State

  // Which target is currently being chased
  Vector2 currentTarget;

  // Whether the character is currently standing still in the heads up interval
  bool inHeadsUp;

  Coroutine headsUpCoroutine;

  //=== Refs
  MovementInterface movementComponent;

  protected override void OnAwake()
  {
    // Get refs
    movementComponent = GetComponent<MovementInterface>();

    if (movementComponent == null)
    {
      Debug.LogError(gameObject.name + " is missing " + movementComponent.GetType().Name + " component");
    }
  }

  protected override string GetDefaultStateKeyName() => "movement";

  protected override void OnUpdate()
  {
    if (isCurrentState && !inHeadsUp)
    {
      Chase();
    }
  }

  protected override void OnStateEnable()
  {
    if (headsUpDelay != 0f) headsUpCoroutine = StartCoroutine(HeadsUp());
  }

  protected override void OnStateDisable()
  {
    StopCoroutine(headsUpCoroutine);
  }

  private void Chase()
  {
    // Get target direction
    float direction = Mathf.Sign(currentTarget.x - transform.position.x);

    // Move towards target
    movementComponent.Move(direction);
  }

  // Waits the configured time and then sets the target for chase
  IEnumerator HeadsUp()
  {
    // Enter heads up mode
    inHeadsUp = true;

    yield return new WaitForSeconds(headsUpDelay);

    inHeadsUp = false;
  }

  //=== Interface

  public void SetTarget(Vector2 target)
  {
    // Set the target
    currentTarget = target;

    if (!isCurrentState) Enable();
  }
}
