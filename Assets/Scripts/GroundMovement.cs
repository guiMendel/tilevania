using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component dependencies
[RequireComponent(typeof(Rigidbody2D))]

public class GroundMovement : MonoBehaviour
{
  // Params
  [Tooltip("The base speed in which the character moves")]
  [SerializeField] float baseSpeed = 5f;

  // Refs
  Rigidbody2D _rigidbody;


  private void Start()
  {
    // Get components
    GetComponentRefs();
  }

  private void GetComponentRefs()
  {
    _rigidbody = GetComponent<Rigidbody2D>();

    // Report dependencies
    if (_rigidbody == null)
    {
      Debug.LogError(gameObject.name + " is missing " + _rigidbody.GetType().Name + " component");
    }
  }

  // Interface

  // Apply movement to character. The movementModifier param can alter the direction as well as the speed
  public void Move(float movementModifier = 1f)
  {
    _rigidbody.velocity = new Vector2(baseSpeed * movementModifier, _rigidbody.velocity.y);
  }

}
