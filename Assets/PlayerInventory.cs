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
    
    [SerializeField] private List<Items> items;
    public List<PlantItem> plantsList;
    public List<SeedItem> seedsList;
    public List<ObjectItem> objectsList;

    public Dictionary<Type, Dictionary<IItems, int>> inventory = new Dictionary<Type, Dictionary<IItems, int>>();

    public int nbArgent;

    /////////////////////////////////////////////////////////////////
    // Normalement la logique de l'inventaire est d�finitive       //
    // t'auras pas besoin de la modifier pour rajouter les         //
    // graines ou autres objets.                                   //
    //                                                             //
    // La seule chose que t'auras � faire, c'est cr�er les         //
    // instances des scriptableObjects et les rajouter             //
    // depuis l'inspecteur � la liste "items", le code             //
    // fera le reste.                                              //
    //                                                             //
    // Du cot� de Inky, par contre t'auras besoin de rajouter      //
    // les variables globales par contre, et bien modifier         //
    // la fonction UpdateVariables(), mais la logique est d�j�     //
    // �crite, t'auras juste � d�commenter les deux derni�res      //
    // boucles for et chnager le nom des variables pour qu'elles   //
    // correspondent aux nouvelles variables globales de Inky      //
    // que t'auras d�fini pour les graines et les objets.          //
    //                                                             //
    // Par contre ce que t'auras besoin d'utiliser pour update     //
    // l'inventaire en dehors de Inky (dans le cas o� le joueur    //
    // r�colte des graines en coupant les plantes par exemple),    //
    // c'est les fonctions qui se trouvent dans la classe          //
    // static Utilities. Pour �a, r�f�re toi aux commentaires      //
    // directement dans la classe Utilities si besoin.             //
    /////////////////////////////////////////////////////////////////

    private void Awake()
    {        
        plantsList = items.OfType<PlantItem>().ToList();
        seedsList = items.OfType<SeedItem>().ToList();
        objectsList = items.OfType<ObjectItem>().ToList();

        for (int i = 0; i < plantsList.Count; i++)
        {
            if (PC_Manager.playerData != null)
            {
                Utilities.AddToDictionary(inventory, plantsList[i], PC_Manager.playerData.nbPlants[i]);
            }
            else
            {
                Utilities.AddToDictionary(inventory, plantsList[i], nbStartPlants);
            }
        }
        
        for (int i = 0; i < seedsList.Count; i++)
        {
            if (PC_Manager.playerData != null)
            {

            }
            else
            {
                Utilities.AddToDictionary(inventory, seedsList[i], nbStartSeeds);
            }
        }

        for (int i = 0; i < objectsList.Count; i++)
        {
            if (PC_Manager.playerData != null)
            {

            }
            else
            {
                Utilities.AddToDictionary(inventory, objectsList[i], nbStartObjects);
            }
        }

        if (PC_Manager.playerData != null)
        {
            nbArgent = PC_Manager.playerData.nbArgent;
        }
        else
        {
            nbArgent = nbStartArgent;
        }
        
        PC_Manager.UpdateUIInventory();
    }

    public void SellBuy<T>(T item, bool sellOrBuy) where T : Items, IItems
    {
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
            PC_Manager.UpdateUIInventory();
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
            Debug.Log("AUCUN ITEM CORRESPONDANT A CE NOM N'A �T� TROUV�");
        }
    }

    public void SellBuyItem(string itemName, int plantPrice, bool sellOrBuy)
    {
        Type itemType = Utilities.GetTypeByName(inventory, itemName);

        if (itemType != null)
        {
            MethodInfo method = typeof(PlayerInventory).GetMethod("SellBuyGenericItem");
            MethodInfo generic = method.MakeGenericMethod(itemType);
            generic.Invoke(this, new object[] { itemName, sellOrBuy });
        }
        else
        {
            Debug.Log("AUCUN ITEM CORRESPONDANT A CE NOM N'A �T� TROUV�");
        }
    }
}
