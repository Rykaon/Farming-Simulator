using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu]
    public class ObjectItem : Items, IItems
    {
        public GameObject Prefab { get { return base.prefab; } }
        public string ItemName { get { return base.itemName; } }
        public int BuyPrice { get { return base.buyPrice; } }
        public int SellPrice { get { return base.sellPrice; } }
    }
}
