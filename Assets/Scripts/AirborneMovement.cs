using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneMovement : Movement
{
  protected override void MoveTowards(Vector2 movementWithInertia, Vector2 originalMovement)
  {
    // Apply it
    _rigidbody.velocity = movementWithInertia;
  }
}
