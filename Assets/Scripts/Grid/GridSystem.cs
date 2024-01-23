using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using Assets.Scripts;
using System.IO;

public class GridSystem : MonoBehaviour
{
    [SerializeField] GameObject uiObject;
    UISystem uiSystem;
    public GridMap<GameObject> tileGrid;
    private PathfindingMe pathfinding;

    public GameObject[,] tileObject;
    public GameObject[,] tileSprite;

    [SerializeField] GameObject tileSpritePrefab;
    [SerializeField] GameObject tileWhite;
    [SerializeField] GameObject tileRed;

    public Sprite tileSpriteBlueDark;
    public Sprite tileSpriteBlueLight;
    public Sprite tileSpriteOrangeDark;
    public Sprite tileSpriteOrangeLight;
    public Sprite tileSpriteWhiteDark;
    public Sprite tileSpriteWhiteLight;
    public Sprite tileSpriteRed;
    public Color colorFull;
    public Color colorTransparent;

    string JSON;
    //SaveDat saveData;

    public List<Vector3> playerOneUnitScales;
    public List<List<Vector3>> playerOneImplantPositions;
    public List<List<Vector3>> playerOneImplantScales;
    public List<List<ActionData>> playerOneActions;
    public List<List<MoveData>> playerOneMoves;

    public List<Vector3> playerTwoUnitScales;
    public List<List<Vector3>> playerTwoImplantPositions;
    public List<List<Vector3>> playerTwoImplantScales;
    public List<List<ActionData>> playerTwoActions;
    public List<List<MoveData>> playerTwoMoves;

    public GameObject spiderWeb;
    public GameObject oil;
    public GameObject energeticParticles;
    public ActionData energeticTower;
    public MoveData basicLinearMove;

    public List<GameObject> playerOneList;
    public List<GameObject> playerTwoList;

    [SerializeField] LevelData levelToLoad;

    public List<GameObject> unitListPlayerOne;
    public List<GameObject> unitListPlayerTwo;
    public GameObject playingUnit;
    public UnitGridSystem playingUnitSystem;

    public bool spawnOne, spawnTwo;

    private PlayerController_MAD playerController;
    private CameraManager cameraManager;
    public Vector3 cameraPivotPos;

    public Team team;
    public enum Team
    {
        Blue,
        Orange
    }

