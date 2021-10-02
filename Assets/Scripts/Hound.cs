using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hound : MonoBehaviour
{
  // Params
  [SerializeField] float runSpeed = 5f;

  // State

  // Which direction the hound is facing (starts to the left)
  int direction = -1;

  // The original position of groundSensor
  Vector3 groundSensorOriginalPosition;

  // Refs
  Rigidbody2D _rigidbody;
  Animator _animator;
  Collider2D groundSensor;
  Collider2D wallSensor;

  // Start is called before the first frame update
  void Start()
  {
    GetComponentRefs();
  }

  private void Update()
  {
    DetectObstacles();
  }

  private void FixedUpdate()
  {
    Move();
  }

  void FlipDirection()
  {
    // Flip direction
    direction = -direction;

    // Flip sprite (it points to the left, so we need to use -direction)
    transform.localScale = new Vector2(-direction, 1f);
  }

  // Detects whether there is round ahead
  private void DetectObstacles()
  {
    // If wall or no floor ahead, flip direction
    if (!IsTouching("Ground", groundSensor) || IsTouching("Ground", wallSensor))
    {
      FlipDirection();
    }
  }

  // Whether or not the collider is currently touching the layer
  bool IsTouching(String layer, Collider2D collider)
  {
    // Get layer
    LayerMask layerMask = LayerMask.GetMask(layer);

    // See if collider is touching it
    return collider.IsTouchingLayers(layerMask);
  }

  private void Move()
  {
    // Apply movement
    _rigidbody.velocity = new Vector2(runSpeed * direction, _rigidbody.velocity.y);

    bool moving = Mathf.Abs(_rigidbody.velocity.x) > Mathf.Epsilon;

    if (moving) _animator.SetBool("Running", true);
    else _animator.SetBool("Running", false);

  }

  private void GetComponentRefs()
  {
    // Get refs
    _rigidbody = GetComponent<Rigidbody2D>();
    _animator = GetComponent<Animator>();
    groundSensor = transform.Find("Ground Sensor").GetComponent<Collider2D>();
    wallSensor = transform.Find("Wall Sensor").GetComponent<Collider2D>();

    // Report dependencies
    if (_rigidbody == null)
    {
      Debug.LogError(gameObject.name + " is missing " + _rigidbody.GetType().Name + " component");
    }
    if (_animator == null)
    {
      Debug.LogError(gameObject.name + " is missing " + _animator.GetType().Name + " component");
    }
    if (groundSensor == null)
    {
      Debug.LogError(gameObject.name + " is missing Ground Sensor child with rigidbody2D component");
    }
    if (wallSensor == null)
    {
      Debug.LogError(gameObject.name + " is missing Wall Sensor child with rigidbody2D component");
    }

  }
}
