using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface MovementInterface
{
  void Move(float movementModifier);
  void SetFacingDirection(float direction);
}
