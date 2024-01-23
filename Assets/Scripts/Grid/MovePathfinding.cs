using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class MovePathfinding : MonoBehaviour, IUnitVelocity
{
    private GridSystem gridSystem;

    private Action onReachedTargetPosition;
    private List<Vector3> pathVectorList;
    private int pathIndex = -1;

    private void Start()
    {
        gridSystem = Camera.main.GetComponent<GridSystem>();
    }

    public void SetVelocity(List<Vector3> vectorList)
    {
        //this.onReachedTargetPosition = onReachedTargetPosition;
        pathVectorList = vectorList;

        if (pathVectorList == null)
        {
            pathIndex = -1;
        }
        else
        {
            pathIndex = 0;
        }

        Debug.Log("MaxMove = " + transform.GetComponent<UnitGridSystem>().remainingMovePoints);
    }

    public void SetVelocity(Vector3 velocityVector, Action onReachedTargetPosition)
    {
        this.onReachedTargetPosition = onReachedTargetPosition;
        //pathVectorList = vectorList;

        if (pathVectorList == null)
        {
            pathIndex = -1;
        }
        else
        {
            pathIndex = 0;
        }

        Debug.Log("MaxMove = " + transform.GetComponent<UnitGridSystem>().remainingMovePoints);
    }

    private void Update()
    {
        if (pathIndex != -1)
        {
            Vector3 nextPathPosition = pathVectorList[pathIndex];
            Vector3 moveVelocity = new Vector3((nextPathPosition.x - transform.position.x), nextPathPosition.y, (nextPathPosition.z - transform.position.z)).normalized;
            GetComponent<IUnitVelocity>().SetVelocity(moveVelocity, onReachedTargetPosition);

            float reachedPathPositionDistance = 1f;
            if (Vector3.Distance(transform.position, nextPathPosition) <= reachedPathPositionDistance /*transform.position == nextPathPosition*/)
            {
                ++pathIndex;
                if (pathIndex >= pathVectorList.Count)
                {
                    pathIndex = -1;
                    //onReachedTargetPosition();
                    PathfindingMe.instance.GetNode((int)Mathf.Round(transform.position.x), (int)Mathf.Round(transform.position.z)).SetIsContainingUnit(true, transform.gameObject);
                    gridSystem.ResetTileOutline();
                }
            }
        }
        else
        {
            GetComponent<IUnitVelocity>().SetVelocity(Vector3.zero, onReachedTargetPosition);
        }
    }
}
