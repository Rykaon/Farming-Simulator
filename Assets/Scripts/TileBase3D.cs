using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileBase3D : TileBase
{
    public GameObject prefab;
    private Tilemap tilemap;

    public GameObject lastInstance;
    public List<GameObject> instantiatedList;

    void Awake()
    {
        instantiatedList = new List<GameObject>();
    }

    public GameObject Instantiate(Vector3 position, Quaternion rotation, Tilemap tilemap)
    {
        lastInstance = Instantiate(prefab, position, rotation, tilemap.transform);
        this.tilemap = tilemap;
        instantiatedList.Add(lastInstance);

        return lastInstance;
    }

    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = null;
        tileData.gameObject = prefab;
        tileData.colliderType = GetColliderType(location, tilemap);
        tileData.flags = TileFlags.LockTransform;
        tileData.transform = Matrix4x4.identity;
    }

    /*public void BoxFill(Vector3Int position, TileBase tile, int count, Tilemap tilemap)
    {
        if (prefab != null)
        {
            for (int i = 0; i < count; i++)
            {
                lastInstance = Instantiate(prefab, position + new Vector3(i, 0, 0), Quaternion.identity, tilemap.transform);
                instantiatedList.Add(lastInstance);
            }
        }
    }*/

    public Transform GetTransform()
    {
        return tilemap.transform;
    }

    public Vector3 GetCellCenterWorld(Vector3Int position)
    {
        return tilemap.GetCellCenterWorld(position);
    }

    public GameObject GetTile(Vector3Int location, Tilemap tilemap)
    {
        location = new Vector3Int(location.x, location.z, 0);
        
        for (int i = 0; i < tilemap.transform.childCount; i++)
        {
            GameObject tileBase3D = tilemap.transform.GetChild(i).gameObject;

            if (tilemap.WorldToCell(tileBase3D.transform.position) == location)
            {
                return tileBase3D;
            }
        }

        return null;
    }

    public Tile.ColliderType GetColliderType(Vector3Int location, ITilemap tilemap)
    {
        // Implémentez la logique pour déterminer le type de collider
        return Tile.ColliderType.None; // Exemple, ajustez selon vos besoins
    }
}
