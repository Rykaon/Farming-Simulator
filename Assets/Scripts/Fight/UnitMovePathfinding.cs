using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Ink.Runtime;

public class UnitMovePathfinding : MonoBehaviour, IUnitMove
{
    Pathfinding pathfinding;
    [SerializeField] private PathNode pathNode;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private UnitMove unitMove;
    [SerializeField] private float reachedPathPositionDistance;
    private List<PathNode> nodeList;
    private int pathIndex = -1;

    private void Start()
    {
        pathfinding = PlayerManager.instance.pathfinding;
    }

    public void SetVelocity(Vector3 targetPos)
    {
        if (transform.gameObject.tag == "Player")
        {
            pathNode = playerManager.playerNode;
        }
        else
        {
            pathNode = transform.GetComponent<UnitManager>().unitNode;
        }

        nodeList = pathfinding.FindAreaPathMove(pathNode.x, pathNode.y, (int)Mathf.Round(targetPos.x), (int)Mathf.Round(targetPos.z));

        if (nodeList == null)
        {
            pathIndex = -1;
            return;
        }
        else
        {
            PathNode startNode = pathNode;
            startNode.isContainingUnit = false;
            startNode.unit = null;
            startNode.isWalkable = true;
            PathNode endNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(targetPos.x), (int)Mathf.Round(targetPos.z));
            endNode.isContainingUnit = true;
            endNode.unit = transform.gameObject;
            endNode.isWalkable = false;

            if (transform.tag == "Player")
            {
                playerManager.playerNode = endNode;
            }
            else
            {
                transform.GetComponent<UnitManager>().unitNode = endNode;
            }

            foreach (PathNode node in nodeList)
            {
                PathNode targetNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(node.x), (int)Mathf.Round(node.y));

                if (transform.tag == "Player")
                {
                    if (targetNode != startNode)
                    {
                        playerManager.currentDistanceMoved++;

                        if (playerManager.currentDistanceMoved == 1 || playerManager.currentDistanceMoved == 3 || playerManager.currentDistanceMoved == 5)
                        {
                            playerManager.currentActionsPoints--;
                        }
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
            PathNode nextNode = nodeList[pathIndex];
            Vector3 moveVelocity = new Vector3((nextNode.x - transform.position.x), 0, (nextNode.y - transform.position.z)).normalized;
            unitMove.SetVelocity(moveVelocity);
            Vector3 unitHeight = new Vector3(transform.position.x, 0, transform.position.z);

            if (Vector3.Distance(unitHeight, new Vector3(nextNode.x, 0, nextNode.y)) <= reachedPathPositionDistance)
            {
                ++pathIndex;
                if (pathIndex >= nodeList.Count)
                {
                    pathIndex = -1;
                }
            }
        }
        else
        {
            unitMove.SetVelocity(Vector3.zero);
        }
    }
}
