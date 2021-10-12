using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SharedState : MonoBehaviour
{
  // This components simply keeps a map of state keys and values.
  // Different components may consult and alter the state key's values.
  Dictionary<string, string> stateMap;

  private void Start()
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

  // Returns what the current state for this stateKey is
  public string SetState(string stateKey, string value)
  {
    return stateMap[stateKey] = value;
  }
}
