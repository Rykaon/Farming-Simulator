using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class MoveAbility : MonoBehaviour
{
    GameObject unit;
    UnitGridSystem unitGridSystem;
    PathfindingMe pathfinding;
    public MoveData moveAbility;
    private int index;

    void Start()
    {
        pathfinding = PathfindingMe.instance;
        if (transform.parent != null)
        {
            InitAbility();
        }
    }

    public void InitAbility()
    {
        unit = transform.parent.gameObject;
        unitGridSystem = unit.GetComponent<UnitGridSystem>();
        unitGridSystem.movePoints += moveAbility.movePoints;

        string firstTagChar = "";
        firstTagChar = firstTagChar + transform.name[0];

        if (firstTagChar == "L")
        {
            transform.GetComponent<SpriteRenderer>().sprite = moveAbility.spriteLeft;
        }
        else
        {
            transform.GetComponent<SpriteRenderer>().sprite = moveAbility.spriteRight;
        }

        index = GetIndexOfAbilityInList(unitGridSystem.moveDataList, moveAbility);
    }

    public void UpdateSprite(MoveData move)
    {
        string firstTagChar = "";
        firstTagChar = firstTagChar + transform.name[0];

        if (firstTagChar == "L")
        {
            transform.GetComponent<SpriteRenderer>().sprite = move.spriteLeft;
        }
        else
        {
            transform.GetComponent<SpriteRenderer>().sprite = move.spriteRight;
        }
    }

    public void DisableAbility()
    {
        unit = transform.parent.gameObject;
        unitGridSystem = unit.GetComponent<UnitGridSystem>();
        unitGridSystem.movePoints -= moveAbility.movePoints;

        if (GetIndexOfAbilityInList(unitGridSystem.moveDataList, moveAbility) >= 0)
        {
            unitGridSystem.moveDataList.RemoveAt(index);
            unitGridSystem.moveAbilityList.RemoveAt(index);
        }

        moveAbility = null;
        transform.GetComponent<SpriteRenderer>().sprite = null;
    }

    public void ChangeAbility(MoveData newMoveData)
    {
        if (moveAbility != null)
        {
            //index = GetIndexOfAbilityInList(unitGridSystem.moveAbilities, moveAbility);
            DisableAbility();
        }
        else
        {
            index = -1;
        }

        moveAbility = newMoveData;
        if (index != -1)
        {
            unitGridSystem.moveDataList.Insert(index, moveAbility);
            unitGridSystem.moveAbilityList.Insert(index, this);
        }
        else
        {
            unitGridSystem.moveDataList.Add(moveAbility);
            unitGridSystem.moveAbilityList.Add(this);
        }
        InitAbility();
    }

    public void CustomChangeAbility(MoveData newMoveData)
    {
        if (moveAbility == null)
        {
            index = -1;
            DisableAbility();
        }

        moveAbility = newMoveData;
        if (index != -1)
        {
            unitGridSystem.moveDataList.Insert(index, moveAbility);
            unitGridSystem.moveAbilityList.Insert(index, this);
        }
        else
        {
            unitGridSystem.moveDataList.Add(moveAbility);
            unitGridSystem.moveAbilityList.Add(this);
        }

        InitAbility();
    }

    private int GetIndexOfAbilityInList(List<MoveData> abilityList, MoveData ability)
    {
        int index = -1;

        for (int i = 0; i < abilityList.Count; ++i)
        {
            if (ability == abilityList[i])
            {
                index = i;
            }
        }

        this.index = index;

        return this.index;
    }
}
