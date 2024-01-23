using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildObjectDrag : MonoBehaviour
{
    private PlayerController_Farm playerController;
    private BuildingSystem buildingSystem;

    private void Awake()
    {
        playerController = PlayerController_Farm.instance;
        buildingSystem = BuildingSystem.instance;
    }

    private void Update()
    {
        if (PlayerController_Farm.instance.currentTile != null)
        {
            transform.position = buildingSystem.SnapCoordinateToGrid(playerController.currentTile.transform.position);
        }
        else
        {
            transform.position = buildingSystem.SnapCoordinateToGrid(Vector3.zero);
        }
        
    }
}
