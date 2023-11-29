using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem instance;

    [SerializeField] PlayerController playerController;
    [SerializeField] ToolManager toolManager;

    public GridLayout gridLayout;
    [SerializeField] Grid grid;
    public Tilemap groundTilemap;
    public Tilemap objectTilemap;

    public TileBase3D fence;
    public TileBase3D fenceCorner;
    public TileBase3D fenceDoor;

    public BuildObject objectToPlace;
    public TileBase3D tileToPlace;

    public bool isPlacing;

    private void Awake()
    {
        instance = this;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && objectToPlace == null)
        {
            InitializeWithObject(fence, objectTilemap);
        }
        else if (Input.GetKeyDown(KeyCode.W) && objectToPlace == null)
        {
            InitializeWithObject(fenceCorner, objectTilemap);
        }
        else if (Input.GetKeyDown(KeyCode.E) && objectToPlace == null)
        {
            InitializeWithObject(fenceDoor, objectTilemap);
        }

        if (Input.GetKeyDown(KeyCode.Space) && objectToPlace != null)
        {
            if (CanBePlaced(objectToPlace, objectTilemap))
            {
                objectToPlace.Place();
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                TakeArea(start, objectToPlace.size, objectTilemap);
                objectToPlace = null;
                tileToPlace = null;
                isPlacing = false;
            }
            else
            {
                Destroy(objectToPlace.gameObject);
                objectToPlace = null;
                tileToPlace = null;
                isPlacing = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && objectToPlace != null)
        {
            Destroy(objectToPlace.gameObject);
            objectToPlace = null;
            tileToPlace = null;
            isPlacing = false;
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            objectToPlace.Rotate(1);
        }
    }

    public Vector3 GetPlayerPosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerController.currentPos, -Vector3.up, out hit, 1f))
        {
            if (hit.collider.gameObject.layer == 6)
            {
                return hit.point;
            }
            return Vector3.zero;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public GameObject GetGroundTile()
    {
        if (fence.GetTile(playerController.forwardPos, groundTilemap) != null)
        {
            return fence.GetTile(playerController.forwardPos, groundTilemap);
        }

        return null;
    }

    public GameObject GetObjectTile()
    {
        if (fence.GetTile(playerController.forwardPos, objectTilemap) != null)
        {
            return fence.GetTile(playerController.forwardPos, objectTilemap);
        }

        return null;
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        return position;
    }

    private GameObject[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        GameObject[] array = new GameObject[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (Vector3Int v in area.allPositionsWithin)
        {
            array[counter] = tileToPlace.GetTile(v, tilemap);
            counter++;
        }

        return array;
    }

    public void InitializeWithObject(TileBase3D tile, Tilemap tilemap)
    {
        isPlacing = true;

        GameObject obj;

        if (playerController.currentGroundTile != null)
        {
            obj = tile.Instantiate(playerController.currentGroundTile.transform.position, Quaternion.identity, tilemap);
        }
        else
        {
            obj = tile.Instantiate(Vector3.zero, Quaternion.identity, tilemap);
        }

        objectToPlace = obj.GetComponent<BuildObject>();
        tileToPlace = tile;
        obj.AddComponent<BuildObjectDrag>();

    }

    public bool CanBePlaced(BuildObject obj, Tilemap tilemap)
    {
        BoundsInt area = new BoundsInt();
        area.position = gridLayout.WorldToCell(obj.GetStartPosition());
        area.size = obj.size;

        /*GameObject[] tileArray = GetTilesBlock(area, tilemap);

        foreach (GameObject v in tileArray)
        {

            if (v != null)
            {
                return false;
            }
        }*/

        if (GetObjectTile() != null)
        {
            if (GetObjectTile().GetComponent<BuildObject>().isPlaced)
            {
                return false;
            }
        }

        return true;
    }

    public void TakeArea(Vector3Int start, Vector3Int size, Tilemap tilemap)
    {
        // Doit réécrire la méthode BoxFill pour accepeter des arguments (int startX, int startY, int endX, int endY)
        // la méthode doit être sous la forme d'une double boucle for

        //tileToPlace.BoxFill(start, tileToPlace, start.x, start.y, start.x + size.x, start.y + size.y);
    }
}
