using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public interface IUnitMove
{
    //void SetVelocity(Vector3 velocityVector, MoveData moveData);
    //void SetVelocity(Vector3 velocityVector, Action onReachedTargetPosition);
    void SetVelocity(Vector3 velocityVector, Action onReachedTargetPosition);
    /*void Disable();
    void Enable();*/
}
