using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionSensor : MonoBehaviour
{
  // Define the sensors' type
  [Serializable]
  public struct Sensor
  {
    [Tooltip("Attach the collider that will serve as a sensor")]
    public Collider2D sensorCollider;
    [Tooltip("Which layers will the sensor sense")]
    public LayerMask layersToSense;
    [Tooltip("Event subject for sensor collision enter")]
    public UnityEvent OnSensorEnter;
    [Tooltip("Event subject for sensor collision exit")]
    public UnityEvent OnSensorExit;
    [Tooltip("Event subject for sensor collision stay")]
    public UnityEvent OnSensorStay;

    // Used to trigger enter & exit events
    [NonSerialized] public bool isTriggering;
  }

  [Tooltip("Defines sensors attached to the game object's colliders, as well as listeners to these sensors")]
  public Sensor[] sensors;

  private void Update()
  {
    // foreach (Sensor sensor in sensors)
    for (int i = 0; i < sensors.Length; i++)
    {
      // Detect collision
      if (sensors[i].sensorCollider.IsTouchingLayers(sensors[i].layersToSense))
      {
        // Raise stay event
        sensors[i].OnSensorStay.Invoke();

        // Check whether to raise enter event
        if (!sensors[i].isTriggering)
        {
          sensors[i].OnSensorEnter.Invoke();
          sensors[i].isTriggering = true;
        }
      }

      // If no collision, check whether to trigger exit event
      else if (sensors[i].isTriggering)
      {
        sensors[i].OnSensorExit.Invoke();
        sensors[i].isTriggering = false;
      }
    }
  }

}
