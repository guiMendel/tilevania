using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
  // State

  // Which target is currently being chased
  Vector2 currentTarget;

  // Refs
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
    if (isCurrentState)
    {
      Chase();
    }
  }

  private void Chase()
  {
    // Get target direction
    float direction = Mathf.Sign(currentTarget.x - transform.position.x);

    // Move towards target
    movementComponent.Move(direction);
  }

  // Interface

  public void SetTarget(Vector2 target)
  {
    Enable();
    currentTarget = target;
  }
}
