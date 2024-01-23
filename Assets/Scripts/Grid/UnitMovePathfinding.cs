using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class UnitMovePathfinding : MonoBehaviour, IUnitMove
{
    private GridSystem gridSystem;
    Pathfinding pathfinding;
    private MoveData moveData;
    private Vector3 startPos;
    private List<Vector3> pathVectorList;
    private int pathIndex = -1;

    public GameObject oil;
    public int FireDoTDamage;

    public bool isInitialized = false;
    Action onReachedTargetPosition;

    private void Start()
    {
        gridSystem = Camera.main.GetComponent<GridSystem>();
        pathfinding = Pathfinding.instance;
    }

    public void SetVelocity(Vector3 targetPos, MoveData moveAbility, Action onReachedTargetPosition)
    {
        moveData = moveAbility;
        startPos = new Vector3((int)Mathf.Round(transform.position.x), 1f, (int)Mathf.Round(transform.position.z));
        pathVectorList = pathfinding.FindPathMove(startPos, targetPos, moveData);
        //Debug.Log(targetPos);
        

        if (pathVectorList == null)
        {
            pathIndex = -1;
            return;
        }
        else
        {
            pathfinding.GetNodeWithCoords((int)Mathf.Round(startPos.x), (int)Mathf.Round(startPos.z)).SetIsContainingUnit(false, null);
            pathfinding.GetNodeWithCoords((int)Mathf.Round(targetPos.x), (int)Mathf.Round(targetPos.z)).SetIsContainingUnit(true, transform.gameObject);
            foreach (Vector3 pathVector in pathVectorList)
            {
                PathNode targetNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(pathVector.x), (int)Mathf.Round(pathVector.z));
                PathNode startNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(startPos.x), (int)Mathf.Round(startPos.z));

                if (targetNode != startNode)
                {
                    if (moveData.abilityName == "Vidange")
                    {
                        GameObject newOil = Instantiate(oil, new Vector3(targetNode.x, 0, targetNode.y), Quaternion.identity);
                        //newOil.transform.SetParent(gridSystem.tileGrid.GetGridObject(targetNode.x, targetNode.y).transform);
                        newOil.transform.GetChild(0).gameObject.SetActive(false);
                        targetNode.SetIsOil(newOil);
                    }

                    if (targetNode.isFire)
                    {
                        for (int i = 0; i < transform.childCount; ++i)
                        {
                            if (transform.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().lifePoints <= FireDoTDamage)
                            {
                                transform.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().InflictDamage(FireDoTDamage);
                                break;
                            }
                            else
                            {
                                transform.GetComponent<UnitGridSystem>().implants[i].GetComponent<ImplantSystem>().InflictDamage(FireDoTDamage);
                            }
                        }
                    }
                }

                if (transform.GetComponent<UnitGridSystem>().hasSpiderWebToBeRemove)
                {
                    if (transform.GetComponent<UnitGridSystem>().spiderWeb != null)
                    {
                        Destroy(transform.GetComponent<UnitGridSystem>().spiderWeb);
                    }
                }
            }

            pathIndex = 0;
        }

        
    }

    private void Update()
    {
        if (pathIndex != -1)
        {
            Vector3 nextPathPosition = pathVectorList[pathIndex];
            Vector3 moveVelocity = new Vector3((nextPathPosition.x - transform.position.x), nextPathPosition.y, (nextPathPosition.z - transform.position.z)).normalized;
            GetComponent<IUnitMove>().SetVelocity(moveVelocity, moveData, onReachedTargetPosition);
            Vector3 unitHeight = new Vector3(transform.position.x, 1, transform.position.z);
            //Debug.Log(pathIndex);
            float reachedPathPositionDistance = 1f;
            if (Vector3.Distance(unitHeight, nextPathPosition) <= reachedPathPositionDistance)
            {
                ++pathIndex;
                if (pathIndex >= pathVectorList.Count)
                {
                    pathIndex = -1;
                    gridSystem.SetEnergeticTower(transform.GetComponent<UnitGridSystem>().team);
                    gridSystem.ResetTileOutline();
                }
            }
        }
        else
        {
            GetComponent<IUnitMove>().SetVelocity(Vector3.zero, moveData, onReachedTargetPosition);
        }

        if (Camera.main.transform.childCount > 0 && isInitialized)
        {
            //transform.LookAt(Camera.main.transform.GetChild(0).position);

            switch (Mathf.Round(Camera.main.transform.rotation.eulerAngles.y))
            {
                case 45:
                    transform.rotation = Quaternion.Euler(0, 45, 0);
                    break;

                case 135:
                    transform.rotation = Quaternion.Euler(0, 135, 0);
                    break;

                case 225:
                    transform.rotation = Quaternion.Euler(0, 225, 0);
                    break;

                case 315:
                    transform.rotation = Quaternion.Euler(0, 315, 0);
                    break;
            }
        }
    }
}