    void Awake()
    {
        playerController = transform.GetComponent<PlayerController_MAD>();
        cameraManager = transform.GetComponent<CameraManager>();
        uiSystem = uiObject.GetComponent<UISystem>();

        /*playerOneUnitScales = JsonUtility.FromJson<List<Vector3>>(File.ReadAllText(Application.dataPath + "/SavedData_One_UnitScales.txt"));
        playerOneImplantPositions = JsonUtility.FromJson<List<List<Vector3>>>(File.ReadAllText(Application.dataPath + "/SavedData_One_ImplantPositions.txt"));
        playerOneImplantScales = JsonUtility.FromJson<List<List<Vector3>>>(File.ReadAllText(Application.dataPath + "/SavedData_One_ImplantScales.txt"));
        playerOneActions = JsonUtility.FromJson<List<List<ActionData>>>(File.ReadAllText(Application.dataPath + "/SavedData_One_Actions.txt"));
        playerOneMoves = JsonUtility.FromJson<List<List<MoveData>>>(File.ReadAllText(Application.dataPath + "/SavedData_One_Moves.txt"));

        playerTwoUnitScales = JsonUtility.FromJson<List<Vector3>>(File.ReadAllText(Application.dataPath + "/SavedData_Two_UnitScales.txt"));
        playerTwoImplantPositions = JsonUtility.FromJson<List<List<Vector3>>>(File.ReadAllText(Application.dataPath + "/SavedData_Two_ImplantPositions.txt"));
        playerTwoImplantScales = JsonUtility.FromJson<List<List<Vector3>>>(File.ReadAllText(Application.dataPath + "/SavedData_Two_ImplantScales.txt"));
        playerTwoActions = JsonUtility.FromJson<List<List<ActionData>>>(File.ReadAllText(Application.dataPath + "/SavedData_Two_Actions.txt"));
        playerTwoMoves = JsonUtility.FromJson<List<List<MoveData>>>(File.ReadAllText(Application.dataPath + "/SavedData_One_Moves.txt"));

        saveData = new SaveData(playerOneUnitScales, playerOneImplantPositions, playerOneImplantScales, playerOneActions, playerOneMoves, playerTwoUnitScales, playerTwoImplantPositions, playerTwoImplantScales, playerTwoActions, playerTwoMoves);*/
        //saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(Application.dataPath + "/SavedData.txt"));

        GameObject playerOneSprite_0 = GameObject.Find("playerOneSprite_0");
        GameObject playerOneSprite_1 = GameObject.Find("playerOneSprite_1");
        GameObject playerOneSprite_2 = GameObject.Find("playerOneSprite_2");
        GameObject playerOneSprite_3 = GameObject.Find("playerOneSprite_3");
        GameObject playerTwoSprite_0 = GameObject.Find("playerTwoSprite_0");
        GameObject playerTwoSprite_1 = GameObject.Find("playerTwoSprite_1");
        GameObject playerTwoSprite_2 = GameObject.Find("playerTwoSprite_2");
        GameObject playerTwoSprite_3 = GameObject.Find("playerTwoSprite_3");

        GameObject newPlayerOneSprite_0 = Instantiate(playerOneSprite_0, new Vector3(5, -2, 5), Quaternion.identity);
        GameObject newPlayerOneSprite_1 = Instantiate(playerOneSprite_1, new Vector3(5, -2, 5), Quaternion.identity);
        GameObject newPlayerOneSprite_2 = Instantiate(playerOneSprite_2, new Vector3(5, -2, 5), Quaternion.identity);
        GameObject newPlayerOneSprite_3 = Instantiate(playerOneSprite_3, new Vector3(5, -2, 5), Quaternion.identity);
        GameObject newPlayerTwoSprite_0 = Instantiate(playerTwoSprite_0, new Vector3(5, -2, 5), Quaternion.identity);
        GameObject newPlayerTwoSprite_1 = Instantiate(playerTwoSprite_1, new Vector3(5, -2, 5), Quaternion.identity);
        GameObject newPlayerTwoSprite_2 = Instantiate(playerTwoSprite_2, new Vector3(5, -2, 5), Quaternion.identity);
        GameObject newPlayerTwoSprite_3 = Instantiate(playerTwoSprite_3, new Vector3(5, -2, 5), Quaternion.identity);

        playerOneList.Add(newPlayerOneSprite_0);
        playerOneList.Add(newPlayerOneSprite_1);
        playerOneList.Add(newPlayerOneSprite_2);
        playerOneList.Add(newPlayerOneSprite_3);
        playerTwoList.Add(newPlayerTwoSprite_0);
        playerTwoList.Add(newPlayerTwoSprite_1);
        playerTwoList.Add(newPlayerTwoSprite_2);
        playerTwoList.Add(newPlayerTwoSprite_3);

        Destroy(playerOneSprite_0);
        Destroy(playerOneSprite_1);
        Destroy(playerOneSprite_2);
        Destroy(playerOneSprite_3);
        Destroy(playerTwoSprite_0);
        Destroy(playerTwoSprite_1);
        Destroy(playerTwoSprite_2);
        Destroy(playerTwoSprite_3);

        spawnOne = false;
        spawnTwo = false;
        team = Team.Orange;
        //playerController.gamepad = Gamepad.all[0];
        //playerController.playerControls.devices = new[] { playerController.gamepad };
        SetUIActive(false);

        cameraPivotPos = new Vector3(levelToLoad.width / 2, 0, levelToLoad.height / 2);

        SetGrid(levelToLoad);
        ResetTileOutline();
        StartCoroutine(Spawn());
    }

