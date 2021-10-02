using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Description : MonoBehaviour
{
  [Tooltip("The game object's description")]
  [TextArea]
  public string description;
}
