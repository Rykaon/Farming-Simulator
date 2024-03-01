using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System.Linq;

public static class Utilities
{
    public static T GetItemByName<T>(Dictionary<Type, Dictionary<IItems, int>> inventory, string name) where T : Items, IItems
    {
        var possibleTypes = typeof(T).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(T)) && !type.IsAbstract).ToList();

        foreach (var itemType in possibleTypes)
        {
            if (inventory.ContainsKey(itemType))
            {
                var itemDictionary = inventory[itemType];
                var item = itemDictionary.Keys.OfType<T>().FirstOrDefault(i => i.ItemName == name);

                if (item != null)
                {
                    return item;
                }
            }
        }

        return null;
    }

    public static Type GetTypeByName(Dictionary<Type, Dictionary<IItems, int>> inventory, string itemName)
    {
        foreach (var type in inventory)
        {
            var itemDictionary = type.Value;
            var itemType = type.Key;

            var item = itemDictionary.Keys.OfType<IItems>().FirstOrDefault(i => i.ItemName == itemName);

            if (item != null)
            {
                return itemType;
            }
        }

        return null;
    }

    public static int GetNumberOfItemByPrefab(Dictionary<Type, Dictionary<IItems, int>> inventory, GameObject prefab)
    {
        foreach (var type in inventory)
        {
            var itemDictionary = type.Value;
            var itemType = type.Key;

            var item = itemDictionary.Keys.OfType<IItems>().FirstOrDefault(i => i.Prefab == prefab);

            if (item != null)
            {
                return inventory[itemType][item];
            }
        }

        return -1;
    }

    public static void AddItemByPrefab(Dictionary<Type, Dictionary<IItems, int>> inventory, GameObject prefab)
    {
        foreach (var type in inventory)
        {
            var itemDictionary = type.Value;
            var itemType = type.Key;

            var item = itemDictionary.Keys.OfType<IItems>().FirstOrDefault(i => i.Prefab == prefab);

            if (item != null)
            {
                inventory[itemType][item] = inventory[itemType][item] + 1;
            }
            else
            {
                Debug.Log("AUCUN ITEM CORRESPONDANT AU PREFAB TROUVÉ DANS L'INVENTAIRE");
            }
        }
    }

    public static void RemoveItemByPrefab(Dictionary<Type, Dictionary<IItems, int>> inventory, GameObject prefab)
    {
        foreach (var type in inventory)
        {
            var itemDictionary = type.Value;
            var itemType = type.Key;

            var item = itemDictionary.Keys.OfType<IItems>().FirstOrDefault(i => i.Prefab == prefab);

            if (item != null)
            {
                if (inventory[itemType][item] > 0)
                {
                    inventory[itemType][item] = inventory[itemType][item] + 1;
                }
                else
                {
                    Debug.Log("L'INVENTAIRE NE CONTIENT DEJA PLUS D'ITEM CORRESPONDANT A CE PREFAB");
                }
            }
            else
            {
                Debug.Log("AUCUN ITEM CORRESPONDANT AU PREFAB TROUVÉ DANS L'INVENTAIRE");
            }
        }
    }

    public static void AddToDictionary<T>(Dictionary<Type, Dictionary<IItems, int>> inventory, T item, int value) where T : IItems
    {
        Type itemType = typeof(T);

        if (!inventory.ContainsKey(itemType))
        {
            inventory[itemType] = new Dictionary<IItems, int>();
        }

        inventory[itemType][item] = value;
    }
}
