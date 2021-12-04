using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class MovementPrediction
{

  // Returns two values: one for positive angle, one for negative angle
  public static Vector2 PredictTargetMovement(Transform transform, Transform target, float projectileVelocity)
  {
    // Make sure target has velocity
    Rigidbody2D targetBody = target.GetComponent<Rigidbody2D>();
    if (targetBody == null || (Vector2)targetBody.velocity == Vector2.zero)
    {
      return (target.position - transform.position).normalized;
    }

    // Get predicted angles
    var (positiveAngle, negativeAngle) = GetPredictionAngles(transform, target, projectileVelocity);

    // If the angles are NaN, prediction isn't possible in this scenario
    if (Double.IsNaN(positiveAngle)) return (target.position - transform.position).normalized;

    // If target is below, use negative angle. Otherwise, use positive angle
    return Quaternion.Euler(
      0, 0, transform.position.y > target.position.y ? negativeAngle : positiveAngle
    ) * Vector2.right;
  }

  static (float, float) GetPredictionAngles(Transform transform, Transform target, float projectileVelocity)
  {
    // Get target's velocities
    Vector2 targetVelocity = target.GetComponent<Rigidbody2D>().velocity;
    var (vy, vx) = (targetVelocity.y, targetVelocity.x);

    // Get xDistance and yDistance
    float xDistance = target.position.x - transform.position.x;
    float yDistance = target.position.y - transform.position.y;

    // Get the velocity factor
    float velocityFactor = (xDistance * vy - yDistance * vx) / projectileVelocity;

    // Get hypotheneuse
    float hypotheneuse = Mathf.Sqrt(Mathf.Pow(xDistance, 2) + Mathf.Pow(yDistance, 2));

    // Get alfa angle
    float alfaAngle = Mathf.Atan(xDistance / yDistance) * Mathf.Rad2Deg;

    return (
      Mathf.Acos(velocityFactor / hypotheneuse) * Mathf.Rad2Deg - alfaAngle,
      -Mathf.Acos(velocityFactor / hypotheneuse) * Mathf.Rad2Deg - alfaAngle
    );
  }
}
