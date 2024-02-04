using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class PlantItem : ScriptableObject
{
    public GameObject prefab;
    public string plantName;
    public int numberOfSeedRecolted;
    public int buyPrice;
    public int sellPrice;
}