    public void SetGrid(LevelData levelToLoad)
    {
        tileGrid = new GridMap<GameObject>(levelToLoad.width, levelToLoad.height, levelToLoad.cellSize, Vector3.zero);
        pathfinding = new PathfindingMe(levelToLoad.width, levelToLoad.height);
        tileObject = new GameObject[levelToLoad.width, levelToLoad.height];

        for (int i = 0; i < levelToLoad.width; ++i)
        {
            for (int j = 0; j < levelToLoad.height; ++j)
            {
                bool isNexusOrange = false;
                bool isNexusBlue = false;
                string encodedObject = "";
                string encodedPrefab;
                string encodedRotation;
                int startingIndex = (i * (levelToLoad.width * 8)) + (j * 8);
                int endingIndex = startingIndex + 8;

                for (int x = startingIndex; x < endingIndex; ++x)
                {
                    encodedObject = encodedObject + levelToLoad.encodedLevel[x];
                }

                encodedPrefab = encodedObject[0].ToString() + encodedObject[1].ToString() + encodedObject[2].ToString();
                encodedRotation = encodedObject[4].ToString() + encodedObject[5].ToString() + encodedObject[6].ToString();

                switch (encodedPrefab)
                {
                    case "W01":
                        tileGrid.SetGridObject(i, j, levelToLoad.W01);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W02":
                        tileGrid.SetGridObject(i, j, levelToLoad.W02);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W03":
                        tileGrid.SetGridObject(i, j, levelToLoad.W03);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W04":
                        tileGrid.SetGridObject(i, j, levelToLoad.W04);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W05":
                        tileGrid.SetGridObject(i, j, levelToLoad.W05);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W06":
                        tileGrid.SetGridObject(i, j, levelToLoad.W06);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W07":
                        tileGrid.SetGridObject(i, j, levelToLoad.W07);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W08":
                        tileGrid.SetGridObject(i, j, levelToLoad.W08);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W09":
                        tileGrid.SetGridObject(i, j, levelToLoad.W09);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W10":
                        tileGrid.SetGridObject(i, j, levelToLoad.W10);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W11":
                        tileGrid.SetGridObject(i, j, levelToLoad.W11);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W12":
                        tileGrid.SetGridObject(i, j, levelToLoad.W12);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W13":
                        tileGrid.SetGridObject(i, j, levelToLoad.W13);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W14":
                        tileGrid.SetGridObject(i, j, levelToLoad.W14);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "W15":
                        tileGrid.SetGridObject(i, j, levelToLoad.W15);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(true);
                        break;

                    case "U01":
                        tileGrid.SetGridObject(i, j, levelToLoad.U01);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U02":
                        tileGrid.SetGridObject(i, j, levelToLoad.U02);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U03":
                        tileGrid.SetGridObject(i, j, levelToLoad.U03);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U04":
                        tileGrid.SetGridObject(i, j, levelToLoad.U04);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U05":
                        tileGrid.SetGridObject(i, j, levelToLoad.U05);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U06":
                        tileGrid.SetGridObject(i, j, levelToLoad.U06);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U07":
                        tileGrid.SetGridObject(i, j, levelToLoad.U07);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U08":
                        tileGrid.SetGridObject(i, j, levelToLoad.U08);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U09":
                        tileGrid.SetGridObject(i, j, levelToLoad.U09);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U10":
                        tileGrid.SetGridObject(i, j, levelToLoad.U10);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        break;

                    case "U98":
                        tileGrid.SetGridObject(i, j, levelToLoad.U98);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        isNexusOrange = true;
                        break;

                    case "U99":
                        tileGrid.SetGridObject(i, j, levelToLoad.U99);
                        pathfinding.GetGrid().GetGridObject(i, j).SetIsWalkable(false);
                        isNexusBlue = true;
                        break;
                }

                switch (encodedRotation)
                {
                    case "000":
                        tileGrid.SetGridRotation(i, j, Quaternion.Euler(0, 0, 0));
                        break;

                    case "090":
                        tileGrid.SetGridRotation(i, j, Quaternion.Euler(0, 90, 0));
                        break;

                    case "180":
                        tileGrid.SetGridRotation(i, j, Quaternion.Euler(0, 180, 0));
                        break;

                    case "270":
                        tileGrid.SetGridRotation(i, j, Quaternion.Euler(0, 270, 0));
                        break;
                }

                tileObject[i, j] = Instantiate(tileGrid.gridObject[i, j], new Vector3(i, 0, j), tileGrid.gridRotation[i, j]);
                if (isNexusOrange)
                {
                    pathfinding.GetGrid().GetGridObject(i, j).SetIsNexus(true, UnitGridSystem.Team.Orange, tileObject[i, j]);
                    isNexusOrange = false;
                }
                else if (isNexusBlue)
                {
                    pathfinding.GetGrid().GetGridObject(i, j).SetIsNexus(true, UnitGridSystem.Team.Blue, tileObject[i, j]);
                    isNexusBlue = false;
                }
            }
        }
    }

