using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildObjectDrag : MonoBehaviour
{
    private PlayerManager playerManager;

    private void Awake()
    {
        playerManager = PlayerManager.instance;
    }

    private void Update()
    {
        if (PlayerController_Farm.instance.GetCurrentNode() != null)
        {
            transform.position = playerManager.PC_farm.SnapCoordinateToGrid(PlayerController_Farm.instance.GetCurrentNode().tile.transform.position);
        }
        else
        {
            transform.position = playerManager.PC_farm.SnapCoordinateToGrid(playerManager.transform.position);
        }
    }
}
