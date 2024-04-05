using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Map;

public class Pathfinding
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 1000;

    public static Pathfinding instance { get; private set; }

    private GridMap<PathNode> grid;
    private List<PathNode> openList;
    private List<PathNode> closedList;

    public Pathfinding(int width, int height)
    {
        instance = this;
        grid = new GridMap<PathNode>(width, height, 1f, Vector3.zero);
        for (int i = 0; i < grid.GetWidth(); ++i)
        {
            for (int j = 0; j < grid.GetHeight(); ++j)
            {
                grid.SetGridObject(i, j, new PathNode(grid, i, j));
            }
        }
    }

    public GridMap<PathNode> GetGrid()
    {
        return grid;
    }

    public PathNode GetNodeWithCoords(int x, int y)
    {
        if (x < 0 || x > grid.GetWidth() || y < 0 || y > grid.GetHeight())
        {
            return null;
        }
        return grid.GetGridObject(x, y);
    }

    public PathNode GetNodeWithVector3(Vector3 vector)
    {
        if (vector.x < 0 || vector.x > grid.GetWidth() || vector.z < 0 || vector.z > grid.GetHeight())
        {
            return null;
        }
        return grid.GetGridObject((int)vector.x, (int)vector.z);
    }

    public PathNode GetNodeWithPlayerWorldPos(Vector3 vector)
    {
        if ((Mathf.Round(vector.x) >= 0 && Mathf.Round(vector.x) <= grid.GetWidth() - 1) && (Mathf.Round(vector.z) >= 0 && Mathf.Round(vector.z) <= grid.GetHeight() - 1))
        {
            if (!grid.GetGridObject((int)Mathf.Round(vector.x), (int)Mathf.Round(vector.z)).isVirtual)
            {
                return grid.GetGridObject((int)Mathf.Round(vector.x), (int)Mathf.Round(vector.z));
            }
        }
        return null;
    }

    public TileManager GetTileWithPlayerWorldPos(Vector3 vector)
    {
        if ((Mathf.Round(vector.x) >= 0 && Mathf.Round(vector.x) <= grid.GetWidth() - 1) && (Mathf.Round(vector.z) >= 0 && Mathf.Round(vector.z) <= grid.GetHeight() - 1))
        {
            if (!grid.GetGridObject((int)Mathf.Round(vector.x), (int)Mathf.Round(vector.z)).isVirtual)
            {
                return grid.GetGridObject((int)Mathf.Round(vector.x), (int)Mathf.Round(vector.z)).tileManager;
            }
        }
        return null;
    }

    public bool IsVectorInGridRange(Vector3 position)
    {
        if (position.x >= 0 && position.x <= grid.GetWidth())
        {
            if (position.z >= 0 && position.z <= grid.GetHeight())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCoordsInGridRange(int x, int y)
    {
        if (x >= 0 && x <= grid.GetWidth())
        {
            if (y >= 0 && y <= grid.GetHeight())
            {
                return true;
            }
        }

        return false;
    }

    public List<Vector3> FindPathMove(Vector3 startPosition, Vector3 endPosition)
    {
        grid.GetXY(startPosition, out int startX, out int startY);
        grid.GetXY(endPosition, out int endX, out int endY);

        List<PathNode> path = new List<PathNode>();

        if (path != null)
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path)
            {
                vectorPath.Add(new Vector3(pathNode.x, 0, pathNode.y) * grid.GetCellSize());
            }
            return vectorPath;
        }

        return null;
    }

    public List<Vector3> FindPathAction(Vector3 startPosition, Vector3 endPosition)
    {
        grid.GetXY(startPosition, out int startX, out int startY);
        grid.GetXY(endPosition, out int endX, out int endY);

        List<PathNode> path = new List<PathNode>();
        /*switch (ability.type)
        {
            case ActionData.Type.Area:
                path = FindAreaPathAction(startX, startY, endX, endY);
                break;

            case ActionData.Type.Linear:
                path = FindLinearPathAction(startX, startY, endX, endY);
                break;

            case ActionData.Type.LinearPath:
                // OUI, C'EST BIEN LINEAR_PATH_MOVE ET PAS LINEAR_PATH_ACTION CAR C'EST LE SEUL CAS OU ON VEUT UN CHEMIN ET PAS UNE RANGE
                path = FindLinearPathMove(startX, startY, endX, endY);
                break;
        }*/

        if (path != null)
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (PathNode pathNode in path)
            {
                vectorPath.Add(new Vector3(pathNode.x, 0, pathNode.y) * grid.GetCellSize());
            }
            return vectorPath;
        }

        return null;
    }

    public List<PathNode> FindAreaPathMove(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);
        openList = new List<PathNode>() { startNode };
        closedList = new List<PathNode>();

        if (!endNode.isWalkable || endNode == startNode)
        {
            return null;
        }

        for (int i = 0; i < grid.GetWidth(); ++i)
        {
            for (int j = 0; j < grid.GetHeight(); ++j)
            {
                PathNode pathNode = grid.GetGridObject(i, j);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                return CalculatePath(startNode, endNode);
            }

            foreach (PathNode neighbourNode in GetNeighbourListArea(currentNode))
            {
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                if (!neighbourNode.isWalkable || neighbourNode.isContainingUnit)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
        }

        return null;
    }

    public List<PathNode> FindLinearPathMove(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);
        PathNode firstNode = startNode;
        if (startNode.x == endNode.x && startNode.y != endNode.y)
        {
            if (endNode.y > startNode.y)
            {
                firstNode = grid.GetGridObject(startNode.x, startNode.y + 1);
            }
            else if (endNode.y < startNode.y)
            {
                firstNode = grid.GetGridObject(startNode.x, startNode.y - 1);
            }
        }
        if (startNode.x != endNode.x && startNode.y == endNode.y)
        {
            if (endNode.x > startNode.x)
            {
                firstNode = grid.GetGridObject(startNode.x + 1, startNode.y);
            }
            else if (endNode.x < startNode.x)
            {
                firstNode = grid.GetGridObject(startNode.x - 1, startNode.y);
            }
        }
        firstNode.cameFromNode = startNode;
        openList = new List<PathNode>() { firstNode }; 
        closedList = new List<PathNode>();

        if (!endNode.isWalkable || endNode.isContainingUnit || endNode == startNode || !firstNode.isWalkable || firstNode.isContainingUnit)
        {
            return null;
        }

        for (int i = 0; i < grid.GetWidth(); ++i)
        {
            for (int j = 0; j < grid.GetHeight(); ++j)
            {
                PathNode pathNode = grid.GetGridObject(i, j);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {    
            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                return CalculatePath(startNode, endNode);
            }

            foreach (PathNode neighbourNode in GetNeighbourListLinear(startNode, endNode, currentNode))
            {
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                if (!neighbourNode.isWalkable || neighbourNode.isContainingUnit)
                {
                    closedList.Add(neighbourNode);
                    return CalculatePath(startNode, currentNode);
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
        }

        return null;
    }

    public List<PathNode> FindAreaPathAction(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);
        openList = new List<PathNode>() { startNode };
        closedList = new List<PathNode>();

        if (/*(!endNode.isWalkable && !endNode.isNexus) || */endNode == startNode)
        {
            return null;
        }

        for (int i = 0; i < grid.GetWidth(); ++i)
        {
            for (int j = 0; j < grid.GetHeight(); ++j)
            {
                PathNode pathNode = grid.GetGridObject(i, j);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                return CalculatePath(startNode, endNode);
            }

            foreach (PathNode neighbourNode in GetNeighbourListArea(currentNode))
            {
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                /*if (!neighbourNode.isWalkable && !neighbourNode.isNexus)
                {
                    closedList.Add(neighbourNode);
                    continue;
                }*/

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
        }

        return null;
    }

    public List<PathNode> FindLinearPathAction(int startX, int startY, int endX, int endY)
    {
        PathNode startNode = grid.GetGridObject(startX, startY);
        PathNode endNode = grid.GetGridObject(endX, endY);
        PathNode firstNode = startNode;
        if (startNode.x == endNode.x && startNode.y != endNode.y)
        {
            if (endNode.y > startNode.y)
            {
                firstNode = grid.GetGridObject(startNode.x, startNode.y + 1);
            }
            else if (endNode.y < startNode.y)
            {
                firstNode = grid.GetGridObject(startNode.x, startNode.y - 1);
            }
        }
        if (startNode.x != endNode.x && startNode.y == endNode.y)
        {
            if (endNode.x > startNode.x)
            {
                firstNode = grid.GetGridObject(startNode.x + 1, startNode.y);
            }
            else if (endNode.x < startNode.x)
            {
                firstNode = grid.GetGridObject(startNode.x - 1, startNode.y);
            }
        }
        firstNode.cameFromNode = startNode;
        openList = new List<PathNode>() { firstNode };
        closedList = new List<PathNode>();

        for (int i = 0; i < grid.GetWidth(); ++i)
        {
            for (int j = 0; j < grid.GetHeight(); ++j)
            {
                PathNode pathNode = grid.GetGridObject(i, j);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostNode(openList);

            if (currentNode == endNode)
            {
                return CalculatePath(startNode, endNode);
            }

            foreach (PathNode neighbourNode in GetNeighbourListLinear(startNode, endNode, currentNode))
            {
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                if (!neighbourNode.isWalkable || neighbourNode.isContainingUnit)
                {
                    closedList.Add(neighbourNode);
                    return CalculatePath(startNode, currentNode);
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);
        }

        return null;
    }

    private int CalculateDistanceCost(PathNode a, PathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; ++i)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }

    private List<PathNode> GetNeighbourListArea(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        if (currentNode.x - 1 >= 0)
        {
            // LEFT
            neighbourList.Add(GetNodeWithCoords(currentNode.x - 1, currentNode.y));
        }

        if (currentNode.x + 1 < grid.GetWidth())
        {
            // RIGHT
            neighbourList.Add(GetNodeWithCoords(currentNode.x + 1, currentNode.y));
        }

        // DOWN
        if (currentNode.y - 1 >= 0)
        {
            neighbourList.Add(GetNodeWithCoords(currentNode.x, currentNode.y - 1));
        }

        // UP
        if (currentNode.y + 1 < grid.GetHeight())
        {
            neighbourList.Add(GetNodeWithCoords(currentNode.x, currentNode.y + 1));
        }

        return neighbourList;
    }

    private List<PathNode> GetNeighbourListLinear(PathNode startNode, PathNode endNode, PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        if (startNode.y == currentNode.y && startNode.x != currentNode.x)
        {
            if (currentNode.x - 1 >= 0 && currentNode.x < startNode.x)
            {
                // LEFT
                neighbourList.Add(GetNodeWithCoords(currentNode.x - 1, currentNode.y));
            }
            
            if (currentNode.x + 1 < grid.GetWidth() && currentNode.x > startNode.x)
            {
                // RIGHT
                neighbourList.Add(GetNodeWithCoords(currentNode.x + 1, currentNode.y));
            }
        }
        
        if (startNode.x == currentNode.x && startNode.y != currentNode.y)
        {
            if (currentNode.y - 1 >= 0 && currentNode.y < startNode.y)
            {
                // DOWN
                neighbourList.Add(GetNodeWithCoords(currentNode.x, currentNode.y - 1));
            }
            
            if (currentNode.y + 1 < grid.GetHeight() && currentNode.y > startNode.y)
            {
                // UP
                neighbourList.Add(GetNodeWithCoords(currentNode.x, currentNode.y + 1));
            }
        }

        return neighbourList;
    }

    private List<PathNode> CalculatePath(PathNode startNode, PathNode endNode)
    {
        List<PathNode> path = new List<PathNode>();
        path.Add(endNode);
        PathNode currentNode = endNode;

        while (currentNode.cameFromNode != null)
        {
            if (currentNode == startNode)
            {
                currentNode = currentNode.cameFromNode;
                continue;
            }
            else
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
        }

        path.Reverse();

        return path;
    }
}
