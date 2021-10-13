using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hound : MonoBehaviour
{
  // Params
  [SerializeField] float runSpeed = 5f;

  [Header("Chase variables")]
  [SerializeField] float jumpPower = 10f;

  [Header("Idle movement variables")]
  [SerializeField] float moveDurationMin = 0.5f;
  [SerializeField] float moveDurationMax = 2f;
  [SerializeField] float idleDurationMin = 1f;
  [SerializeField] float idleDurationMax = 5f;

  // State

  // Whether it's moving or idle
  bool idleMoving = false;

  // Whether it has a target position to chase
  Vector3 target;

  // Which direction the hound is facing (starts to the left)
  int direction = -1;

  // The original position of groundSensor
  Vector3 groundSensorOriginalPosition;

  // Reference to idle coroutine
  Coroutine idleCoroutine;

  // Refs
  Rigidbody2D _rigidbody;
  Animator _animator;
  Collider2D groundSensor;
  Collider2D wallSensor;
  Collider2D feet;

  // Start is called before the first frame update
  void Start()
  {
    GetComponentRefs();

    // Decide random movements
    // idleCoroutine = StartCoroutine(DecideMovement());
  }

  private void Update()
  {
    DetectObstacles();

    // Chase player
    target = FindObjectOfType<Player>().transform.position;
  }

  private void FixedUpdate()
  {
    Move();

    // Detect if airborne
    DetectAirborne();

    // Detect movement
    DetectMovement();
  }

  private void DetectMovement()
  {
    // Detect y movement
    float xMovement = Mathf.Abs(_rigidbody.velocity.x);

    // Update animation
    _animator.SetBool("Running", xMovement > 0.05f);
  }

  private void DetectAirborne()
  {
    // Detect y movement
    float yMovement = Mathf.Abs(_rigidbody.velocity.y);

    // Detect significant vertical movement
    _animator.SetBool("Airborne", yMovement > 0.1f);
  }

  // Decides when to randomly move
  private IEnumerator DecideMovement()
  {
    // Never stop
    while (true)
    {
      // Decide when to change state
      float min = idleMoving ? moveDurationMin : idleDurationMin;
      float max = idleMoving ? moveDurationMax : idleDurationMax;

      float stateChangeTimeout = Random.Range(min, max);

      // Don't execute if is currently chasing
      yield return new WaitUntil(() => target == null);

      // Wait this time
      yield return new WaitForSeconds(stateChangeTimeout);

      // Change state
      idleMoving = !idleMoving;

      // Randomly switch direction
      if (idleMoving && Random.value < 0.5f) FlipDirection();
    }
  }

  void SetDirection(int newDirection)
  {
    // Flip direction
    direction = newDirection;

    // Flip sprite (it points to the left, so we need to use -newDirection)
    transform.localScale = new Vector2(-newDirection, 1f);
  }

  void FlipDirection() => SetDirection(-direction);

  // Detects whether there is round ahead
  private void DetectObstacles()
  {
    // When chasing
    if (target != null)
    {
      // Jump if encounters a wall
      if (IsTouching("Ground", wallSensor)) Jump();
      return;
    }

    // When idle moving
    if (idleMoving)
    {
      // If wall or no floor ahead, flip direction
      if (!IsTouching("Ground", groundSensor) || IsTouching("Ground", wallSensor))
      {
        FlipDirection();
      }
      return;
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
    float movement = 0f;

    // If chasing
    if (target != null)
    {
      // Keep facing target
      SetDirection((int)Mathf.Sign(target.x - transform.position.x));

      movement = runSpeed * direction;
    }

    // If idly moving
    else if (idleMoving) movement = runSpeed * direction;

    // Apply movement
    _rigidbody.velocity = new Vector2(movement, _rigidbody.velocity.y);
  }

  private void Jump()
  {
    // Ensure it's grounded
    if (!IsTouching("Ground", feet)) return;

    // Add y velocity
    _rigidbody.velocity = new Vector2(_rigidbody.velocity.y, jumpPower);
  }

  private void GetComponentRefs()
  {
    // Get refs
    _rigidbody = GetComponent<Rigidbody2D>();
    _animator = GetComponent<Animator>();
    groundSensor = transform.Find("Ground Sensor").GetComponent<Collider2D>();
    wallSensor = transform.Find("Wall Sensor").GetComponent<Collider2D>();
    feet = transform.Find("Feet").GetComponent<Collider2D>();

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
    if (feet == null)
    {
      Debug.LogError(gameObject.name + " is missing Feet child with rigidbody2D component");
    }

  }
}
