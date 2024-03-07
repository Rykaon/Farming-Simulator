using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts;

/////////////////////////////////////////////////////////////////
// Bon, on va pas se mentir, la logique de ces fonction est    //
// un peu compliqu�, mais l'aventage c'est que �a permet de    //
// de g�rer n'importe quel type d'item (PlantItem, SeedItem    //
// et ObjectItem).                                             //
/////////////////////////////////////////////////////////////////

public static class Utilities
{
    public static Vector3 GetTransformForward(Transform transform)
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.normalized;
    }

    public static Vector3 GetTransformRight(Transform transform)
    {
        Vector3 right = transform.right;
        right.y = 0f;
        return right.normalized;
    }

    public static void RotateList<T>(List<T> list)
    {
        if (list.Count > 1)
        {
            T firstItem = list[0];
            list.RemoveAt(0);
            list.Add(firstItem);
        }
    }

    public static void SetEmission(Material material, float value)
    {
        if (material != null)
        {
            if (material.HasProperty("_EmissionColor"))
            {
                Color finalEmissionColor = Color.white * value;
                material.SetColor("_EmissionColor", finalEmissionColor);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                Debug.LogError("Le shader du mat�riau ne prend pas en charge l'�mission.");
            }
        }
        else
        {
            Debug.LogError("Le mat�riau cible n'est pas d�fini.");
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////
    // Les deux premi�res fonctions te permettent de r�cup�rer                        //
    // une r�f�rence � n'importe quel item stock� dans l'inventaire                   //
    // � partir de son nom ou de son prefab.                                          //
    //                                                                                //
    // Dans le cas o� tu as un nom ou un prefab comme r�f�rence                       //
    // mais que l'item que tu cherches peut �tre de n'importe quel                    //
    // type, tu devras l'utiliser dans une fonction qui prend comme                   //
    // param�tre un type g�n�rique T (la m�me impl�mentation que                      //
    // ces fonctions). Tu la stockes dans une variable g�n�rique ce qui               //
    // donne => T item = Utilities.GetItemByName<T>(inventory, "itemName");           //
    // Normalement t'en auras pas besoin.                                             //
    //                                                                                //
    // Par contre, dans le cas o� tu es s�r et certain de quel type                   //
    // d'item tu recherches, tu dois utiliser ces fonctions en leur                   //
    // pr�cisant quel type tu recherches, ce qui va donner un truc                    //
    // du genre =>                                                                    //
    // PlantItem plant = Utilities.GetItemByName<PlantItem>(inventory, "plantName");  //
    ////////////////////////////////////////////////////////////////////////////////////

    public static T GetItemByName<T>(Dictionary<Type, Dictionary<IItems, int>> inventory, string name) where T : Items, IItems
    {
        var possibleTypes = typeof(T).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Items)) && !type.IsAbstract).ToList();

        foreach (var itemType in possibleTypes)
        {
            if (inventory.ContainsKey(itemType))
            {
                var item = inventory[itemType].Keys.OfType<T>().FirstOrDefault(i => i.ItemName == name);

                if (item != null)
                {
                    return item;
                }
            }
        }

        return null;
    }

    public static T GetItemByPrefab<T>(Dictionary<Type, Dictionary<IItems, int>> inventory, GameObject prefab) where T : Items, IItems
    {
        var possibleTypes = typeof(T).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Items)) && !type.IsAbstract).ToList();

        foreach (var itemType in possibleTypes)
        {
            if (inventory.ContainsKey(itemType))
            {
                var item = inventory[itemType].Keys.OfType<T>().FirstOrDefault(i => i.Prefab == prefab);

                if (item != null)
                {
                    return item;
                }
            }
        }

        return null;
    }

    //////////////////////////////////////////////////////////////////////////////
    // Celle-l�, �a m'�tonnerait fort que t'en ais besoin, donc t'inqui�tes.    //
    //////////////////////////////////////////////////////////////////////////////

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

    ////////////////////////////////////////////////////////////////////////////////////
    // Pour les deux prochaines fonctions, comme leurs noms l'indiquent, �a te        //
    // retourne le nombre d'exemplaire d'un item pr�cis dans l'inventaire du joueur,  //
    // soit en fonction de son nom ou de son prefab. Rien de particulier pour les     //
    // utiliser, c'est une impl�mentation classique.                                  //
    ////////////////////////////////////////////////////////////////////////////////////

    public static int GetNumberOfItemByName(Dictionary<Type, Dictionary<IItems, int>> inventory, string name)
    {
        foreach (var type in inventory)
        {
            var itemDictionary = type.Value;
            var itemType = type.Key;

            var item = itemDictionary.Keys.OfType<IItems>().FirstOrDefault(i => i.ItemName == name);

            if (item != null)
            {
                return inventory[itemType][item];
            }
        }

        return -1;
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

    ////////////////////////////////////////////////////////////////////////////////////
    // Les deux derni�res fonctions qui te seront utiles pour update le nombre        //
    // d'exemplaire d'un item en particulier, soit en enlever soit en rajouter.       //
    // J'ai jamais eu de cas o� j'avais de nom, donc j'utilisais les prefabs,         //
    // mais si jamais tu en as besoin tu peux juste en faire des copies qui           //
    // utilisent une string plut�t qu'un GameObject.                                  //
    ////////////////////////////////////////////////////////////////////////////////////

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
                PlayerManager.instance.UpdateUIInventory();
            }
            else
            {
                Debug.Log("AUCUN ITEM CORRESPONDANT AU PREFAB TROUV� DANS L'INVENTAIRE");
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
                    inventory[itemType][item] = inventory[itemType][item] - 1;
                    PlayerManager.instance.UpdateUIInventory();
                }
                else
                {
                    Debug.Log("L'INVENTAIRE NE CONTIENT DEJA PLUS D'ITEM CORRESPONDANT A CE PREFAB");
                }
            }
            else
            {
                Debug.Log("AUCUN ITEM CORRESPONDANT AU PREFAB TROUV� DANS L'INVENTAIRE");
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // T'auras jamais besoin de celle-l�, elle sert juste � instancier le   //
    // dictionnaire de l'inventaire (dans l'Awake() de PlayerInventory).    //
    // Si tu veux udpate l'inventaire, utilise plut�t les deux fonctions    //
    // juste au-dessus.                                                     //
    //////////////////////////////////////////////////////////////////////////

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
