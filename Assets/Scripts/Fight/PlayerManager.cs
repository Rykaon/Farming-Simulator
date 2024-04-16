using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using static Cinemachine.DocumentationSortingAttribute;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using Map;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Level level;

    public PlayerControls playerControls { get; private set; }
    public Pathfinding pathfinding { get; private set; }

    [Header("Component References")]
    [SerializeField] public CameraManager cameraManager;
    [SerializeField] public PlayerController_Fight PC_fight;
    [SerializeField] public PlayerController_Farm PC_farm;
    [SerializeField] public VirtualMouseManager virtualMouseManager;
    [SerializeField] public MapGenerator mapGenerator;
    [SerializeField] public Animator animator;
    [SerializeField] public Rigidbody rigidBody;
    [SerializeField] public UnitMovePathfinding movePathfinding;
    [SerializeField] public PlayerInventory inventory;
    [SerializeField] public DialogueManager dialogueManager;

    [Header("VFX References")]
    [SerializeField] private ParticleSystem particuleSystem;

    [Header("UI References")]
    [SerializeField] public GameObject athFarm;
    [SerializeField] public GameObject athFight;
    [SerializeField] public UnitOrderUI fightOrderUI;
    [SerializeField] private TextMeshProUGUI argent;
    [SerializeField] private TextMeshProUGUI plantes;
    [SerializeField] private TextMeshProUGUI actions;


    public enum ControlState
    {
        Farm,
        FarmUI,
        Fight,
        FightUI,
        World,
        WorldUI
    }

    public enum ActionState
    {
        Plant,
        Collect,
        Object,
        Water,
        Sun
    }

    public enum Turn
    {
        Player,
        Plant,
        Unit
    }

    public enum Position
    {
        Outside,
        Inside
    }

    public ControlState controlState;
    public ActionState actionState;
    public Turn turn;
    public Position position;

    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject unitPrefab;
    [HideInInspector] public GameObject plantPrefab = null;
    [HideInInspector] public GameObject objectPrefab = null;
    public PathNode playerNode;

    public List<GameObject> tilesList;
    public List<GameObject> plantList;
    public List<GameObject> unitList;
    public List<GameObject> entitiesList;

    [SerializeField] public int maxActionRange;
    [SerializeField] public int maxMoveRange;
    [SerializeField] public int maxActionPoints;
    public int currentDistanceMoved;
    public int curentActionDistanceMoved;
    public int currentActionsPoints;
    public int moveRange;
    public int actionRange;

    public bool isBoosted = false;
    public int boostFactor = 0;

    private bool isPress;
    public bool LBRBisPressed = false;

    private void Awake()
    {
        instance = this;
        playerControls = new PlayerControls();
        pathfinding = new Pathfinding(9, 15);
        turn = Turn.Player;
        position = Position.Inside;

        tilesList = new List<GameObject>();
        plantList = new List<GameObject>();
        unitList = new List<GameObject>();

        currentActionsPoints = maxActionPoints;
        currentDistanceMoved = 0;
        curentActionDistanceMoved = 0;
        actionRange = maxActionRange;
        moveRange = maxMoveRange;

        SetGrid(level);
    }

    ////////////////////////////////////////////////////////////////////////////////////////
    // La première fonction sert à instancier les tiles du vaisseau. Le format est        //
    // similaire avec ce qu'on a vu pour le Sokoban donc tu devrais pas être paumé.       //
    // Le code c'est : F(Floor), J(Joueur), U(Unit) et P(Plant) et E(Empty) pour          //
    // les cases virtuelles qu'on utilise pas sur la grille.                              //
    //                                                                                    //
    // On instantie un floor dans toutes les cases sauf les E(Empty).                     //
    // Étant donné qu'on commence le jeu avec un vaisseau vide, la fonction est pas       //
    // compliqué, mais j'ai mis plein de trucs en commentaire, fais-y pas gaffe,          //
    // c'est la logique au cas-où le joueur aurait déjà une sauvegarde et on recréerait   //
    // le setup qu'il avait pour qu'il retrouve les mêmes plantes et tout, c'est une      //
    // aide pour plus tard mais pour l'instant no care.                                   //
    ////////////////////////////////////////////////////////////////////////////////////////

    private void SetGrid(Level level)
    {
        string[][] levelObjects = level.Content.Split('\n').Select(x => x.Split(',')).ToArray();

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 15; ++j)
            {
                if (levelObjects[i][j][0] == 'E')
                {
                    PathNode node = pathfinding.GetNodeWithCoords(i, j);
                    node.isVirtual = true;
                    node.isContainingUnit = false;
                    node.unit = null;
                    node.isWalkable = false;
                }
                else
                {
                    GameObject tile = Instantiate(groundTilePrefab, new Vector3(i, 0, j), Quaternion.identity);
                    PathNode node = pathfinding.GetNodeWithCoords(i, j);
                    node.SetTile(tile);
                    node.SetTileManager(tile.GetComponent<TileManager>());
                    tilesList.Add(tile);
                    node.isVirtual = false;
                    node.isContainingUnit = false;
                    node.unit = null;
                    node.isWalkable = true;
                }

                if (levelObjects[i][j][0] == 'U')
                {
                    /*GameObject unit = Instantiate(unitPrefab, new Vector3(i, 1, j), Quaternion.identity);
                    node.isContainingUnit = true;
                    node.unit = unit;
                    node.isWalkable = false;
                    unitList.Add(unit);
                    unit.GetComponent<UnitManager>().index = unitList.Count - 1;

                    if (levelObjects[i][j].Length > 1)
                    {
                        if (levelObjects[i][j][1] == 'P')
                        {
                            node.isSeeded = true;
                            node.isPlant = true;
                            
                            GameObject plant = null;
                            if (levelObjects[i][j][2] == '1')
                            {
                                plant = Instantiate(plantPrefab_1, new Vector3(i, 0, j), Quaternion.identity);
                            }
                            else if (levelObjects[i][j][2] == '2')
                            {
                                plant = Instantiate(plantPrefab_2, new Vector3(i, 0, j), Quaternion.identity);
                            }
                            else if (levelObjects[i][j][2] == '3')
                            {
                                plant = Instantiate(plantPrefab_3, new Vector3(i, 0, j), Quaternion.identity);
                            }

                            node.plant = plant;
                            plantList.Add(plant);
                            plant.GetComponent<PlantManager>().index = plantList.Count - 1;
                        }
                    }*/
                }
                else if (levelObjects[i][j][0] == 'P')
                {
                    /*GameObject plant = null;
                    if (levelObjects[i][j][1] == '1')
                    {
                        plant = Instantiate(plantPrefab_1, new Vector3(i, 0, j), Quaternion.identity);
                    }
                    else if (levelObjects[i][j][1] == '2')
                    {
                        plant = Instantiate(plantPrefab_2, new Vector3(i, 0, j), Quaternion.identity);
                    }
                    else if (levelObjects[i][j][1] == '3')
                    {
                        plant = Instantiate(plantPrefab_3, new Vector3(i, 0, j), Quaternion.identity);
                    }

                    plantList.Add(plant);
                    node.isSeeded = true;
                    node.isPlant = true;
                    node.plant = plant;
                    node.isWalkable = true;
                    node.isContainingUnit = false;
                    node.unit = null;
                    plant.GetComponent<PlantManager>().index = plantList.Count - 1;*/
                }
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    // Celle-là utilise exactement la même logique mais pour l'instantiation des ennemis    //
    // au début d'un combat. Du coup ça veut dire que les ennemis sont toujours instantiés  //
    // sur les mêmes cases. La seule vérification importante qu'il faut faire               //
    // c'est qu'ils ne peuvent pas apparaître sur la case du joueur, mais pour ça faut      //
    // vérifier sur quelle case est le joueur à ce moment-là (ça peut ne pas correspondre   //
    // à la case J(Joueur) du level).                                                       //
    //                                                                                      //
    // Le mieux si tu veux te faire chier c'est même de virer tout ça et d'utiliser un      //
    // système d'aléatoire pour que les ennemis spawns à des endroits différents entre les  //
    // combats. Normalement c'est pas difficile, et peut-être que pour la présentation      //
    // c'est pas nécéssaire mais si tu veux pousser le truc, ça pourrait plaire à Lénophie. //
    //////////////////////////////////////////////////////////////////////////////////////////

    public void SetUnits(int nbrUnits, bool isBoss)
    {
        entitiesList = new List<GameObject>();
        entitiesList.Add(gameObject);

        for (int i = 0; i < plantList.Count; ++i)
        {
            entitiesList.Add(plantList[i]);
        }

        // nbrUnits doit devenir un paramètre de la fonction pour instancier le nombre d'ennemis que l'on veut selon le combat choisi
        List<PathNode> emptyNodes = new List<PathNode>();

        for (int i = 0; i < pathfinding.GetGrid().GetWidth(); i++)
        {
            for (int j = 0; j < pathfinding.GetGrid().GetHeight(); ++j)
            {
                PathNode node = pathfinding.GetNodeWithCoords(i, j);

                if (node != null)
                {
                    if (!node.isVirtual)
                    {
                        if (node.plant == null && !node.isContainingUnit)
                        {
                            emptyNodes.Add(node);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < Mathf.Min(nbrUnits, emptyNodes.Count); i++)
        {
            int randomIndex = Random.Range(0, emptyNodes.Count);
            GameObject unit;

            if (isBoss && i == 0)
            {
                // Changer en instantiation d'un Boss
                unit = Instantiate(unitPrefab, new Vector3(emptyNodes[randomIndex].x, 1, emptyNodes[randomIndex].y), Quaternion.identity);
            }
            else
            {
                unit = Instantiate(unitPrefab, new Vector3(emptyNodes[randomIndex].x, 1, emptyNodes[randomIndex].y), Quaternion.identity);
            }

            emptyNodes[randomIndex].isContainingUnit = true;
            emptyNodes[randomIndex].unit = unit;
            emptyNodes[randomIndex].isWalkable = false;
            unitList.Add(unit);
            entitiesList.Add(unit);
            unit.GetComponent<UnitManager>().index = unitList.Count - 1;
            emptyNodes.RemoveAt(randomIndex);
        }



        /*string[][] levelObjects = level.Content.Split('\n').Select(x => x.Split(',')).ToArray();

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 15; ++j)
            {
                PathNode node = pathfinding.GetNodeWithCoords(i, j);

                if (levelObjects[i][j][0] == 'F' || levelObjects[i][j][0] == 'J')
                {

                }
                else if (levelObjects[i][j][0] == 'U')
                {
                    GameObject unit = Instantiate(unitPrefab, new Vector3(i, 1, j), Quaternion.identity);
                    node.isContainingUnit = true;
                    node.unit = unit;
                    node.isWalkable = false;
                    unitList.Add(unit);
                    unit.GetComponent<UnitManager>().index = unitList.Count - 1;
                }
                else if (levelObjects[i][j][0] == 'P')
                {

                }
            }
        }*/
    }

    //////////////////////////////////////////////////////////////////////////////////////////
    // La fonction qui setup le passage du mode "Farm" au mode "Fight" et inversement. Pour //
    // savoir quelle actionMap utiliser pour les inputs du joueur avec le NewInputSystem,   //
    // je fais la distinction si le joueur est en train d'utiliser les menus ou pas.        //
    // Au final ça donne quatre modes distincts, "Farm", "FarmUI", "Fight" et "FightUI".    //
    //                                                                                      //
    // Normalement, t'as pas besoin d'y toucher, tout est déjà en place et fonctionnel.     //
    //////////////////////////////////////////////////////////////////////////////////////////

    public void ChangeState(ControlState state)
    {
        switch (state)
        {
            case ControlState.World:
                controlState = ControlState.World;
                playerControls.Gamepad.Enable();
                playerControls.UI.Disable();
                PC_farm.isActive = true;
                PC_fight.isActive = false;
                break;

            case ControlState.WorldUI:
                controlState = ControlState.World;
                playerControls.Gamepad.Disable();
                playerControls.UI.Enable();
                PC_farm.isActive = false;
                PC_fight.isActive = false;
                break;

            case ControlState.Farm:
                controlState = ControlState.Farm;
                playerControls.Gamepad.Enable();
                playerControls.UI.Disable();
                PC_farm.isActive = true;
                PC_fight.isActive = false;
                break;

            case ControlState.FarmUI:
                controlState = ControlState.Farm;
                playerControls.Gamepad.Disable();
                playerControls.UI.Enable();
                PC_farm.isActive = false;
                PC_fight.isActive = false;
                break;

            case ControlState.Fight:
                controlState = ControlState.Fight;
                playerControls.Gamepad.Enable();
                playerControls.UI.Disable();
                PC_farm.isActive = false;
                PC_fight.isActive = true;
                break;

            case ControlState.FightUI:
                controlState = ControlState.Fight;
                playerControls.Gamepad.Disable();
                playerControls.UI.Enable();
                PC_farm.isActive = false;
                PC_fight.isActive = false;
                break;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    // Toutes les fonctions qui vont gérer l'ordre des actions pendant le combat.               //
    //                                                                                          //
    // Lorsque le joueur rentre en combat, c'est son tour. Une fois que ses points d'actions    //
    // sont à 0 ou qu'il passe sont tour, on lui reset ses stats et on lance une première       //
    // coroutine qui fait jouer toutes les plantes dans l'ordre dans lequel elles ont atteint   //
    // la taille adulte (pas l'ordre de plantage, mais l'ordre d'arrosage si tu prèfères).      //
    // Une fois le tour des plantes terminé, à la fin de la couroutine, j'en lance une          //
    // deuxième qui va gérer le tour des ennemis. A la fin de cette couroutine, le joueur       //
    // récupère le contrôle de son avatar et on recommence.                                     //
    //                                                                                          //
    // Pour résumer : Joueur > Plantes > Ennemis > Joueur > Plantes > Ennemis, etc.             //
    //////////////////////////////////////////////////////////////////////////////////////////////

    public void EndTurn()
    {
        PlayerController_Fight.instance.isActive = false;
        playerControls.Gamepad.Disable();
        playerControls.UI.Disable();

        ResetStats();

        Debug.Log("Player Turn Ended");
        StartCoroutine(PlantsTurn());
    }

    public void ResetStats()
    {
        moveRange = maxMoveRange;
        actionRange = maxActionRange;
        currentActionsPoints = maxActionPoints;
        currentDistanceMoved = 0;

        // isBoosted et boostFactor servent à savoir si les stats du joueur sont boostés par les plantes boost.
        // A la fin du tour du joueur, on reset ses stats et donc les boost. Pendant le tour des plantes juste
        // après, les boost s'appliquent si les conditions sont réunies. Lorsque le joueur récupère le contrôle
        // de son avatar à son prochain tour, il garde les boosts qu'il a eu des plantes. Une fois son tour fini,
        // on recommence l'opération.
        // Pareil pour les plantes, ça veut dire que si on veut qu'une plante soit boostée, elle doit jouer APRES
        // la plante qui la boost.
        for (int i = 0; i < pathfinding.GetGrid().GetWidth(); ++i)
        {
            for (int j = 0; j < pathfinding.GetGrid().GetHeight(); ++j)
            {
                if (pathfinding.GetNodeWithCoords(i, j) != null)
                {
                    if (!pathfinding.GetNodeWithCoords(i, j).isVirtual)
                    {
                        if (pathfinding.GetNodeWithCoords(i, j).tileManager.isBoosted)
                        {
                            Destroy(pathfinding.GetNodeWithCoords(i, j).tileManager.boostVFX);
                            pathfinding.GetNodeWithCoords(i, j).tileManager.boostVFX = null;
                            pathfinding.GetNodeWithCoords(i, j).tileManager.isBoosted = false;
                        }
                    }
                }
            }
        }

        isBoosted = false;
        boostFactor = 0;

        UpdateFightUI();
    }

    public void SetCameraTarget(Transform follow, Transform look)
    {
        if (cameraManager.cameraTransition != null)
        {
            StopCoroutine(cameraManager.cameraTransition);
        }

        Debug.Log("kjbfkjfs");

        cameraManager.cameraTransition = StartCoroutine(cameraManager.SetCameraTarget(follow, look));
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////
    // Les deux coroutines qui gèrent les tours des plantes et des ennemis fonctionnent            //
    // exactement sur la même logique. La couroutine sert juste à passer dans une boucle pour      //
    // gérer le tour des plantes/ennemis les uns à la suite. Les plantes sont ajoutées à la liste  //
    // quand elles arrivent à maturité (arrosées ou ensolleillées), et les ennemis dans l'odre de  //
    // leur spawn. Donc parcourir l'index suffit à gérer qui joue dans quel ordre.                 //
    //                                                                                             //
    // Les comportements spécifiques des plantes et ennemis sont gérés respectivement dans les     //
    // scripts PlantManager et UnitManager. Leurs actions utilisent aussi des coroutines.          //
    // Au début, on set la variable isActive = true, qui repassera à false à la fin des            //
    // coroutines d'exécution de leur comportement. Donc on reste dans la boucle While tant que    //
    // isActive = true, et lorsqu'on en sort on passe à la prochaine plante ou au prochain ennemi. //
    //                                                                                             //
    // Entre chaque plante ou ennemis on vérifie si le combat est gagné ou s'il est perdu.         //
    /////////////////////////////////////////////////////////////////////////////////////////////////

    public IEnumerator PlantsTurn()
    {
        for (int i = 0; i < plantList.Count; i++)
        {
            fightOrderUI.SetNextEntity();
            Debug.Log("Plant " + i + " turn Start");
            PlantManager plantManager = plantList[i].GetComponent<PlantManager>();
            plantManager.isActive = true;
            SetCameraTarget(plantManager.transform, plantManager.transform);

            while (plantManager.isActive)
            {
                yield return new WaitForEndOfFrame();
            }

            if (CheckWinCondition())
            {
                fightOrderUI.gameObject.SetActive(false);
                foreach (GameObject unit in unitList)
                {
                    unit.GetComponent<UnitManager>().unitNode.isContainingUnit = false;
                    unit.GetComponent<UnitManager>().unitNode.unit = null;
                    unit.GetComponent<UnitManager>().unitNode.isWalkable = true;
                    Destroy(unit);
                }
                unitList.Clear();

                ChangeState(ControlState.World);

                athFarm.SetActive(true);
                athFight.SetActive(false);
                mapGenerator.TakeReward();
                yield break;
            }
            Debug.Log("Plant " + i + " turn End");
        }

        StartCoroutine(UnitsTurn());

        Debug.Log("Plants Turn Ended");
    }

    public IEnumerator UnitsTurn()
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            fightOrderUI.SetNextEntity();
            UnitManager unitManager = unitList[i].GetComponent<UnitManager>();
            unitManager.isActive = true;
            SetCameraTarget(unitManager.transform, unitManager.transform);

            while (unitManager.isActive)
            {
                //Debug.Log("Unit " + i + " = " + unitList[i].GetComponent<UnitManager>().unitNode);
                yield return new WaitForEndOfFrame();
            }

            if (CheckLooseCondition())
            {
                foreach (GameObject unit in unitList)
                {
                    unit.GetComponent<UnitManager>().unitNode.isContainingUnit = false;
                    unit.GetComponent<UnitManager>().unitNode.unit = null;
                    unit.GetComponent<UnitManager>().unitNode.isWalkable = true;
                    Destroy(unit);
                }
                unitList.Clear();
                ChangeState(ControlState.World);

                // fin de la run
                yield break;
            }
        }

        PC_fight.isActive = true;
        playerControls.Gamepad.Enable();
        playerControls.UI.Disable();
        SetCameraTarget(transform, transform);
        fightOrderUI.SetNextEntity();


        Debug.Log("Unit Turn Ended");
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Les conditions de défaite et de victoire, mais pour être honnête y'a que la logique        //
    // pour savoir si on a gagné ou perdu le combat qui est implémenté. Pour effectuer le         //
    // comportement qui résulte d'une défaite/victoire, ça se fait juste au-dessus dans les       //
    // coroutines des plantes/ennemis.                                                            //
    //                                                                                            //
    // Je vais être honnête, ça fait un moment que j'ai pas touché au code, et il me semble       //
    // qu'en cas de victoire/défaite j'ai fait le strict minimum, y'a moyen que ça mérite un peu  //
    // de débug. Et aucune récompense/punition n'est donnée au joueur encore. Donc si jamais      //
    // t'as une idée simple et brillante, amuses-toi.                                             //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    private bool CheckWinCondition()
    {
        if (unitList.Count == 0)
        {
            return true;
        }

        return false;
    }

    private bool CheckLooseCondition()
    {
        if (plantList.Count == 0)
        {
            return true;
        }

        return false;
    }

    public void SetLooseRun(bool isFightLoose, bool isRunLoose)
    {
        GameObject info = null;

        if (isFightLoose && !isRunLoose)
        {
            //info = mapInfoEventNotCheck;
        }
        else if (!isFightLoose && isRunLoose)
        {
            //info = mapInfoDestinationNotSet;
        }
        else
        {
            Debug.Log("Parameters not set");
            return;
        }

        StartCoroutine(SetLooseRun(info));
    }

    public IEnumerator SetLooseRun(GameObject info)
    {
        info.SetActive(true);
        yield return new WaitForSecondsRealtime(5);
        // Retour au menu
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // Une fonction qui détecte si un input est isPressed() pendant un certain temps.                 //
    //                                                                                                //
    // Basiquement je m'en sers pour savoir si le joueur appuie une fois rapidement sur               //
    // le bouton B ou s'il reste appuyé dessus longtemps. Si le joueur est en combat et que c'est     //
    // un appui court, il passe son tour, mais si c'est un appuie long, ça met fin au combat.         //
    //                                                                                                //
    // Je t'avoue que le fait que ça mette fin au combat, c'est pas vraiment une question de GD,      //
    // c'est plus une fonctionnalité utile pour tester le proto. MAis normalement t'as compris        //
    // la logique.                                                                                    //
    //                                                                                                //
    // Et truc en plus, la fonction prends comme paramètre un InputAction, donc un input dans         //
    // l'actionMap du NewInputSystem. Pour l'instant dans la fonction je fais pas de vérification     //
    // pour savoir quel est l'input passé en paramètre parce que je n'appelle cette fonction que      //
    // dans le cas où le joueur appuie sur B. Mais techniquement, si tu rajoutes des conditions       //
    // pour checker quel est l'input, tu peux rajouter des comportements différents pour chaque       //
    // touche de la manette entre appui long et appuie court.                                         //
    //                                                                                                //
    // Si tu sens que c'est pertinent de l'utiliser avec d'autres boutons en d'autres circonstances,  //
    // fais-toi plaisir. Pour l'instant : Mode Farm et B court => Rien.                               //
    // Mode Farm et B long => on lance un combat. Mode Fight et B court, le joueur passe son tour.    //
    // Mode Fight et B long, on stop le combat et on retourne farmer.                                 //
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    private IEnumerator InputLongPress(InputAction action)
    {
        float elapsedTime = 0;
        float pressTime = 1;
        
        while (elapsedTime < pressTime)
        {
            elapsedTime += Time.deltaTime;
            if (!action.IsPressed() && elapsedTime < 0.5f)
            {
                isPress = false;

                switch (controlState)
                {
                    case ControlState.Farm:
                        
                        break;

                    case ControlState.Fight:

                        if (PC_fight.isActive)
                        {
                            EndTurn();
                        }
                        break;
                }
            }
            yield return new WaitForEndOfFrame();
        }

        if (isPress)
        {   
            switch (controlState)
            {
                case ControlState.Farm:
                    mapGenerator.travelElapsedTime = mapGenerator.travelTime;
                    break;

                case ControlState.Fight:
                    
                    break;
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////
    // Pour éviter de constamment utiliser de la mémoire pour update l'UI  //
    // j'ai fait des fonctions publiques pour les updates seulement quand  //
    // un changement est fait dans l'inventaire ou au marchand.            //
    //                                                                     //
    // Si jamais tu vois un bug dans l'affichage de l'inventaire ou des    //
    // informations de combat du joueur, c'est sûrement du à un oubli      //
    // d'appel de ces fonctions. Mais dans le doute je les ai appelé       //
    // directement dans les fonctions de la classe Utilities qui update    //
    // l'inventaire, donc le problème devrait être réglé à la source.      //
    /////////////////////////////////////////////////////////////////////////

    public void UpdateUIInventory()
    {
        argent.text = inventory.nbArgent.ToString() + " $";

        string nbrOfITemToDisplay = "";
        for (int i = 0; i < inventory.plantsList.Count; i++)
        {
            nbrOfITemToDisplay = nbrOfITemToDisplay + inventory.plantsList[i].ItemName[0] + ":" + Utilities.GetNumberOfItemByPrefab(inventory.inventory, inventory.plantsList[i].prefab).ToString() + " / ";
        }
        plantes.text = nbrOfITemToDisplay;
    }

    public void UpdateFightUI()
    {
        actions.text = currentActionsPoints.ToString() + " / " + maxActionPoints + " Actions";
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////
    // Cet update est assez court et ne check pas grand chose. La plupart des inputs sont checks   //
    // séparément soit dans le PlayerController_Farm soit le PlayerController_Fight, ça permet     //
    // d'éviter de tout rassembler en un seul méga update complètement tentaculaire et illisible.  //
    //                                                                                             //
    // Dans celui-là, (et comme dans les autres) on check d'abord les bool qui detectent si un     //
    // input est isPressed ou non pour éviter que des fonctions soient appelées tous les updates   //
    // alors que le joueur n'a appuyé qu'une fois. Ici on ne check que le bouton B. S'il était     //
    // préssé la frame d'avant et qu'il ne l'est plus, on reset la bool pour pouvoir écouter le    //
    // prochain input. Lorsqu'on le détecte, on fait le comportement décris dans la fonction       //
    // juste au-dessus.                                                                            //
    //                                                                                             //
    // Dernière chose, c'est ici qu'on check si c'est le tour du joueur et que ses points d'action //
    // sont à 0 ou non. Si oui, on passe le tour du joueur et on commence le tour des IA.          //
    /////////////////////////////////////////////////////////////////////////////////////////////////

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //particuleSystem.Stop();

            /*if (!virtualMouseManager.isActive)
            {
                mapGenerator.ShowHideUIMap(true);
                ChangeState(ControlState.WorldUI);
                virtualMouseManager.Enable();
            }
            else
            {
                mapGenerator.ShowHideUIMap(false);
                ChangeState(ControlState.Farm);
                virtualMouseManager.Disable(false);
            }*/
            
        }

        if (isPress)
        {
            if (!playerControls.Gamepad.B.IsPressed())
            {
                isPress = false;
            }
        }

        if (LBRBisPressed)
        {
            if (!playerControls.Gamepad.LB.IsPressed() && !playerControls.Gamepad.RB.IsPressed())
            {
                LBRBisPressed = false;
}
        }

        switch (controlState)
        {
            case ControlState.Farm:
                if (turn == Turn.Player && playerControls.Gamepad.B.IsPressed() && !isPress)
                {
                    isPress = true;
                    StartCoroutine(InputLongPress(playerControls.Gamepad.B));

                }
                break;

            case ControlState.Fight:
                if (turn == Turn.Player && currentDistanceMoved == maxActionPoints * maxMoveRange)
                {
                    EndTurn();
                }

                if (turn == Turn.Player && playerControls.Gamepad.B.IsPressed() && !isPress)
                {
                    isPress = true;
                    StartCoroutine(InputLongPress(playerControls.Gamepad.B));

                }

                if (turn == Turn.Player && playerControls.Gamepad.LB.IsPressed() && !LBRBisPressed)
                {
                    fightOrderUI.RunThroughEntities(-1);
                    LBRBisPressed = true;
                }
                else if (turn == Turn.Player && playerControls.Gamepad.RB.IsPressed() && !LBRBisPressed)
                {
                    fightOrderUI.RunThroughEntities(1);
                    LBRBisPressed = true;
                }
                break;
        }
    }
}
