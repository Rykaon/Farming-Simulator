using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnitVelocity
{
    void SetVelocity(Vector3 velocityVector, Action onReachedTargetPosition);
    void SetVelocity(List<Vector3> vectorList);
}
