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
        if (playerManager.PC_farm.GetCurrentNode() != null)
        {
            if (!playerManager.PC_farm.GetCurrentNode().isVirtual)
            {
                transform.position = playerManager.PC_farm.SnapCoordinateToGrid(playerManager.PC_farm.GetCurrentNode().tile.transform.position);
            }
            else
            {
                transform.position = playerManager.PC_farm.SnapCoordinateToGrid(playerManager.transform.position);
            }
        }
        else
        {
            transform.position = playerManager.PC_farm.SnapCoordinateToGrid(playerManager.transform.position);
        }
    }
}
