using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int runIndex;
    public int nbArgent;
    public List<int> nbPlants;

    public PlayerData()
    {
        runIndex = PlayerManager.instance.mapGenerator.runIndex;
        nbArgent = PlayerManager.instance.inventory.nbArgent;
        nbPlants = new List<int>();
        for (int i = 0; i < PlayerManager.instance.inventory.plantsList.Count; i++)
        {
            nbPlants.Add(Utilities.GetNumberOfItemByPrefab(PlayerManager.instance.inventory.inventory, PlayerManager.instance.inventory.plantsList[i].Prefab));
        }
    }
}
