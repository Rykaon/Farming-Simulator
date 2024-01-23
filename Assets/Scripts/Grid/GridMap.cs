using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap<TGridObject>
{
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    int gridWidth;
    int gridHeight;
    float cellSize;
    Vector3 gridOrigin;
    public TGridObject[,] gridObject;
    public Quaternion[,] gridRotation;

    public Sprite[,] previousSprite;
    public Color[,] previousColor;

    public GridMap<TGridObject> instance;

    public GridMap(int width, int height, float size, Vector3 origin)
    {
        instance = this;
        this.gridWidth = width;
        this.gridHeight = height;
        this.cellSize = size;
        this.gridOrigin = origin;

        this.gridObject = new TGridObject[gridWidth, gridHeight];
        this.gridRotation = new Quaternion[gridWidth, gridHeight];

        this.previousSprite = new Sprite[gridWidth, gridHeight];
        this.previousColor = new Color[gridWidth, gridHeight];
    }

    public GridMap(int width, int height, float size, Vector3 origin, Func<GridMap<TGridObject>, int, int, TGridObject> create)
    {
        instance = this;
        this.gridWidth = width;
        this.gridHeight = height;
        this.cellSize = size;
        this.gridOrigin = origin;

        this.gridObject = new TGridObject[gridWidth, gridHeight];
        this.gridRotation = new Quaternion[gridWidth, gridHeight];

        this.previousSprite = new Sprite[gridWidth, gridHeight];
        this.previousColor = new Color[gridWidth, gridHeight];
    }

    public int GetWidth()
    {
        return gridWidth;
    }

    public int GetHeight()
    {
        return gridHeight;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + gridOrigin;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - gridOrigin).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - gridOrigin).z / cellSize);
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight)
        {
            gridObject[x, y] = value;
        }
    }

    public void SetGridRotation(int x, int y, Quaternion rotation)
    {
        if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight)
        {
            gridRotation[x, y] = rotation;
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < gridWidth && y < gridHeight)
        {
            return gridObject[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }
}
