using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNodeMe
{
    private GridMap<PathNodeMe> grid;
    public int x, y;

    public int gCost, hCost, fCost;

    public bool isWalkable;
    public bool isValidMovePosition;
    public bool isContainingUnit;
    public GameObject unit;
    public bool isContainingImplant;
    public GameObject implant;
    public bool isNexus;
    public GameObject nexusOrange;
    public GameObject nexusBlue;
    public bool isOil;
    public bool isFire;
    public GameObject oil;
    public PathNodeMe cameFromNode;

    public PathNodeMe(GridMap<PathNodeMe> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

        isContainingUnit = false;
        isContainingImplant = false;
        implant = null;
        isOil = false;
        isFire = false;
        oil = null;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void SetIsValidMovePosition(bool isValidMovePosition)
    {
        this.isValidMovePosition = isValidMovePosition;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void SetIsContainingUnit(bool isContainingUnit, GameObject unit)
    {
        this.isContainingUnit = isContainingUnit;
        this.unit = unit;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void SetIsContainingImplant(bool isContainingImplant, GameObject implant)
    {
        if (isContainingImplant)
        {
            if (this.implant == null)
            {
                this.isContainingImplant = isContainingImplant;
                this.implant = implant;
            }
            else
            {
                Debug.Log("NODE ARLEADY CONTAINING AN IMPLANT");
            }
        }
        else
        {
            this.isContainingImplant = isContainingImplant;
            this.implant = implant;
        }
    }

    public void SetIsNexus(bool isNexus, UnitGridSystem.Team team, GameObject nexus)
    {
        if (!this.isNexus)
        {
            this.isNexus = isNexus;

            switch (team)
            {
                case UnitGridSystem.Team.Orange:
                    nexusOrange = nexus;
                    break;

                case UnitGridSystem.Team.Blue:
                    nexusBlue = nexus;
                    break;
            }
        }
    }

    public void SetIsOil(GameObject oil)
    {
        if (isWalkable && !isOil)
        {
            isOil = true;
            this.oil = oil;
        }
    }

    public void SetIsFire(bool isFire)
    {
        if (isWalkable && isOil)
        {
            this.isFire = isFire;
            oil.transform.GetChild(0).gameObject.SetActive(isFire);
        }
    }

    public override string ToString()
    {
        return x + "," + y;
    }

    private int GetIndexOfAbilityInList(List<GameObject> GameObjectList, GameObject gameObject)
    {
        int index = -1;

        for (int i = 0; i < GameObjectList.Count; ++i)
        {
            if (gameObject == GameObjectList[i])
            {
                index = i;
            }
        }

        return index;
    }
}
