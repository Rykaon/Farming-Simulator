using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IItems
    {
        GameObject Prefab {  get; }
        string ItemName { get; }
        int BuyPrice { get; }
        int SellPrice { get; }
    }

    [CreateAssetMenu]
    public class Items : ScriptableObject
    {
        public GameObject prefab;
        public string itemName;
        public int buyPrice;
        public int sellPrice;
    }
}
