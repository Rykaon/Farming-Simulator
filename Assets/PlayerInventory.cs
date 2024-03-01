using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Assets.Scripts;
using Unity.VisualScripting;
using System.Reflection;

public class PlayerInventory : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerController_Farm playerControllerFarm;
    [SerializeField] private PlayerManager PC_Manager;

    [SerializeField] private int nbStartArgent;
    [SerializeField] private int nbStartPlants;
    [SerializeField] private int nbStartSeeds;
    [SerializeField] private int nbStartObjects;

    public bool isShopOpen = false;

    [SerializeField] private List<Item> items;
    private List<PlantItem> plantsList;
    private List<SeedItem> seedsList;
    private List<ObjectItem> objectsList;

    public Dictionary<Type, Dictionary<IItems, int>> inventory = new Dictionary<Type, Dictionary<IItems, int>>();

    public int nbArgent;


    private void Awake()
    {        
        plantsList = items.OfType<PlantItem>().ToList();
        seedsList = items.OfType<SeedItem>().ToList();
        objectsList = items.OfType<ObjectItem>().ToList();

        for (int i = 0; i < plantsList.Count; i++)
        {
            Utilities.AddToDictionary(inventory, plantsList[i], nbStartPlants);
        }

        for (int i = 0; i < seedsList.Count; i++)
        {
            Utilities.AddToDictionary(inventory, seedsList[i], nbStartSeeds);
        }

        for (int i = 0; i < objectsList.Count; i++)
        {
            Utilities.AddToDictionary(inventory, objectsList[i], nbStartObjects);
        }

        nbArgent = nbStartArgent;
    }

    public void SellBuy<T>(T item, bool sellOrBuy) where T : Items, IItems
    {
        nbArgent = nbArgent - 15;
        Type itemType = typeof(T);

        if (inventory.ContainsKey(itemType))
        {
            if (sellOrBuy)
            {
                if (inventory[itemType][item] == 0)
                {
                    Debug.Log("LE JOUEUR N'A AUCUN ITEM DE CE TYPE A VENDRE");
                }
                else
                {
                    inventory[itemType][item] = inventory[itemType][item] - 1;
                    nbArgent += item.SellPrice;
                }
            }
            else
            {
                if (item.BuyPrice < nbArgent)
                {
                    inventory[itemType][item] = inventory[itemType][item] + 1;
                    nbArgent -= item.BuyPrice;
                }
                else
                {
                    Debug.Log("LE JOUEUR N'A PAS ASSEZ D'ARGENT");
                }
            }
           
        }
        else
        {
            Debug.LogWarning("LE TYPE D'ITEM N'EST PAS RECONNU");
            return;
        }
    }

    public void SellBuyGenericItem<T>(string itemName, bool sellOrBuy) where T : Items, IItems
    {
        T item = Utilities.GetItemByName<T>(inventory, itemName);

        if (item != null)
        {
            SellBuy(item, sellOrBuy);
        }
        else
        {
            Debug.Log("AUCUN ITEM CORRESPONDANT A CE NOM N'A ÉTÉ TROUVÉ");
        }
    }

    public void SellBuyItem(string itemName, int plantPrice, bool sellOrBuy)
    {
        Type itemType = Utilities.GetTypeByName(inventory, itemName);

        if (itemType != null)
        {
            /*dynamic item = Utilities.GetItemByName(inventory, itemName);
            SellBuy(item, sellOrBuy);*/

            MethodInfo method = typeof(PlayerInventory).GetMethod("SellBuyGenericItem");
            MethodInfo generic = method.MakeGenericMethod(itemType);
            generic.Invoke(this, new object[] { itemName, sellOrBuy });
        }
        else
        {
            Debug.Log("AUCUN ITEM CORRESPONDANT A CE NOM N'A ÉTÉ TROUVÉ");
        }
    }
}
