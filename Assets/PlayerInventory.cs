using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private PlayerController_Farm playerControllerFarm;
    [SerializeField] private PlayerManager PC_Manager;

    [SerializeField] private int nbStartPlant;
    [SerializeField] private int nbStartArgent;

    public bool isShopOpen = false;

    public PlantItem plantAttack;
    public PlantItem plantMove;
    public PlantItem plantBoost;

    public int nbArgent;
    public int nbAttack;
    public int nbMove;
    public int nbBoost;
    //Dans le PlayerControllerFarm il faudra vérifier que le nbPlante>0 pour pouvoir planter la plante correspondante

    private PlayerControls playerControls;

    private void Start()
    {
        playerControls = PC_Manager.playerControls;

        nbArgent = nbStartArgent;
        nbAttack = nbStartPlant;
        nbMove = nbStartPlant;
        nbBoost = nbStartPlant;
    }

    public void BuyPlant(string plantName)
    {
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
    }

    public void SellPlant(string plantName)
    {
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
    }
}
