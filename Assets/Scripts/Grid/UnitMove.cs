using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class UnitMove : MonoBehaviour, IUnitMove
{
    [SerializeField] private float moveSpeed;

    private Action onReachedTargetPosition;
    private Vector3 velocityVector;

    private void Awake()
    {
        //characterBase = GetComponent<Character_Base>();
    }

    /*public void SetVelocity(Vector3 velocityVector, Action onReachedTargetPosition)
    {
        this.onReachedTargetPosition = onReachedTargetPosition;
        this.velocityVector = velocityVector;
    }*/

    public void SetVelocity(Vector3 velocityVector, MoveData moveData, Action onReachedTargetPosition)
    {
        this.onReachedTargetPosition = onReachedTargetPosition;
        this.velocityVector = velocityVector;
    }

    /*public void Disable()
    {
        this.enabled = false;
        velocityVector = Vector3.zero;
    }

    public void Enable()
    {
        this.enabled = true;
    }*/

    private void FixedUpdate()
    {
        transform.position += velocityVector * moveSpeed * Time.deltaTime;
    }
}