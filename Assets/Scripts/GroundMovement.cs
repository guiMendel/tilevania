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
  [Tooltip("Whether to invert the direction the character faces")]
  [SerializeField] bool invertFacingDirection;

  // Refs
  Rigidbody2D _rigidbody;


  private void Start()
  {
    // Get components
    GetComponentRefs();
  }

  private void Update()
  {
    UpdateFacingDirection();
  }

  private void UpdateFacingDirection()
  {
    // Ignore small movements
    if (Mathf.Abs(_rigidbody.velocity.x) > 0.1f)
    {
      // Keep facing direction updated
      float direction = invertFacingDirection ? -1 : 1;
      transform.localScale = new Vector2(Mathf.Sign(_rigidbody.velocity.x) * direction, 1f);
    }
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
