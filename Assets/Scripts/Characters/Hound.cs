using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Deps
[RequireComponent(typeof(SharedState))]
[RequireComponent(typeof(CollisionSensor))]
[RequireComponent(typeof(GroundMovement))]
[RequireComponent(typeof(ChaseState))]

public class Hound : MonoBehaviour
{
  //=== Refs
  SharedState _sharedState;
  CollisionSensor _collisionSensor;
  GroundMovement _groundMovement;
  ChaseState _chaseState;

  private void Awake()
  {
    _sharedState = GetComponent<SharedState>();
    _collisionSensor = GetComponent<CollisionSensor>();
    _groundMovement = GetComponent<GroundMovement>();
    _chaseState = GetComponent<ChaseState>();
  }

  private void Start()
  {
    // When chasing, jump whenever facing a wall
    var wallSensor = _collisionSensor.GetSensorByGameObjectName("Wall Sensor");

    wallSensor.OnSensorStay.AddListener(JumpIfChasing);
  }

  private void JumpIfChasing()
  {
    if (_sharedState.IsStateActive(_chaseState)) _groundMovement.Jump();
  }
}
