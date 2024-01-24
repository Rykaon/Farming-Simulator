using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private GridMap<PathNode> grid;
    public int x, y;

    public int gCost, hCost, fCost;
    public PathNode cameFromNode;

    public bool isVirtual;
    public bool isSeeded;
    public bool isWalkable;
    public bool isValidMovePosition;
    public bool isContainingUnit;

    public GameObject unit;
    public GameObject plant;
    public GameObject tile;
    public TileManager tileManager;
    
    public PathNode(GridMap<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

        isVirtual = false;
        isSeeded = false;
        isWalkable = true;
        isValidMovePosition = false;
        isContainingUnit = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void SetIsSeeded(bool isSeeded, GameObject plant)
    {
        this.isSeeded = isSeeded;
        this.plant = plant;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
    }

    public void SetIsValidMovePosition(bool isValidMovePosition)
    {
        this.isValidMovePosition = isValidMovePosition;
    }

    public void SetIsContainingUnit(bool isContainingUnit, GameObject unit)
    {
        this.isContainingUnit = isContainingUnit;
        this.isContainingUnit = unit;
    }

    public void SetTile(GameObject tile)
    {
        this.tile = tile;
    }

    public void SetTileManager(TileManager tileManager)
    {
        this.tileManager = tileManager;
    }

    public override string ToString()
    {
        return x + "," + y;
    }

    /*private int GetIndexOfAbilityInList(List<GameObject> GameObjectList, GameObject gameObject)
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
    }*/
}
