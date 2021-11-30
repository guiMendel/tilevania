using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
  void OnDeathMessage()
  {
    print(42);
    Destroy(gameObject);
  }
}
