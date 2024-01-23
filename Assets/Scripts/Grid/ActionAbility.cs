using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class ActionAbility : MonoBehaviour
{
    GameObject unit;
    UnitGridSystem unitGridSystem;
    Pathfinding pathfinding;
    public MoveData basicLinearMove;
    public ActionData actionAbility;
    private int index;
    public bool hasBeenUsed = false;

    public GameObject spiderWebPrefab;

    private void Awake()
    {
        //audioManager = Camera.main.GetComponent<AudioManager>();
    }

    void Start()
    {
        pathfinding = Pathfinding.instance;
        if (transform.parent != null)
        {
            InitAbility();
        }
    }

    public void InitAbility()
    {
        unit = transform.parent.gameObject;
        unitGridSystem = unit.GetComponent<UnitGridSystem>();

        if (actionAbility == null)
        {
            Debug.Log(transform.name + " / ACTION_ABILITY == NULL");
        }
        else
        {
            if (actionAbility.abilityName == "Tir de Mortiers")
            {
                unitGridSystem.movePoints -= 3;
            }

            unitGridSystem.actionPoints += actionAbility.actionPoints;
        }

        string firstTagChar = "";
        firstTagChar = firstTagChar + transform.name[0];

        if (firstTagChar == "L")
        {
            transform.GetComponent<SpriteRenderer>().sprite = actionAbility.spriteLeft;
        }
        else if (firstTagChar == "H")
        {
            switch (unitGridSystem.team)
            {
                case UnitGridSystem.Team.Orange:
                    transform.GetComponent<SpriteRenderer>().sprite = actionAbility.spriteLeft;
                    break;

                case UnitGridSystem.Team.Blue:
                    transform.GetComponent<SpriteRenderer>().sprite = actionAbility.spriteRight;
                    break;
            }
        }
        else
        {
            transform.GetComponent<SpriteRenderer>().sprite = actionAbility.spriteRight;
        }

        index = GetIndexOfAbilityInList(unitGridSystem.actionDataList, actionAbility);
    }

    public void UpdateSprite(ActionData action)
    {
        string firstTagChar = "";
        firstTagChar = firstTagChar + transform.name[0];

        if (firstTagChar == "L")
        {
            transform.GetComponent<SpriteRenderer>().sprite = action.spriteLeft;
        }
        else if (firstTagChar == "H")
        {
            switch (unitGridSystem.team)
            {
                case UnitGridSystem.Team.Orange:
                    transform.GetComponent<SpriteRenderer>().sprite = action.spriteLeft;
                    break;

                case UnitGridSystem.Team.Blue:
                    transform.GetComponent<SpriteRenderer>().sprite = action.spriteRight;
                    break;
            }
        }
        else
        {
            transform.GetComponent<SpriteRenderer>().sprite = action.spriteRight;
        }
    }

    public void DisableAbility()
    {
        Debug.Log(transform.name);
        unit = transform.parent.gameObject;
        unitGridSystem = unit.GetComponent<UnitGridSystem>();

        if (actionAbility.abilityName == "Trancheur Laser")
        {
            unitGridSystem.actionPoints += actionAbility.actionPoints;
        }
        else
        {
            unitGridSystem.actionPoints -= actionAbility.actionPoints;
        }

        if (actionAbility.abilityName == "Tir de Mortiers")
        {
            unitGridSystem.movePoints += 3;
        }

        if (GetIndexOfAbilityInList(unitGridSystem.actionDataList, actionAbility) >= 0)
        {
            unitGridSystem.actionDataList.RemoveAt(index);
            unitGridSystem.actionAbilityList.RemoveAt(index);
        }

        actionAbility = null;
        transform.GetComponent<SpriteRenderer>().sprite = null;
    }

    public void ChangeAbility(ActionData newActionData)
    {
        if (actionAbility != null)
        {
            //index = GetIndexOfAbilityInList(unitGridSystem.actionAbilities, actionAbility);
            DisableAbility();
        }

        actionAbility = newActionData;
        Debug.Log(newActionData.abilityName);

        if (index != -1)
        {
            unitGridSystem.actionDataList.Insert(index, actionAbility);
            unitGridSystem.actionAbilityList.Insert(index, this);
        }
        else
        {
            unitGridSystem.actionDataList.Add(actionAbility);
            unitGridSystem.actionAbilityList.Add(this);
        }

        InitAbility();
    }

    public void CustomChangeAbility(ActionData newActionData)
    {
        if (actionAbility == null)
        {
            index = -1;
            DisableAbility();
        }

        actionAbility = newActionData;
        if (index != -1)
        {
            unitGridSystem.actionDataList.Insert(index, actionAbility);
            unitGridSystem.actionAbilityList.Insert(index, this);
        }
        else
        {
            unitGridSystem.actionDataList.Add(actionAbility);
            unitGridSystem.actionAbilityList.Add(this);
        }

        InitAbility();
    }

    private int GetIndexOfAbilityInList(List<ActionData> abilityList, ActionData ability)
    {
        int index = -1;
        
        if (ability != null)
        {
            for (int i = 0; i < abilityList.Count; ++i)
            {
                if (ability == abilityList[i])
                {
                    index = i;
                }
            }
        }

        this.index = index;

        return this.index;
    }

    public void ApplyAbility(GameObject targetUnit, int implantIndex, GameObject nexus, PathNode targetNode)
    {
        PathNode unitNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(unit.transform.position.x), (int)Mathf.Round(unit.transform.position.z));
        PathNode targetUnitNode = null;
        if (targetUnit != null)
        {
            targetUnitNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(targetUnit.transform.position.x), (int)Mathf.Round(targetUnit.transform.position.z));
        }
        PathNode nexusNode = null;
        if (nexus != null)
        {
            nexusNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(nexus.transform.position.x), (int)Mathf.Round(nexus.transform.position.z));
        }
        PathNode nodeToMove = null;
        PathNode moveNode = null;
        List<GameObject> unitList = new List<GameObject>();
        PathNode unitListNode = null;

        float audioLenght = 0;
        Debug.Log(actionAbility.abilityName);
        switch (actionAbility.abilityName)
        {
            case "Grappin":
                if (targetUnitNode != null)
                {
                    if (unitNode.x == targetUnitNode.x)
                    {
                        if (unitNode.y < targetUnitNode.y)
                        {
                            nodeToMove = pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + 1);
                            for (int i = 1; i <= targetUnitNode.y - nodeToMove.y; ++i)
                            {
                                if (!pathfinding.GetNodeWithCoords(nodeToMove.x, targetUnitNode.y - i).isWalkable || pathfinding.GetNodeWithCoords(nodeToMove.x, targetUnitNode.y - i).isContainingUnit)
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(nodeToMove.x, (int)Mathf.Round(targetUnit.transform.position.z) - (i - 1));
                                    break;
                                }
                                else
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(nodeToMove.x, (int)Mathf.Round(targetUnit.transform.position.z) - i);
                                }
                            }
                        }
                        else if (unitNode.y > targetUnitNode.y)
                        {
                            nodeToMove = pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - 1);
                            for (int i = 1; i <= nodeToMove.y - targetUnitNode.y; ++i)
                            {
                                if (!pathfinding.GetNodeWithCoords(nodeToMove.x, targetUnitNode.y + i).isWalkable || pathfinding.GetNodeWithCoords(nodeToMove.x, targetUnitNode.y + i).isContainingUnit)
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(nodeToMove.x, targetUnitNode.y + (i - 1));
                                    break;
                                }
                                else
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(nodeToMove.x, targetUnitNode.y + i);
                                }
                            }
                        }
                    }
                    else if (unitNode.y == targetUnitNode.y)
                    {
                        if (unitNode.x < targetUnitNode.x)
                        {
                            nodeToMove = pathfinding.GetNodeWithCoords(unitNode.x + 1, unitNode.y);
                            for (int i = 1; i <= targetUnitNode.x - nodeToMove.x; ++i)
                            {
                                if (!pathfinding.GetNodeWithCoords(targetUnitNode.x - i, nodeToMove.y).isWalkable || pathfinding.GetNodeWithCoords(targetUnitNode.x - i, nodeToMove.y).isContainingUnit)
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(targetUnitNode.x - (i - 1), nodeToMove.y);
                                    break;
                                }
                                else
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(targetUnitNode.x - i, nodeToMove.y);
                                }
                            }
                        }
                        else if (unitNode.x > targetUnitNode.x)
                        {
                            nodeToMove = pathfinding.GetNodeWithCoords(unitNode.x - 1, targetUnitNode.y);
                            for (int i = 1; i <= nodeToMove.x - targetUnitNode.x; ++i)
                            {
                                if (!pathfinding.GetNodeWithCoords(targetUnitNode.x + i, nodeToMove.y).isWalkable || pathfinding.GetNodeWithCoords(targetUnitNode.x + i, nodeToMove.y).isContainingUnit)
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(targetUnitNode.x + (i - 1), nodeToMove.y);
                                    break;
                                }
                                else
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(targetUnitNode.x + i, nodeToMove.y);
                                }
                            }
                        }
                    }

                    if (moveNode != null)
                    {
                        Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                        targetUnitNode.SetIsContainingUnit(false, null);
                        targetUnit.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                    }
                    else
                    {
                        Debug.Log(actionAbility.name + " = NO TARGET NODE FOUND TO MOVE GRABBED UNIT");
                    }

                    if (implantIndex > -1)
                    {
                        targetUnit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                    }
                }
                break;

            case "Pression Hydraulique":
                if (unitNode.x == targetNode.x)
                {
                    if (unitNode.y > targetNode.y)
                    {
                        Debug.Log("x = x, y > y");
                        for (int i = 0; i < actionAbility.range; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)) != null)
                            {
                                pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).SetIsFire(false);

                                if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit != unit)
                                    {
                                        unitList.Add(pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit);
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }

                        if (unitList.Count > 0)
                        {
                            for (int i = 0; i < actionAbility.range - 1; ++i)
                            {
                                if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)) != null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).isWalkable && !pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).isContainingUnit)
                                    {
                                        nodeToMove = pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i));
                                        break;
                                    }
                                }
                            }

                            if (nodeToMove != null)
                            {
                                foreach (GameObject unit in unitList)
                                {
                                    unitListNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(unit.transform.position.x), (int)Mathf.Round(unit.transform.position.z));

                                    for (int i = 1; i <= unitListNode.y - nodeToMove.y; ++i)
                                    {
                                        if (!pathfinding.GetNodeWithCoords(nodeToMove.x, unitListNode.y - i).isWalkable || pathfinding.GetNodeWithCoords(nodeToMove.x, unitListNode.y - i).isContainingUnit)
                                        {
                                            moveNode = pathfinding.GetNodeWithCoords(nodeToMove.x, unitListNode.y - (i - 1));
                                            break;
                                        }
                                        else
                                        {
                                            moveNode = pathfinding.GetNodeWithCoords(nodeToMove.x, unitListNode.y - i);
                                        }
                                    }

                                    if (moveNode != null)
                                    {
                                        Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                                        unitListNode.SetIsContainingUnit(false, null);
                                        unit.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                                    }
                                    else
                                    {
                                        Debug.Log(actionAbility.name + " = NO TARGET NODE FOUND TO MOVE UNIT");
                                    }
                                }
                            }

                            foreach (GameObject unit in unitList)
                            {
                                for (int i = 0; i < unit.transform.childCount; ++i)
                                {
                                    if (unit.transform.GetChild(3).GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                    {
                                        unit.transform.GetChild(3).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                        break;
                                    }
                                    else
                                    {
                                        unit.transform.GetChild(i).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                    else if (unitNode.y < targetNode.y)
                    {
                        Debug.Log("x = x, y < y");
                        for (int i = 0; i < actionAbility.range; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)) != null)
                            {
                                pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).SetIsFire(false);

                                if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit != unit)
                                    {
                                        unitList.Add(pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit);
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }

                        if (unitList.Count > 0)
                        {
                            for (int i = 0; i < actionAbility.range - 1; ++i)
                            {
                                if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)) != null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).isWalkable && !pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).isContainingUnit)
                                    {
                                        nodeToMove = pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i));
                                        break;
                                    }
                                }
                            }

                            if (nodeToMove != null)
                            {
                                foreach (GameObject unit in unitList)
                                {
                                    unitListNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(unit.transform.position.x), (int)Mathf.Round(unit.transform.position.z));

                                    for (int i = 1; i <= nodeToMove.y - unitListNode.y; ++i)
                                    {
                                        if (!pathfinding.GetNodeWithCoords(nodeToMove.x, unitListNode.y + i).isWalkable || pathfinding.GetNodeWithCoords(nodeToMove.x, unitListNode.y + i).isContainingUnit)
                                        {
                                            moveNode = pathfinding.GetNodeWithCoords(nodeToMove.x, unitListNode.y + (i - 1));
                                            break;
                                        }
                                        else
                                        {
                                            moveNode = pathfinding.GetNodeWithCoords(nodeToMove.x, unitListNode.y + i);
                                        }
                                    }

                                    if (moveNode != null)
                                    {
                                        Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                                        unitListNode.SetIsContainingUnit(false, null);
                                        unit.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                                    }
                                    else
                                    {
                                        Debug.Log(actionAbility.name + " = NO TARGET NODE FOUND TO MOVE UNIT");
                                    }
                                }
                            }

                            foreach (GameObject unit in unitList)
                            {
                                for (int i = 0; i < unit.transform.childCount; ++i)
                                {
                                    if (unit.transform.GetChild(3).GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                    {
                                        unit.transform.GetChild(3).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                        break;
                                    }
                                    else
                                    {
                                        unit.transform.GetChild(i).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (unitNode.y == targetNode.y)
                {
                    if (unitNode.x > targetNode.x)
                    {
                        Debug.Log("y = y, x > x");
                        for (int i = 0; i < actionAbility.range; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y) != null)
                            {
                                pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).SetIsFire(false);

                                if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit != unit)
                                    {
                                        unitList.Add(pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit);
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }

                        if (unitList.Count > 0)
                        {
                            for (int i = 0; i < actionAbility.range - 1; ++i)
                            {
                                if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y) != null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).isWalkable && !pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).isContainingUnit)
                                    {
                                        nodeToMove = pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y);
                                        break;
                                    }
                                }
                            }

                            if (nodeToMove != null)
                            {
                                foreach (GameObject unit in unitList)
                                {
                                    unitListNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(unit.transform.position.x), (int)Mathf.Round(unit.transform.position.z));

                                    for (int i = 1; i <= unitListNode.x - nodeToMove.x; ++i)
                                    {
                                        if (!pathfinding.GetNodeWithCoords(unitListNode.x - i, nodeToMove.y).isWalkable || pathfinding.GetNodeWithCoords(unitListNode.x - i, nodeToMove.y).isContainingUnit)
                                        {
                                            moveNode = pathfinding.GetNodeWithCoords(unitListNode.x - (i - 1), nodeToMove.y);
                                            break;
                                        }
                                        else
                                        {
                                            moveNode = pathfinding.GetNodeWithCoords(unitListNode.x - i, nodeToMove.y);
                                        }
                                    }

                                    if (moveNode != null)
                                    {
                                        Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                                        unitListNode.SetIsContainingUnit(false, null);
                                        unit.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                                    }
                                    else
                                    {
                                        Debug.Log(actionAbility.name + " = NO TARGET NODE FOUND TO MOVE UNIT");
                                    }
                                }
                            }

                            foreach (GameObject unit in unitList)
                            {
                                for (int i = 0; i < unit.transform.childCount; ++i)
                                {
                                    if (unit.transform.GetChild(3).GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                    {
                                        unit.transform.GetChild(3).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                        break;
                                    }
                                    else
                                    {
                                        unit.transform.GetChild(i).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                    else if (unitNode.x < targetNode.x)
                    {
                        Debug.Log("y = y, x < x");
                        for (int i = 0; i < actionAbility.range; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y) != null)
                            {
                                pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).SetIsFire(false);

                                if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit != unit)
                                    {
                                        unitList.Add(pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit);
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }

                        if (unitList.Count > 0)
                        {
                            for (int i = 0; i < actionAbility.range - 1; ++i)
                            {
                                if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y) != null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).isWalkable && !pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).isContainingUnit)
                                    {
                                        nodeToMove = pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y);
                                        break;
                                    }
                                }
                            }

                            if (nodeToMove != null)
                            {
                                foreach (GameObject unit in unitList)
                                {
                                    unitListNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(unit.transform.position.x), (int)Mathf.Round(unit.transform.position.z));

                                    for (int i = 1; i <= nodeToMove.x - unitListNode.x; ++i)
                                    {
                                        if (!pathfinding.GetNodeWithCoords(unitListNode.x + i, nodeToMove.y).isWalkable || pathfinding.GetNodeWithCoords(unitListNode.x + i, nodeToMove.y).isContainingUnit)
                                        {
                                            moveNode = pathfinding.GetNodeWithCoords(unitListNode.x + (i - 1), nodeToMove.y);
                                            break;
                                        }
                                        else
                                        {
                                            moveNode = pathfinding.GetNodeWithCoords(unitListNode.x + i, nodeToMove.y);
                                        }
                                    }

                                    if (moveNode != null)
                                    {
                                        Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                                        unitListNode.SetIsContainingUnit(false, null);
                                        unit.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                                    }
                                    else
                                    {
                                        Debug.Log(actionAbility.name + " = NO TARGET NODE FOUND TO MOVE UNIT");
                                    }
                                }
                            }

                            foreach (GameObject unit in unitList)
                            {
                                for (int i = 0; i < unit.transform.childCount; ++i)
                                {
                                    if (unit.transform.GetChild(3).GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                    {
                                        unit.transform.GetChild(3).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                        break;
                                    }
                                    else
                                    {
                                        unit.transform.GetChild(i).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                }
                break;

            case "Lance-Flamme":
                if (unitNode.x == targetNode.x)
                {
                    if (unitNode.y > targetNode.y)
                    {
                        Debug.Log("x = x, y > y");
                        for (int i = 0; i <= actionAbility.range - 1; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)) != null)
                            {
                                pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).SetIsFire(true);

                                if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit != unit)
                                    {
                                        for (int j = 0; j < pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit.transform.childCount; ++j)
                                        {
                                            if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                            {
                                                pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                                break;
                                            }
                                            else
                                            {
                                                pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit.GetComponent<UnitGridSystem>().implants[j].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                            }
                                        }
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                    else if (unitNode.y < targetNode.y)
                    {
                        Debug.Log("x = x, y < y");
                        for (int i = 0; i <= actionAbility.range - 1; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)) != null)
                            {
                                pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).SetIsFire(true);

                                if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit != unit)
                                    {
                                        for (int j = 0; j < pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit.transform.childCount; ++j)
                                        {
                                            if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                            {
                                                pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                                break;
                                            }
                                            else
                                            {
                                                pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit.GetComponent<UnitGridSystem>().implants[j].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                            }
                                        }
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (unitNode.y == targetNode.y)
                {
                    if (unitNode.x > targetNode.x)
                    {
                        Debug.Log("y = y, x > x");
                        for (int i = 0; i <= actionAbility.range - 1; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y) != null)
                            {
                                pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).SetIsFire(true);

                                if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit != unit)
                                    {
                                        for (int j = 0; j < pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit.transform.childCount; ++j)
                                        {
                                            if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                            {
                                                pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                                break;
                                            }
                                            else
                                            {
                                                pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit.GetComponent<UnitGridSystem>().implants[j].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                            }
                                        }
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                    else if (unitNode.x < targetNode.x)
                    {
                        Debug.Log("y = y, x < x");
                        for (int i = 0; i <= actionAbility.range - 1; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y) != null)
                            {
                                pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).SetIsFire(true);

                                if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit != unit)
                                    {
                                        for (int j = 0; j < pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit.transform.childCount; ++j)
                                        {
                                            if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                            {
                                                pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                                break;
                                            }
                                            else
                                            {
                                                pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit.GetComponent<UnitGridSystem>().implants[j].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                            }
                                        }
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                }
                break;

            case "Poing de Métal":
                if (targetNode != null)
                {
                    if (targetNode != null)
                    {
                        if (targetNode.isContainingUnit)
                        {
                            if (targetNode.unit != unit)
                            {
                                if (implantIndex > -1)
                                {
                                    targetNode.unit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                }
                            }
                        }
                        else if (targetNode.isNexus && nexus == null)
                        {
                            if (targetNode.nexusOrange != null && targetNode.nexusBlue == null)
                            {
                                targetNode.nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                            }
                            else if (targetNode.nexusOrange == null && targetNode.nexusBlue != null)
                            {
                                targetNode.nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                            }
                        }

                    }
                }
                break;

            case "Charge de Métal":
                if (unitNode.x == targetNode.x)
                {
                    if (unitNode.y > targetNode.y)
                    {
                        Debug.Log("x = x, y > y");
                        for (int i = 0; i < 2; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(targetNode.x, targetNode.y + i) != null)
                            {
                                if (pathfinding.GetNodeWithCoords(targetNode.x, targetNode.y + i).isWalkable && !pathfinding.GetNodeWithCoords(targetNode.x, targetNode.y + i).isContainingUnit)
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(targetNode.x, targetNode.y + i);
                                    break;
                                }
                            }
                        }

                        if (moveNode != null)
                        {
                            Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                            transform.parent.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                        }
                    }
                    else if (unitNode.y < targetNode.y)
                    {
                        Debug.Log("x = x, y < y");
                        for (int i = 0; i < 2; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(targetNode.x, targetNode.y - i) != null)
                            {
                                if (pathfinding.GetNodeWithCoords(targetNode.x, targetNode.y - i).isWalkable && !pathfinding.GetNodeWithCoords(targetNode.x, targetNode.y - i).isContainingUnit)
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(targetNode.x, targetNode.y - i);
                                    break;
                                }
                            }
                        }

                        if (moveNode != null)
                        {
                            Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                            transform.parent.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                        }
                    }
                }
                else if (unitNode.y == targetNode.y)
                {
                    if (unitNode.x > targetNode.x)
                    {
                        Debug.Log("y = y, x > x");
                        for (int i = 0; i < 2; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y) != null)
                            {
                                if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y).isWalkable && !pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y).isContainingUnit)
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y);
                                    break;
                                }
                            }
                        }

                        if (moveNode != null)
                        {
                            Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                            transform.parent.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                            Debug.Log(targetNode);
                        }
                    }
                    else if (unitNode.x < targetNode.x)
                    {
                        Debug.Log("y = y, x < x");
                        for (int i = 0; i < 2; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(targetNode.x - i, targetNode.y) != null)
                            {
                                if (pathfinding.GetNodeWithCoords(targetNode.x - i, targetNode.y).isWalkable && !pathfinding.GetNodeWithCoords(targetNode.x - i, targetNode.y).isContainingUnit)
                                {
                                    moveNode = pathfinding.GetNodeWithCoords(targetNode.x - i, targetNode.y);
                                    break;
                                }
                            }
                        }

                        if (moveNode != null)
                        {
                            Vector3 targetPosition = new Vector3(moveNode.x, 0, moveNode.y);
                            transform.parent.GetComponent<UnitGridSystem>().movePathfinding.SetVelocity(targetPosition, basicLinearMove, () => { });
                        }
                    }
                }

                if (targetNode != null)
                {
                    if (targetNode.isContainingUnit)
                    {
                        if (targetNode.unit != unit)
                        {
                            if (implantIndex > -1)
                            {
                                targetNode.unit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                            }
                        }
                    }
                    else if (targetNode.isNexus && nexus == null)
                    {
                        if (targetNode.nexusOrange != null && targetNode.nexusBlue == null)
                        {
                            targetNode.nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                        }
                        else if (targetNode.nexusOrange == null && targetNode.nexusBlue != null)
                        {
                            targetNode.nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                        }
                    }

                }
                break;

            case "Tir Focalisé":
                if (targetNode != null)
                {
                    if (targetNode.isContainingUnit)
                    {
                        if (targetNode.unit != unit)
                        {
                            if (implantIndex > -1)
                            {
                                targetNode.unit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                            }
                        }
                    }
                    else if (targetNode.isNexus && nexus == null)
                    {
                        if (targetNode.nexusOrange != null && targetNode.nexusBlue == null)
                        {
                            targetNode.nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                        }
                        else if (targetNode.nexusOrange == null && targetNode.nexusBlue != null)
                        {
                            targetNode.nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                        }
                    }

                }
                break;

            case "Tir Perçant":
                if (unitNode.x == targetNode.x)
                {
                    if (unitNode.y > targetNode.y)
                    {
                        Debug.Log("x = x, y > y");
                        for (int i = 0; i <= actionAbility.range - 1; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)) != null)
                            {
                                if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit != unit)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).unit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - (actionAbility.range - i)).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                    else if (unitNode.y < targetNode.y)
                    {
                        Debug.Log("x = x, y < y");
                        for (int i = 0; i <= actionAbility.range - 1; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)) != null)
                            {
                                if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit != unit)
                                    {
                                        Debug.Log(targetUnit.transform.position);
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).unit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + (actionAbility.range - i)).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (unitNode.y == targetNode.y)
                {
                    if (unitNode.x > targetNode.x)
                    {
                        Debug.Log("y = y, x > x");
                        for (int i = 0; i <= actionAbility.range - 1; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y) != null)
                            {
                                if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit != unit)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).unit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x - (actionAbility.range - i), unitNode.y).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                    else if (unitNode.x < targetNode.x)
                    {
                        Debug.Log("y = y, x < x");
                        for (int i = 0; i <= actionAbility.range - 1; ++i)
                        {
                            if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y) != null)
                            {
                                if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).isContainingUnit)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit != unit)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).unit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange != null && pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusOrange == null && pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(unitNode.x + (actionAbility.range - i), unitNode.y).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                }
                break;

            case "Trancheur Laser":
                if (targetNode != null)
                {
                    if (targetNode.isContainingUnit)
                    {
                        if (targetNode.unit != unit)
                        {
                            if (implantIndex > -1)
                            {
                                targetNode.unit.transform.GetChild(implantIndex).GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                            }
                        }
                    }
                    else if (targetNode.isNexus && nexus == null)
                    {
                        if (targetNode.nexusOrange != null && targetNode.nexusBlue == null)
                        {
                            targetNode.nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                        }
                        else if (targetNode.nexusOrange == null && targetNode.nexusBlue != null)
                        {
                            targetNode.nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                        }
                    }

                }
                break;

            case "Surchauffe":
                unitGridSystem.movePoints += actionAbility.damage;
                unitGridSystem.actionPoints += actionAbility.damage;
                unitGridSystem.remainingMovePoints += actionAbility.damage;
                unitGridSystem.remainingActionPoints += actionAbility.damage;
                transform.GetComponent<ImplantSystem>().InflictDamage(40);
                hasBeenUsed = true;
                unitGridSystem.isOverheat = true;
                break;

            case "Crachat Arachnéen":
                if (targetUnit != null)
                {
                    if (!targetUnit.GetComponent<UnitGridSystem>().isSpiderWeb)
                    {
                        targetUnit.GetComponent<UnitGridSystem>().isSpiderWeb = true;

                        if (targetUnit.GetComponent<UnitGridSystem>().hasSpiderWebToBeRemove)
                        {
                            targetUnit.GetComponent<UnitGridSystem>().hasSpiderWebToBeRemove = false;
                        }
                        else
                        {
                            targetUnit.GetComponent<UnitGridSystem>().spiderWeb = Instantiate(spiderWebPrefab, new Vector3(targetUnit.transform.position.x, 0, targetUnit.transform.position.z), Quaternion.identity);
                        }
                    }
                }
                break;

            case "Tir de Mortiers":
                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; ++j)
                    {
                        if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j) != null)
                        {
                            if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).x == targetNode.x || pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).y == targetNode.y)
                            {
                                if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).isContainingUnit)
                                {
                                    for (int k = 0; k < pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).unit.transform.childCount; ++k)
                                    {
                                        if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().lifePoints <= actionAbility.damage)
                                        {
                                            pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                            break;
                                        }
                                        else
                                        {
                                            pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).unit.GetComponent<UnitGridSystem>().implants[k].GetComponent<ImplantSystem>().InflictDamage(actionAbility.damage);
                                        }
                                    }
                                }
                                else if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).isNexus && nexus == null)
                                {
                                    if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).nexusOrange != null && pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).nexusBlue == null)
                                    {
                                        pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).nexusOrange.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                    else if (pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).nexusOrange == null && pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).nexusBlue != null)
                                    {
                                        pathfinding.GetNodeWithCoords(targetNode.x + i, targetNode.y + j).nexusBlue.GetComponent<NexusSystem>().InflictDamage(actionAbility.damage);
                                    }
                                }
                            }
                        }
                    }
                }

                unitGridSystem.remainingActionPoints = 0;
                break;
        }

        if (actionAbility.isPhysical || actionAbility.abilityName == "Pression Hydraulique")
        {
            if (targetUnit != null)
            {
                if (targetUnit.transform.GetChild(1).gameObject.GetComponent<ActionAbility>().actionAbility.abilityName == "Aura Électrique")
                {
                    for (int i = 0; i < unit.transform.childCount; ++i)
                    {
                        if (unitGridSystem.implants[3].GetComponent<ImplantSystem>().lifePoints <= targetUnit.transform.GetChild(1).gameObject.GetComponent<ActionAbility>().actionAbility.damage)
                        {
                            unitGridSystem.implants[3].GetComponent<ImplantSystem>().InflictDamage(targetUnit.transform.GetChild(1).gameObject.GetComponent<ActionAbility>().actionAbility.damage);
                            break;
                        }
                        else
                        {
                            unitGridSystem.implants[i].GetComponent<ImplantSystem>().InflictDamage(targetUnit.transform.GetChild(1).gameObject.GetComponent<ActionAbility>().actionAbility.damage);
                        }
                    }
                }
            }
            
            if (unitList != null)
            {
                for (int i = 0; i < unitList.Count; ++i)
                {
                    if (unitList[i] != targetUnit)
                    {
                        if (unitList[i].transform.GetChild(1).gameObject.GetComponent<ActionAbility>().actionAbility.abilityName == "Aura Électrique")
                        {
                            for (int j = 0; j < unit.transform.childCount; ++j)
                            {
                                if (unitGridSystem.implants[3].GetComponent<ImplantSystem>().lifePoints <= unitList[i].transform.GetChild(1).gameObject.GetComponent<ActionAbility>().actionAbility.damage)
                                {
                                    unitGridSystem.implants[3].GetComponent<ImplantSystem>().InflictDamage(unitList[i].transform.GetChild(1).gameObject.GetComponent<ActionAbility>().actionAbility.damage);
                                    break;
                                }
                                else
                                {
                                    unitGridSystem.implants[j].GetComponent<ImplantSystem>().InflictDamage(unitList[i].transform.GetChild(1).gameObject.GetComponent<ActionAbility>().actionAbility.damage);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    IEnumerator SplashScreen(/*Sounds sound, */float time)
    {
        //audioManager.Play(sound.name);
        Debug.Log("Play");
        yield return new WaitForSecondsRealtime(time);
    }
}
