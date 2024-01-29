using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Fight : MonoBehaviour
{
    public static PlayerController_Fight instance;

    [Header("Component References")]
    [SerializeField] private PlayerManager PC_Manager;

    private PlayerControls playerControls;
    private Pathfinding pathfinding;

    public RadialMenu rm;

    public bool isActive = true;
    public bool canUseTool = true;

    public bool LBRBisPressed = false;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        playerControls = PC_Manager.playerControls;
        pathfinding = PC_Manager.pathfinding;
        playerControls.Gamepad.Enable();
        playerControls.UI.Disable();
    }

    public List<GameObject> GetValidTarget(bool isMove, bool isAction, int range, PathNode startNode)
    {
        List<GameObject> objectList = new List<GameObject>();
        List<PathNode> pathList = new List<PathNode>();
        
        for (int i = startNode.x - range; i < startNode.x + range; ++i)
        {
            for (int j = startNode.y - range; j < startNode.y + range; ++j)
            {
                if (pathfinding.IsCoordsInGridRange(i, j))
                {
                    PathNode node = pathfinding.GetNodeWithCoords(i, j);

                    if (node != null)
                    {
                        if (!node.isVirtual)
                        {
                            if (isMove && !isAction)
                            {
                                if (!node.isContainingUnit)
                                {
                                    pathList = pathfinding.FindAreaPathMove(startNode.x, startNode.y, i, j);

                                    if (pathList != null)
                                    {
                                        if (pathList.Count <= range)
                                        {
                                            objectList.Add(node.tile);
                                        }
                                    }
                                }
                            }
                            else if (!isMove && isAction)
                            {
                                if (i == startNode.x || j == startNode.y)
                                {
                                    if (node.isContainingUnit)
                                    {
                                        if (node.unit != null)
                                        {
                                            if (node.unit.tag == "Unit")
                                            {
                                                Debug.Log("yo");
                                                pathList = pathfinding.FindLinearPathAction(startNode.x, startNode.y, i, j);
                                                
                                                if (pathList != null)
                                                {
                                                    if (pathList.Count <= range)
                                                    {
                                                        objectList.Add(node.unit);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (objectList.Count > 0)
        {
            return objectList;
        }
        else
        {
            return null;
        }
    }

    private void Update()
    {
        if (isActive)
        {
            if (playerControls.Gamepad.Y.IsPressed())
            {
                rm.gameObject.SetActive(true);
            }

            if (playerControls.Gamepad.X.IsPressed())
            {
                PC_Manager.moveRange = PC_Manager.maxMoveRange;
                PC_Manager.actionRange = PC_Manager.maxActionRange;
                PC_Manager.currentActionsPoints = PC_Manager.maxActionPoints;
                PC_Manager.currentDistanceMoved = 0;
                PC_Manager.isBoosted = false;
                PC_Manager.boostFactor = 0;
            }
        }
    }
}
