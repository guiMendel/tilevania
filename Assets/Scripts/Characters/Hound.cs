using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Deps
[RequireComponent(typeof(SharedState))]
[RequireComponent(typeof(CollisionSensor))]
[RequireComponent(typeof(GroundMovement))]

public class Hound : MonoBehaviour
{
  //=== Refs
  SharedState _sharedState;
  CollisionSensor _collisionSensor;
  GroundMovement _groundMovement;

  private void Awake()
  {
    _sharedState = GetComponent<SharedState>();
    _collisionSensor = GetComponent<CollisionSensor>();
    _groundMovement = GetComponent<GroundMovement>();
  }

  private void Start()
  {
    // When chasing, jump whenever facing a wall
    Collider2D wallCollider = transform.Find("Wall Sensor").GetComponent<Collider2D>();

    var wallSensor = Array.Find(
      _collisionSensor.sensors,
      sensor => GameObject.Equals(sensor.sensorCollider, wallCollider)
    );

    wallSensor.OnSensorStay.AddListener(JumpIfChasing);
  }

  private void JumpIfChasing()
  {
    if (_sharedState.GetState("movement") == typeof(ChaseState).Name)
    {
      _groundMovement.Jump();
    }
  }
}