    public void ResetTileOutline()
    {
        for (int i = 0; i < pathfinding.GetGrid().GetWidth(); ++i)
        {
            for (int j = 0; j < pathfinding.GetGrid().GetHeight(); ++j)
            {
                pathfinding.GetNode(i, j).SetIsValidMovePosition(false);
                for (int k = 0; k < tileObject[i, j].transform.childCount; ++k)
                {
                    //tileObject[i, j].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.transparentWhite;
                    //playerController.selectedTileColor = playerController.transparentWhite;
                }
            }
        }

        if (playingUnit != null)
        {
            PathNodeMe playingUnitNode = pathfinding.GetNode((int)Mathf.Round(playingUnit.transform.position.x), (int)Mathf.Round(playingUnit.transform.position.z));
            for (int k = 0; k < tileObject[playingUnitNode.x, playingUnitNode.y].transform.childCount; ++k)
            {
                tileObject[playingUnitNode.x, playingUnitNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.opaqueWhite;
            }
        }
    }

    IEnumerator Spawn()
    {
        playerController.cursorRectTransform.gameObject.GetComponent<Image>().color = uiSystem.darkOrange;
        StartCoroutine(SpawnPlayers(playerOneList, team));

        for (; ; )
        {
            if (spawnOne)
            {
                break;
            }
            yield return null;
        }

        team = Team.Blue;
        playerController.gamepad = Gamepad.all[1];
        playerController.playerControls.devices = new[] { playerController.gamepad };
        cameraManager.team = CameraManager.Team.Blue;
        cameraManager.SetCamera();
        playerController.ChangeCursor(playerController.cursorAim);
        playerController.cursorRectTransform.gameObject.GetComponent<Image>().color = uiSystem.darkBlue;

        for (int i = 0; i < uiSystem.currentMode.transform.childCount; ++i)
        {
            uiSystem.currentMode.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }

        StartCoroutine(SpawnPlayers(playerTwoList, team));

        for (; ; )
        {
            if (spawnTwo)
            {
                break;
            }
            yield return null;
        }

        team = Team.Orange;
        playerController.gamepad = Gamepad.all[0];
        playerController.playerControls.devices = new[] { playerController.gamepad };
        cameraManager.team = CameraManager.Team.Orange;
        cameraManager.SetCamera();
        playerController.ChangeCursor(playerController.cursorAim);
        playerController.cursorRectTransform.gameObject.GetComponent<Image>().color = uiSystem.darkOrange;
        SetEnergeticTower(UnitGridSystem.Team.Orange);
        SetEnergeticTower(UnitGridSystem.Team.Blue);
        playingUnit = unitListPlayerOne[0];
        playingUnitSystem = playingUnit.GetComponent<UnitGridSystem>();

        SetUIActive(true);
        uiObject.GetComponent<UISystem>().playingUnit = playingUnit;
        uiObject.GetComponent<UISystem>().playingUnitSystem = playingUnitSystem;

        //playingUnitSystem.playerController = playerController;
        playingUnitSystem.GetComponent<UnitGridSystem>().turn = UnitGridSystem.Turn.Playing;
        playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);

        playerController.playingUnit = playingUnit;
        playerController.playingUnitSystem = playingUnitSystem;

        ResetTileOutline();
        for (int i = 0; i < 4; ++i)
        {
            Destroy(playerOneList[i]);
            Destroy(playerTwoList[i]);
        }
        uiSystem.DisplayNexusUI(false, null);
        uiSystem.PopUp(false);
    }

