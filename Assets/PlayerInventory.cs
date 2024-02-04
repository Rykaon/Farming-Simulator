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
    public int nbArgent { get; private set; }
    public int nbPlanteRouge { get; private set; }
    public int nbPlanteBleu { get; private set; }
    public int nbPlanteJaune { get; private set; }
    //Dans le PlayerControllerFarm il faudra vérifier que le nbPlante>0 pour pouvoir planter la plante correspondante

    private PlayerControls playerControls;

    private void Start()
    {
        playerControls = PC_Manager.playerControls;

        nbArgent = nbStartArgent;
        nbPlanteRouge = nbStartPlant;
        nbPlanteBleu = nbStartPlant;
        nbPlanteJaune = nbStartPlant;
    }

    public void BuyPlant(string plantName)
    {
        nbArgent = nbArgent - 15;

        switch (plantName)
        {
            case "Rouge":
                nbPlanteRouge++;
                break;
            case "Bleu":
                nbPlanteBleu++;
                break;
            case "Jaune":
                nbPlanteJaune++;
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
                nbPlanteRouge--;
                break;
            case "Bleu":
                nbPlanteBleu--;
                break;
            case "Jaune":
                nbPlanteJaune--;
                break;
            default:
                Debug.LogWarning("La plante acheté n'est pas valide !");
                break;
        }
    }
}
