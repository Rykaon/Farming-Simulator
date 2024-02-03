using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Cinemachine.DocumentationSortingAttribute;
using UnityEngine.InputSystem;
using static UnitGridSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public Level level;

    public PlayerControls playerControls { get; private set; }
    public Pathfinding pathfinding { get; private set; }

    [Header("Component References")]
    [SerializeField] public PlayerController_Fight PC_fight;
    [SerializeField] public PlayerController_Farm PC_farm;
    [SerializeField] public VirtualMouseManager virtualMouseManager;
    [SerializeField] public Animator animator;
    [SerializeField] public Rigidbody rigidBody;
    [SerializeField] public UnitMovePathfinding movePathfinding;

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

    public ControlState controlState;
    public ActionState actionState;
    public Turn turn;

    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] public GameObject plantPrefab_1;
    [SerializeField] public GameObject plantPrefab_2;
    [SerializeField] public GameObject plantPrefab_3;
    [HideInInspector] public GameObject plant = null;
    public PathNode playerNode;

    public List<GameObject> tilesList;
    public List<GameObject> plantList;
    public List<GameObject> unitList;

    [SerializeField] public int maxActionRange;
    [SerializeField] public int maxMoveRange;
    [SerializeField] public int maxActionPoints;
    public int currentDistanceMoved;
    public int currentActionsPoints;
    public int moveRange;
    public int actionRange;

    public bool isBoosted = false;
    public int boostFactor = 0;

    private bool isPress;

    private void Awake()
    {
        instance = this;
        playerControls = new PlayerControls();
        pathfinding = new Pathfinding(9, 15);
        turn = Turn.Player;

        tilesList = new List<GameObject>();
        plantList = new List<GameObject>();
        unitList = new List<GameObject>();

        currentActionsPoints = maxActionPoints;
        currentDistanceMoved = 0;
        actionRange = maxActionRange;
        moveRange = maxMoveRange;

        SetGrid(level);
    }

    private void SetUnits(Level level)
    {
        string[][] levelObjects = level.Content.Split('\n').Select(x => x.Split(',')).ToArray();

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
        }
    }

    private void SetGrid(Level level)
    {
        string[][] levelObjects = level.Content.Split('\n').Select(x => x.Split(',')).ToArray();

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 15; ++j)
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
        isBoosted = false;
        boostFactor = 0;
    }

    public IEnumerator PlantsTurn()
    {
        for (int i = 0; i < plantList.Count; i++)
        {
            PlantManager plantManager = plantList[i].GetComponent<PlantManager>();
            plantManager.isActive = true;

            while (plantManager.isActive)
            {
                Debug.Log("Plant " + i + " = " + plantList[i].GetComponent<PlantManager>().plantNode);
                yield return new WaitForEndOfFrame();
            }

            if (CheckWinCondition())
            {

                break;
            }
        }

        StartCoroutine(UnitsTurn());

        Debug.Log("Plant Turn Ended");
    }

    public IEnumerator UnitsTurn()
    {
        for (int i = 0; i < unitList.Count; i++)
        {
            UnitManager unitManager = unitList[i].GetComponent<UnitManager>();
            unitManager.isActive = true;

            while (unitManager.isActive)
            {
                Debug.Log("Unit " + i + " = " + unitList[i].GetComponent<UnitManager>().unitNode);
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

                controlState = ControlState.Farm;
                PC_farm.isActive = true;
                PC_fight.isActive = false;
                yield break;
            }
        }

        PC_fight.isActive = true;
        playerControls.Gamepad.Enable();
        playerControls.UI.Disable();

        Debug.Log("Unit Turn Ended");
    }

    private bool CheckWinCondition()
    {
        bool hasWin = false;

        return hasWin;
    }

    private bool CheckLooseCondition()
    {
        if (plantList.Count == 0)
        {
            return true;
        }

        return false;
    }

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
                    Debug.Log("yo");
                    if (PC_farm.GetCurrentNode() != null)
                    {
                        if (!PC_farm.GetCurrentNode().isVirtual)
                        {
                            playerNode = PC_farm.GetCurrentNode();
                        }
                        else
                        {
                            playerNode = pathfinding.GetNodeWithCoords(0, 0);
                        }
                    }
                    else
                    {
                        playerNode = pathfinding.GetNodeWithCoords(0, 0);
                    }

                    playerNode.isContainingUnit = true;
                    playerNode.unit = gameObject;
                    playerNode.isWalkable = false;

                    ResetStats();

                    SetUnits(level);

                    turn = Turn.Player;
                    controlState = ControlState.Fight;
                    PC_farm.isActive = false;
                    PC_fight.isActive = true;

                    Debug.Log(new Vector3(playerNode.x, transform.position.y, playerNode.y));
                    rigidBody.velocity = Vector3.zero;
                    transform.position = new Vector3(playerNode.x, transform.position.y, playerNode.y);
                    break;

                case ControlState.Fight:
                    foreach (GameObject unit in unitList)
                    {
                        unit.GetComponent<UnitManager>().unitNode.isContainingUnit = false;
                        unit.GetComponent<UnitManager>().unitNode.unit = null;
                        unit.GetComponent<UnitManager>().unitNode.isWalkable = true;
                        Destroy(unit);
                    }
                    unitList.Clear();

                    controlState = ControlState.Farm;
                    PC_farm.isActive = true;
                    PC_fight.isActive = false;
                    break;
            }
        }
    }

    private void Update()
    {
        if (isPress)
        {
            if (!playerControls.Gamepad.B.IsPressed())
            {
                isPress = false;
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
                break;
        }
    }
}
