using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class UnitMove : MonoBehaviour, IUnitMove
{
    [SerializeField] private float moveSpeed;

    private Vector3 velocityVector;

    public void SetVelocity(Vector3 velocityVector)
    {
        this.velocityVector = velocityVector;
    }

    public void Disable()
    {
        this.enabled = false;
        velocityVector = Vector3.zero;
    }

    public void Enable()
    {
        this.enabled = true;
    }

    private void FixedUpdate()
    {
        transform.position += velocityVector * moveSpeed * Time.deltaTime;
    }
}