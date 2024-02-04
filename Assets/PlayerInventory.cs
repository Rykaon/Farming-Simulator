using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Component References")]
    private PlayerControls playerControls;
    [SerializeField] private PlayerController_Farm playerControllerFarm;
    [SerializeField] private PlayerManager PC_Manager;

    [SerializeField] private int nbStartArgent;
    [SerializeField] private int nbStartPlant;

    public bool isShopOpen = false;

    public int nbAttack;
    public int nbMove;
    public int nbBoost;

    public List<PlantItem> itemList;
    public List<int> itemNbrList;
    public int nbArgent;

    private void Awake()
    {
        playerControls = PC_Manager.playerControls;

        itemList = new List<PlantItem>();
        itemNbrList = new List<int>();
        
        // Pour l'instant on instantie une valeur au nombre d'item pour pouvoir travailler mais ça devra être enlevé plus tard
        for (int i = 0; i < itemList.Count; i++)
        {
            itemNbrList[i] = nbStartPlant;
        }
        nbAttack = nbStartPlant;
        nbMove = nbStartPlant;
        nbBoost = nbStartPlant;

    nbArgent = nbStartArgent;
    }

    // **************************** //
    // Rassembler ces deux fonction en une avec un booleen en plus en paramètre
    // **************************** //

    public void BuyPlant(string plantName/* int plantPrice*/)
    {
        // Une fois les changements nécéssaires effectués du coté du DialogueManager et de Inky,
        // enlever cette partie et la remplacer par la partie en commentaire
        // **************************** //
        nbArgent = nbArgent - 15;

        switch (plantName)
        {
            case "Rouge":
                nbAttack++;
                break;
            case "Bleu":
                nbMove++;
                break;
            case "Jaune":
                nbBoost++;
                break;
            default:
                Debug.LogWarning("La plante acheté n'est pas valide !");
                break;
        }
        // **************************** //

        /*int index = GetIndexByName(plantName);

        if (index > -1)
        {
            nbArgent -= plantPrice; //Ajouter en paramètre de la fonction une référence en int qui correspond à Item.buyPrice
            itemNbrList[index]++;
        }*/
    }

    public void SellPlant(string plantName/* int plantPrice, bool isSelling*/)
    {
        // Une fois les changements nécéssaires effectués du coté du DialogueManager et de Inky,
        // enlever cette partie et la remplacer par la partie en commentaire
        // isSelling est un boollen qui dit que true = vendre et false = acheter
        // **************************** //
        nbArgent = nbArgent + 10;

        switch (plantName)
        {
            case "Rouge":
                nbAttack--;
                break;
            case "Bleu":
                nbMove--;
                break;
            case "Jaune":
                nbBoost--;
                break;
            default:
                Debug.LogWarning("La plante acheté n'est pas valide !");
                break;
        }
        // **************************** //

        /*int index = GetIndexByName(plantName);

        if (index > -1)
        {
            if (isSelling)
            {
                nbArgent += plantPrice; //Ajouter en paramètre de la fonction une référence en int qui correspond à Item.sellPrice
                itemNbrList[index]--;
            }
            else
            {
                nbArgent -= plantPrice; //Ajouter en paramètre de la fonction une référence en int qui correspond à Item.buyPrice
                itemNbrList[index]++;
            }
            
        }*/
    }

    public int GetIndexByName(string name)
    {
        for (int i = 0; i < itemList.Count; ++i)
        {
            if (name == itemList[i].name)
            {
                return i;
            }
        }
        
        return -1;
    }

    public int GetIndexByObject(GameObject gameObject)
    {
        for (int i = 0; i < itemList.Count; ++i)
        {
            if (gameObject == itemList[i].prefab)
            {
                return i;
            }
        }

        return -1;
    }
}