    IEnumerator SpawnPlayers(List<GameObject> unitList, Team team)
    {
        for (int x = 0; x < unitList.Count; ++x)
        {
            ResetTileOutline();

            switch (team)
            {
                case Team.Orange:
                    for (int i = 0; i < 2; ++i)
                    {
                        for (int j = 0; j < tileGrid.GetHeight(); ++j)
                        {
                            if (pathfinding.GetNode(i, j).isWalkable)
                            {
                                if (!pathfinding.GetNode(i, j).isNexus)
                                {
                                    if (!pathfinding.GetNode(i, j).isContainingUnit)
                                    {
                                        pathfinding.GetNode(i, j).SetIsValidMovePosition(true);
                                        for (int k = 0; k < tileObject[i, j].transform.childCount; ++k)
                                        {
                                            tileObject[i, j].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                case Team.Blue:
                    for (int i = tileGrid.GetWidth() - 2; i < tileGrid.GetWidth(); ++i)
                    {
                        for (int j = 0; j < tileGrid.GetHeight(); ++j)
                        {
                            if (pathfinding.GetNode(i, j).isWalkable)
                            {
                                if (!pathfinding.GetNode(i, j).isContainingUnit)
                                {
                                    pathfinding.GetNode(i, j).SetIsValidMovePosition(true);
                                    for (int k = 0; k < tileObject[i, j].transform.childCount; ++k)
                                    {
                                        tileObject[i, j].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            bool spawned = false;
            for (;;)
            {
                if (playerController.GetMouseWorldPosition() != new Vector3(-1, -1, -1))
                {
                    if (/*playerController.playerControls.Player.Validate.triggered && pathfinding.GetNode((int)playerController.GetMouseWorldPosition().x, (int)playerController.GetMouseWorldPosition().z).isValidMovePosition*/isActiveAndEnabled)
                    {
                        GameObject newUnit = null;
                        
                        switch (team)
                        {
                            case Team.Orange:
                                newUnit = Instantiate(playerOneList[x], new Vector3(0, 1.40f, 0), Quaternion.identity);
                                pathfinding.GetNode((int)playerController.GetMouseWorldPosition().x, (int)playerController.GetMouseWorldPosition().z).SetIsContainingUnit(true, newUnit);
                                newUnit.transform.position = new Vector3((int)playerController.GetMouseWorldPosition().x, newUnit.transform.position.y, (int)playerController.GetMouseWorldPosition().z);
                                InitialiazeUnit(newUnit);
                                unitListPlayerOne.Add(newUnit);
                                break;

                            case Team.Blue:
                                newUnit = Instantiate(playerTwoList[x], new Vector3(0, 1.40f, 0), Quaternion.identity);
                                pathfinding.GetNode((int)playerController.GetMouseWorldPosition().x, (int)playerController.GetMouseWorldPosition().z).SetIsContainingUnit(true, newUnit);
                                newUnit.transform.position = new Vector3((int)playerController.GetMouseWorldPosition().x, newUnit.transform.position.y, (int)playerController.GetMouseWorldPosition().z);
                                InitialiazeUnit(newUnit);
                                unitListPlayerTwo.Add(newUnit);
                                break;
                        }
                        yield return new WaitForSecondsRealtime(0.15f);
                        spawned = true;
                    }
                }

                if (spawned)
                {
                    break;
                }

                yield return null;
            }
        }

        ResetTileOutline();

        switch (team)
        {
            case Team.Orange:
                spawnOne = true;
                break;

            case Team.Blue:
                spawnTwo = true;
                break;
        }
    }

    private void InitialiazeUnit(GameObject unit)
    {
        unit.GetComponent<UnitGridSystem>().ui = uiObject;
        unit.GetComponent<UnitGridSystem>().uiSystem = uiObject.GetComponent<UISystem>();
        unit.GetComponent<UnitGridSystem>().gridSystem = this;
        //unit.GetComponent<UnitGridSystem>().playerController = playerController;
        unit.GetComponent<UnitGridSystem>().GetListOfAbilities();
        unit.GetComponent<UnitMovePathfinding>().isInitialized = true;
    }

    public void SetEnergeticTower(UnitGridSystem.Team team)
    {
        switch (team)
        {
            case UnitGridSystem.Team.Orange:
                for (int i = 0; i < unitListPlayerOne.Count; ++i)
                {
                    unitListPlayerOne[i].GetComponent<UnitGridSystem>().SetEnergeticTower();
                }
                break;

            case UnitGridSystem.Team.Blue:
                for (int i = 0; i < unitListPlayerTwo.Count; ++i)
                {
                    unitListPlayerTwo[i].GetComponent<UnitGridSystem>().SetEnergeticTower();
                }
                break;
        }
    }

    public void ChangePlayingUnit(List<GameObject> unitList, float buttonValue)
    {
        int index = GetIndexOfObjectInList(playingUnit, unitList);
        GameObject nextPlayingUnit = null;
        switch (buttonValue)
        {
            case -1:
                if (index == 0)
                {
                    nextPlayingUnit = unitList[unitList.Count - 1];
                }
                else if (index > 0)
                {
                    nextPlayingUnit = unitList[index - 1];
                }
                break;

            case 1:
                if (index >= 0 && index + 1 <= unitList.Count - 1)
                {
                    nextPlayingUnit = unitList[index + 1];
                }
                else if (index == unitList.Count - 1)
                {
                    nextPlayingUnit = unitList[0];
                }
                break;
        }

        if (nextPlayingUnit != null)
        {
            playingUnitSystem.turn = UnitGridSystem.Turn.Waiting;
            playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
            InitialiazePlayingUnit(nextPlayingUnit);
            playingUnitSystem.turn = UnitGridSystem.Turn.Playing;
            playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
            cameraManager.SetCamera(playingUnit.transform.position);        
        }
        else
        {
            Debug.Log("NEXT UNIT NOT FOUND");
        }

        ResetTileOutline();
    }

    public void InitialiazePlayingUnit(GameObject newPlayingUnit)
    {
        UnitGridSystem newPlayingUnitSystem = newPlayingUnit.GetComponent<UnitGridSystem>();
        this.playingUnit = newPlayingUnit;
        this.playingUnitSystem = newPlayingUnitSystem;
        uiSystem.playingUnit = newPlayingUnit;
        uiSystem.playingUnitSystem = newPlayingUnitSystem;
        playerController.playingUnit = newPlayingUnit;
        playerController.playingUnitSystem = newPlayingUnitSystem;
    }

    public void SetUIActive(bool active)
    {
        for (int i = 0; i < uiObject.transform.childCount - 2; ++i)
        {
            uiObject.transform.GetChild(i).gameObject.SetActive(active);
        }
    }

    public int GetIndexOfObjectInList(GameObject gameObject, List<GameObject> list)
    {
        int index = -1;

        if(list.Count > 0)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (gameObject == list[i])
                {
                    index = i;
                    break;
                }
            }
        }
        
        return index;
    }
}