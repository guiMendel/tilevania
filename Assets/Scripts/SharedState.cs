using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SharedState : MonoBehaviour
{
  // This components simply keeps a map of state keys and values.
  // Different components may consult and alter the state key's values.
  Dictionary<string, string> stateMap;

  private void Awake()
  {
    stateMap = new Dictionary<string, string>();
  }

  // Interface

  // Returns what the current state for this stateKey is
  public string GetState(string stateKey)
  {
    string value = "";
    stateMap.TryGetValue(stateKey, out value);

    return value;
  }

  // Sets a new value for the given state key
  public string SetState(string stateKey, string value)
  {
    return stateMap[stateKey] = value;
  }

  // Checks if the provided state is the one stored in it's corresponding state key
  public bool IsStateActive(State state)
  {
    return GetState(state.GetStateKeyName()) == state.GetType().Name;
  }
}
