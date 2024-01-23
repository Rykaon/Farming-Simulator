using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using Assets.Scripts;
using TMPro;

public class UISystem : MonoBehaviour
{
    public GridSystem gridSystem;
    public CameraManager cameraManager;
    public GameObject playingUnit;
    public GameObject targetUnit;
    public UnitGridSystem playingUnitSystem;
    public PlayerController_MAD playerController;
    public ActionData overheat;
    private Pathfinding pathfinding;

    [SerializeField] public Color darkBlue;
    [SerializeField] public Color lightBlue;
    [SerializeField] public Color darkOrange;
    [SerializeField] public Color lightOrange;

    [SerializeField] GameObject UILines;

    public GameObject movePoints;
    public GameObject[] moveAbilities;
    public GameObject actionPoints;
    public GameObject[] actionAbilities;

    public GameObject selectedAbility;
    public MoveData selectedMoveData;
    public ActionData selectedActionData;
    public MoveAbility selectedMoveAbility;
    public ActionAbility selectedActionAbility;

    public GameObject information;
    public GameObject[] implants;
    public GameObject[] availableImplants;
    public GameObject otherImplant;
    public List<GameObject> uiImplants;
    public bool isContainingUnit;
    public bool isContainingImplant;
    public string otherImplantTag;

    public GameObject selectedImplantInfoText;
    public GameObject selectedImplantUI;
    public GameObject selectedImplant;

    public GameObject selectedInfo;

    public GameObject nexusUI;

    public Color darkColor;
    public Color lightColor;
    public Color oppositeDarkColor;
    public Color oppositeLightColor;

    public GameObject defaultMode;
    public GameObject worldMode;
    public GameObject otherMode;
    public GameObject nexusMode;
    public GameObject selfMode;
    public GameObject abilityWorldMode;
    public GameObject abilityUIMode;
    public GameObject currentMode;

    public GameObject changeTurn;
    public GameObject changeTurnYes;
    public GameObject changeTurnNo;
    public GameObject selectedPopUp;

    void Start()
    {
        gridSystem = Camera.main.GetComponent<GridSystem>();
        cameraManager = Camera.main.GetComponent<CameraManager>();
        playerController = Camera.main.GetComponent<PlayerController_MAD>();
        pathfinding = Pathfinding.instance;

        CloseUIMode(moveAbilities);
        CloseUIMode(actionAbilities);
        SetModeToHitUI(false, false, null, false, null);
        DisplayNexusUI(false, null);

        darkColor = darkOrange;
        lightColor = lightOrange;
        UILines.GetComponent<Image>().color = lightColor;
    }

    void Update()
    {
        if (gridSystem.spawnOne && gridSystem.spawnTwo)
        {
            DisplayUI();

            if (playingUnitSystem.state == UnitGridSystem.State.PopUp || playingUnitSystem.state == UnitGridSystem.State.OtherInfo || playingUnitSystem.state == UnitGridSystem.State.SelfInfo || playingUnitSystem.state == UnitGridSystem.State.Repair || playingUnitSystem.state == UnitGridSystem.State.MoveUI || playingUnitSystem.state == UnitGridSystem.State.ActionUI || playingUnitSystem.state == UnitGridSystem.State.HitUI)
            {
                SelectUIWithCursor();
            }
        }
    }

    public void PopUp(bool value)
    {
        if (value)
        {
            changeTurn.SetActive(true);
            changeTurn.GetComponent<Image>().color = lightColor;
            changeTurnYes.GetComponent<Image>().color = darkColor;
            changeTurnNo.GetComponent<Image>().color = lightColor;
            selectedPopUp = changeTurnYes;
        }
        else
        {
            changeTurn.SetActive(false);
        }
    }

    public void SetSelectedPopUp(GameObject popUp)
    {
        selectedPopUp.GetComponent<Image>().color = lightColor;
        selectedPopUp = popUp;
        selectedPopUp.GetComponent<Image>().color = darkColor;
    }

    public void ChangeSelectedPopUp()
    {
        if (selectedPopUp == changeTurnYes)
        {
            SetSelectedPopUp(changeTurnNo);
        }
        else if (selectedPopUp == changeTurnNo)
        {
            SetSelectedPopUp(changeTurnYes);
        }
    }

    public void ChangePlayer(GridSystem.Team previousPlayer)
    {
        for (int i = 0; i < gridSystem.unitListPlayerOne.Count; ++i)
        {
            PathNode unitNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(gridSystem.unitListPlayerOne[i].transform.position.x), (int)Mathf.Round(gridSystem.unitListPlayerOne[i].transform.position.z));
            /*if (unitNode.isFire)
            {
                for (int j = 0; j < gridSystem.unitListPlayerOne[i].transform.childCount; ++j)
                {
                    gridSystem.unitListPlayerOne[i].transform.GetChild(j).GetComponent<ImplantSystem>().InflictDamage(gridSystem.unitListPlayerOne[i].GetComponent<UnitMovePathfinding>().FireDoTDamage);
                }
            }*/
        }

        for (int i = 0; i < gridSystem.unitListPlayerTwo.Count; ++i)
        {
            PathNode unitNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(gridSystem.unitListPlayerTwo[i].transform.position.x), (int)Mathf.Round(gridSystem.unitListPlayerTwo[i].transform.position.z));
            /*if (unitNode.isFire)
            {
                for (int j = 0; j < gridSystem.unitListPlayerTwo[i].transform.childCount; ++j)
                {
                    gridSystem.unitListPlayerTwo[i].transform.GetChild(j).GetComponent<ImplantSystem>().InflictDamage(gridSystem.unitListPlayerTwo[i].GetComponent<UnitMovePathfinding>().FireDoTDamage);
                }
            }*/
        }

        switch (previousPlayer)
        {
            case GridSystem.Team.Blue:
                darkColor = darkOrange;
                lightColor = lightOrange;
                oppositeDarkColor = darkBlue;
                oppositeLightColor = lightBlue;
                UILines.GetComponent<Image>().color = lightColor;
                //playerController.ChangeCursor(playerController.cursorAim);
                //playerController.cursorRectTransform.gameObject.GetComponent<Image>().color = darkColor;
                playingUnitSystem.turn = UnitGridSystem.Turn.Waiting;

                for (int i = 0; i < gridSystem.unitListPlayerOne.Count; ++i)
                {
                    for (int j = 0; j < gridSystem.unitListPlayerOne[i].transform.childCount; ++j)
                    {
                        if (gridSystem.unitListPlayerOne[i].transform.GetChild(j).TryGetComponent<ActionAbility>(out ActionAbility action))
                        {
                            action.hasBeenUsed = false;
                        }
                    }

                    if (gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().hasSpiderWebToBeRemove)
                    {
                        if (gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().spiderWeb != null)
                        {
                            Destroy(gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().spiderWeb);
                        }
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().hasSpiderWebToBeRemove = false;
                    }

                    if (gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().isOverheat)
                    {
                        Debug.Log(gridSystem.unitListPlayerOne[i].transform.position);
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().movePoints -= overheat.damage;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().actionPoints -= overheat.damage;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().remainingMovePoints = 0;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().remainingActionPoints = 0;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().isOverheat = false;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().isSpiderWeb = false;
                    }
                    else if (gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().isSpiderWeb && !gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().isOverheat)
                    {
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().remainingMovePoints = 0;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().remainingActionPoints = gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().actionPoints;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().isSpiderWeb = false;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().hasSpiderWebToBeRemove = true;
                    }
                    else if (!gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().isOverheat && !gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().isSpiderWeb)
                    {
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().remainingMovePoints = gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().movePoints;
                        gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().remainingActionPoints = gridSystem.unitListPlayerOne[i].GetComponent<UnitGridSystem>().actionPoints;
                    }
                }

                gridSystem.InitialiazePlayingUnit(gridSystem.unitListPlayerOne[0]);
                gridSystem.team = GridSystem.Team.Orange;
                //playerController.gamepad = Gamepad.all[0];
                //playerController.playerControls.devices = new[] { playerController.gamepad };
                cameraManager.team = CameraManager.Team.Orange;
                cameraManager.SetCamera();
                cameraManager.SetCamera(playingUnit.transform.position);
                break;

            case GridSystem.Team.Orange:
                darkColor = darkBlue;
                lightColor = lightBlue;
                oppositeDarkColor = darkOrange;
                oppositeLightColor = lightOrange;
                UILines.GetComponent<Image>().color = lightColor;
                //playerController.ChangeCursor(playerController.cursorAim);
                //playerController.cursorRectTransform.gameObject.GetComponent<Image>().color = darkColor;
                playingUnitSystem.turn = UnitGridSystem.Turn.Waiting;

                for (int i = 0; i < gridSystem.unitListPlayerTwo.Count; ++i)
                {
                    for (int j = 0; j < gridSystem.unitListPlayerTwo[i].transform.childCount; ++j)
                    {
                        if (gridSystem.unitListPlayerTwo[i].transform.GetChild(j).TryGetComponent<ActionAbility>(out ActionAbility action))
                        {
                            action.hasBeenUsed = false;
                        }
                    }

                    if (gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().isOverheat)
                    {
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().movePoints -= overheat.damage;
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().actionPoints -= overheat.damage;
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().remainingMovePoints = 0;
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().remainingActionPoints = 0;
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().isOverheat = false;
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().isSpiderWeb = false;
                    }
                    else if (gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().isSpiderWeb && !gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().isOverheat)
                    {
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().remainingMovePoints = 0;
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().remainingActionPoints = gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().actionPoints;
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().isSpiderWeb = false;
                    }
                    else if (!gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().isOverheat && !gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().isSpiderWeb)
                    {
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().remainingMovePoints = gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().movePoints;
                        gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().remainingActionPoints = gridSystem.unitListPlayerTwo[i].GetComponent<UnitGridSystem>().actionPoints;
                    }
                }

                gridSystem.InitialiazePlayingUnit(gridSystem.unitListPlayerTwo[0]);
                gridSystem.team = GridSystem.Team.Blue;
                //playerController.gamepad = Gamepad.all[1];
                //playerController.playerControls.devices = new[] { playerController.gamepad };
                cameraManager.team = CameraManager.Team.Blue;
                cameraManager.SetCamera();
                cameraManager.SetCamera(playingUnit.transform.position);
                break;
        }

        playingUnitSystem.turn = UnitGridSystem.Turn.Playing;
        playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
        gridSystem.ResetTileOutline();
    }

    private void DisplayUI()
    {
        movePoints.GetComponent<TextMeshProUGUI>().text = playingUnitSystem.remainingMovePoints + "/" + playingUnitSystem.movePoints + " PM";
        actionPoints.GetComponent<TextMeshProUGUI>().text = playingUnitSystem.remainingActionPoints + "/" + playingUnitSystem.actionPoints + " PA";
    }

    public void DisplayNexusUI(bool status, GameObject nexus)
    {
        nexusUI.SetActive(status);
        if (status)
        {
            switch (nexus.GetComponent<NexusSystem>().team)
            {
                case NexusSystem.Team.Blue:
                    nexusUI.GetComponent<Image>().color = lightBlue;
                    nexusUI.transform.GetChild(0).GetComponent<Image>().color = darkBlue;
                    nexusUI.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = darkBlue;
                    nexusUI.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.black;
                    nexusUI.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = nexus.GetComponent<NexusSystem>().lifePoints.ToString();
                    break;

                case NexusSystem.Team.Orange:
                    nexusUI.GetComponent<Image>().color = lightOrange;
                    nexusUI.transform.GetChild(0).GetComponent<Image>().color = darkOrange;
                    nexusUI.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = darkOrange;
                    nexusUI.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.black;
                    nexusUI.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = nexus.GetComponent<NexusSystem>().lifePoints.ToString();
                    break;
            }
        }
    }

    public void OpenUIMoveMode(GameObject[] abilities, List<MoveData> abilitiesList)
    {
        for (int i = 0; i < abilitiesList.Count; ++i)
        {
            abilities[i].SetActive(true);
            abilities[i].GetComponent<Image>().color = lightColor;
            abilities[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
            abilities[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = abilitiesList[i].abilityName;
            abilities[i].transform.GetChild(0).gameObject.SetActive(true);
            abilities[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = abilitiesList[i].stats.text + "\n\n" + abilitiesList[i].description.text;
            abilities[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
            abilities[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void OpenUIActionMode(GameObject[] abilities, List<ActionData> abilitiesList)
    {
        for (int i = 0; i < abilitiesList.Count; ++i)
        {
            abilities[i].SetActive(true);
            abilities[i].GetComponent<Image>().color = lightColor;
            abilities[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
            abilities[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = abilitiesList[i].abilityName;
            abilities[i].transform.GetChild(0).gameObject.SetActive(true);
            if (abilitiesList[i].stats == null)
            {
                abilities[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = abilitiesList[i].description.text;
            }
            else
            {
                abilities[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = abilitiesList[i].stats.text + "\n\n" + abilitiesList[i].description.text;
            }
            abilities[i].transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
            abilities[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public void DisplayUIHitMode(bool status, bool isContainingUnit, GameObject unit, bool isContainingImplant, GameObject otherImplant)
    {
        if (status == false)
        {
            for (int i = 0; i < implants.Length; ++i)
            {
                implants[i].SetActive(status);
            }
            information.transform.GetChild(2).gameObject.SetActive(false);
            information.SetActive(false);

            selectedImplantUI = null;
            selectedImplant = null;
            this.otherImplant = null;
            otherImplantTag = null;
            this.isContainingImplant = false;
            this.isContainingUnit = false;
        }
        else
        {
            uiImplants = new List<GameObject>();
            information.SetActive(true);
            if (isContainingUnit)
            {
                information.transform.GetChild(2).gameObject.SetActive(true);
                this.isContainingUnit = true;
                switch (unit.GetComponent<UnitGridSystem>().team)
                {
                    case UnitGridSystem.Team.Blue:
                        information.GetComponent<Image>().color = lightBlue;
                        information.transform.GetChild(2).gameObject.GetComponent<Image>().color = lightBlue;
                        for (int i = 0; i < implants.Length; ++i)
                        {
                            if (i == implants.Length - 1)
                            {
                                implants[i].SetActive(status);
                                implants[i].GetComponent<Image>().color = lightBlue;
                            }
                            else if (i != implants.Length - 1 && i != implants.Length - 2)
                            {
                                implants[i].SetActive(status);
                                if (unit.GetComponent<UnitGridSystem>().implants[i].GetComponent<ImplantSystem>().lifePoints > 0)
                                {
                                    implants[i].GetComponent<Image>().color = lightBlue;
                                    implants[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = lightBlue;
                                    implants[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.grey;
                                    implants[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = unit.GetComponent<UnitGridSystem>().implants[i].GetComponent<ImplantSystem>().lifePoints.ToString();
                                }
                                else
                                {
                                    implants[i].GetComponent<Image>().color = new Color(lightBlue.r, lightBlue.g, lightBlue.b, 0.5f);
                                    implants[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "0";

                                }

                                uiImplants.Add(implants[i]);
                            }
                        }

                        for (int i = 0; i < implants.Length; ++i)
                        {
                            if (unit.GetComponent<UnitGridSystem>().implants[i].GetComponent<ImplantSystem>().lifePoints > 0)
                            {
                                selectedImplantUI = implants[i];
                                selectedImplant = playingUnitSystem.implants[GetIndexOfObjectInArray(selectedImplantUI, implants)].gameObject;
                                selectedImplantUI.GetComponent<Image>().color = darkBlue;
                                selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkBlue;
                                selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                                break;
                            }
                        }
                        break;

                    case UnitGridSystem.Team.Orange:
                        information.GetComponent<Image>().color = lightOrange;
                        information.transform.GetChild(2).gameObject.GetComponent<Image>().color = lightOrange;
                        for (int i = 0; i < implants.Length; ++i)
                        {
                            implants[i].SetActive(status);
                            if (i == implants.Length - 1)
                            {
                                implants[i].SetActive(status);
                                implants[i].GetComponent<Image>().color = lightOrange;
                            }
                            else if (i != implants.Length - 1 && i != implants.Length - 2)
                            {
                                implants[i].SetActive(status);
                                if (unit.GetComponent<UnitGridSystem>().implants[i].GetComponent<ImplantSystem>().lifePoints > 0)
                                {
                                    implants[i].GetComponent<Image>().color = lightOrange;
                                    implants[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = lightOrange;
                                    implants[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.grey;
                                    implants[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = unit.GetComponent<UnitGridSystem>().implants[i].GetComponent<ImplantSystem>().lifePoints.ToString();
                                }
                                else
                                {
                                    implants[i].GetComponent<Image>().color = new Color(lightOrange.r, lightOrange.g, lightOrange.b, 0.5f);
                                    implants[i].transform.GetChild(0).gameObject.GetComponent<Image>().color = new Color(lightOrange.r, lightOrange.g, lightOrange.b, 0.5f);
                                    implants[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                                    implants[i].transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = "0";
                                }

                                uiImplants.Add(implants[i]);
                            }
                        }

                        for (int i = 0; i < implants.Length; ++i)
                        {
                            if (unit.GetComponent<UnitGridSystem>().implants[i].GetComponent<ImplantSystem>().lifePoints > 0)
                            {
                                selectedImplantUI = implants[i];
                                selectedImplant = playingUnitSystem.implants[GetIndexOfObjectInArray(selectedImplantUI, implants)].gameObject;
                                selectedImplantUI.GetComponent<Image>().color = darkOrange;
                                selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkOrange;
                                selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                                break;
                            }
                        }
                        break;
                }
                SetSelectedImplant();
            }
            else
            {
                this.isContainingUnit = false;
            }

            if (isContainingImplant)
            {
                this.isContainingImplant = true;
                implants[implants.Length - 2].SetActive(true);
                implants[implants.Length - 1].SetActive(true);
                this.otherImplant = otherImplant;
                Debug.Log("OTHER IMPLANT = " + otherImplant.name);
                otherImplantTag = otherImplant.tag;
                if (unit != null)
                {
                    switch (unit.GetComponent<UnitGridSystem>().team)
                    {
                        case UnitGridSystem.Team.Blue:
                            implants[implants.Length - 2].GetComponent<Image>().color = lightBlue;
                            implants[implants.Length - 1].GetComponent<Image>().color = darkBlue;
                            break;

                        case UnitGridSystem.Team.Orange:
                            implants[implants.Length - 2].GetComponent<Image>().color = lightOrange;
                            implants[implants.Length - 1].GetComponent<Image>().color = darkOrange;
                            break;
                    }
                }
                else
                {
                    implants[implants.Length - 2].GetComponent<Image>().color = lightColor;
                    implants[implants.Length - 1].GetComponent<Image>().color = darkColor;
                }

                uiImplants.Add(implants[implants.Length - 2]);
                
                if (!isContainingUnit)
                {
                    selectedImplantUI = implants[implants.Length - 2];
                    selectedImplant = otherImplant;
                    selectedImplantUI.GetComponent<Image>().color = darkColor;
                    SetSelectedImplant();
                }
            }
            else
            {
                this.isContainingImplant = false;
                implants[implants.Length - 2].SetActive(false);
            }
        }
    }

    public void CloseUIMode(GameObject[] abilities)
    {
        foreach (GameObject ability in abilities)
        {
            ability.transform.GetChild(0).gameObject.SetActive(false);
            ability.SetActive(false);
        }
    }

    public void SetModeToMoveUI(List<MoveData> abilitiesList, GameObject previousSelectedAbility)
    {
        CloseUIMode(actionAbilities);
        OpenUIMoveMode(moveAbilities, abilitiesList);

        if (previousSelectedAbility == null)
        {
            selectedAbility = moveAbilities[0];
            selectedAbility.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(0).gameObject.SetActive(true);
            selectedAbility.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = abilitiesList[0].stats.text + "\n\n" + abilitiesList[0].description.text;
            selectedAbility.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
            selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
        }
        else
        {
            selectedAbility = previousSelectedAbility;
            selectedAbility.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(0).gameObject.SetActive(true);
            selectedAbility.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
        }
        SetSelectedMoveAbility();
    }

    public void SetModeToActionUI(List<ActionData> abilitiesList, GameObject previousSelectedAbility)
    {
        CloseUIMode(moveAbilities);
        OpenUIActionMode(actionAbilities, abilitiesList);

        if (previousSelectedAbility == null)
        {
            selectedAbility = actionAbilities[0];
            selectedAbility.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(0).gameObject.SetActive(true);
            selectedAbility.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkColor;
            if (abilitiesList[0].stats == null)
            {
                selectedAbility.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = abilitiesList[0].description.text;
            }
            else
            {
                selectedAbility.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = abilitiesList[0].stats.text + "\n\n" + abilitiesList[0].description.text;
            }
            selectedAbility.transform.GetChild(0).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
            selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
        }
        else
        {
            selectedAbility = previousSelectedAbility;
            selectedAbility.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(0).gameObject.SetActive(true);
            selectedAbility.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
        }
        SetSelectedActionAbility();
    }

    public void SetModeToHitUI(bool status, bool isContainingUnit, GameObject unit, bool isContainingImplant, GameObject otherImplant)
    {
        targetUnit = unit;
        DisplayUIHitMode(status, isContainingUnit, targetUnit, isContainingImplant, otherImplant);
    }

    public void ChangeSelectedMoveAbility(GameObject[] array, List<MoveData> abilitiesList, float buttonValue)
    {
        int maxIndex = abilitiesList.Count;
        ChangeSelectedAbility(array, maxIndex, buttonValue);
        SetSelectedMoveAbility();
        playingUnitSystem.HoverMoveAbility(selectedMoveData);
    }

    public void ChangeSelectedActionAbility(GameObject[] array, List<ActionData> abilitiesList, float buttonValue)
    {
        int maxIndex = abilitiesList.Count;
        ChangeSelectedAbility(array, maxIndex, buttonValue);
        SetSelectedActionAbility();
        playingUnitSystem.HoverActionAbility(selectedActionData);
    }

    public void ChangeSelectedAbility(GameObject[] array, int maxIndex, float buttonValue)
    {
        int index = GetIndexOfObjectInArray(selectedAbility, array); ;
        GameObject nextSelectedAbility = null;

        switch (buttonValue)
        {
            case -1:
                if (index == 0)
                {
                    nextSelectedAbility = array[maxIndex - 1];
                }
                else if (index > 0)
                {
                    nextSelectedAbility = array[index - 1];
                }
                break;

            case 1:
                if (index >= 0 && index + 1 <= maxIndex -1)
                {
                    nextSelectedAbility = array[index + 1];
                }
                else if (index == maxIndex - 1)
                {
                    nextSelectedAbility = array[0];
                }
                break;
        }

        if (nextSelectedAbility != null)
        {
            selectedAbility.transform.GetChild(0).gameObject.SetActive(false);
            selectedAbility.GetComponent<Image>().color = lightColor;
            selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
            selectedAbility = nextSelectedAbility;
            selectedAbility.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(0).gameObject.SetActive(true);
            selectedAbility.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkColor;
            selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
        }
        else
        {
            Debug.Log("NEXT ABILITY NOT FOUND");
        }
    }

    public void ChangeSelectedImplant(GameObject[] unitImplants, float buttonValue)
    {
        ChangeSelectedImplant(this.uiImplants, this.implants, unitImplants, buttonValue);
        SetSelectedImplant();
    }

    public void ChangeSelectedImplant(List<GameObject> implantsList, GameObject[] uiImplants, GameObject[] unitImplants, float buttonValue)
    {
        int maxIndexList = implantsList.Count;
        int selectedImplantIndexList = GetIndexOfObjectInList(selectedImplantUI, implantsList);
        GameObject nextSelectedImplant = null;

        switch (buttonValue)
        {
            case -1:
                if (selectedImplantIndexList == 0)
                {
                    for (int i = 0; i < implantsList.Count; ++i)
                    {
                        if (isContainingImplant && implantsList[maxIndexList - (1 + i)] == implantsList[maxIndexList - 1])
                        {
                            nextSelectedImplant = implantsList[maxIndexList - 1];
                            break;
                        }
                        else if (isContainingUnit && unitImplants[GetIndexOfObjectInArray(implantsList[maxIndexList - (1 + i)], implants)].GetComponent<ImplantSystem>().lifePoints > 0)
                        {
                            nextSelectedImplant = implantsList[maxIndexList - (1 + i)];
                            break;
                        }
                    }
                }
                else if (selectedImplantIndexList > 0)
                {
                    for (int i = 0; i < implantsList.Count; ++i)
                    {
                        bool nextSelectedImplantFound = false;

                        if ((selectedImplantIndexList - (1 + i)) < 0)
                        {
                            if (isContainingImplant && implantsList[GetIndexOfObjectInArray(implantsList[implantsList.Count - i], implants)] == implantsList[maxIndexList - 1])
                            {
                                nextSelectedImplant = implantsList[GetIndexOfObjectInArray(implantsList[implantsList.Count - i], implants)];
                                nextSelectedImplantFound = true;
                            }
                            else if (isContainingUnit && unitImplants[GetIndexOfObjectInArray(implantsList[implantsList.Count - i], implants)].GetComponent<ImplantSystem>().lifePoints > 0)
                            {
                                nextSelectedImplant = implantsList[GetIndexOfObjectInArray(implantsList[implantsList.Count - i], implants)];
                                nextSelectedImplantFound = true;
                            }
                        }
                        else
                        {
                            if (isContainingImplant && implantsList[selectedImplantIndexList - (1 + i)] == implantsList[maxIndexList - 1])
                            {
                                nextSelectedImplant = implantsList[maxIndexList - 1];
                                nextSelectedImplantFound = true;
                            }
                            else if (isContainingUnit && unitImplants[GetIndexOfObjectInArray(implantsList[selectedImplantIndexList - (1 + i)], implants)].GetComponent<ImplantSystem>().lifePoints > 0)
                            {
                                nextSelectedImplant = implantsList[selectedImplantIndexList - (1 + i)];
                                nextSelectedImplantFound = true;
                            }
                        }

                        if (nextSelectedImplantFound)
                        {
                            break;
                        }
                    }
                }
                break;

            case 1:
                if (selectedImplantIndexList >= 0 && selectedImplantIndexList < maxIndexList - 1)
                {
                    for (int i = 0; i < implantsList.Count; ++i)
                    {
                        bool nextSelectedImplantFound = false;

                        if ((selectedImplantIndexList + (1 + i)) <= maxIndexList - 1)
                        {
                            if (isContainingImplant && implantsList[selectedImplantIndexList + (1 + i)] == implantsList[maxIndexList - 1])
                            {
                                nextSelectedImplant = implantsList[maxIndexList - 1];
                                nextSelectedImplantFound = true;
                            }
                            else if (isContainingUnit && unitImplants[GetIndexOfObjectInArray(implantsList[selectedImplantIndexList + (1 + i)], implants)].GetComponent<ImplantSystem>().lifePoints > 0)
                            {
                                nextSelectedImplant = implantsList[selectedImplantIndexList + (1 + i)];
                                nextSelectedImplantFound = true;
                            }
                        }
                        else
                        {
                            if (otherImplant != null && implantsList[0 + (i - (selectedImplantIndexList + 1))] == implantsList[maxIndexList - 1])
                            {
                                nextSelectedImplant = implantsList[maxIndexList - 1];
                                nextSelectedImplantFound = true;
                            }
                            else if (isContainingUnit && unitImplants[GetIndexOfObjectInArray(implantsList[0 + (i - (selectedImplantIndexList + 1))], implants)].GetComponent<ImplantSystem>().lifePoints > 0)
                            {
                                nextSelectedImplant = implantsList[0 + (i - (selectedImplantIndexList + 1))];
                                nextSelectedImplantFound = true;
                            }
                        }

                        if (nextSelectedImplantFound)
                        {
                            break;
                        }
                    }
                }
                else if (selectedImplantIndexList == maxIndexList - 1)
                {
                    for (int i = 0; i < implantsList.Count; ++i)
                    {
                        if (isContainingImplant && implantsList[i] == implantsList[maxIndexList - 1])
                        {
                            nextSelectedImplant = implantsList[maxIndexList - 1];
                            break;
                        }
                        else if (isContainingUnit && unitImplants[GetIndexOfObjectInArray(implantsList[i], implants)].GetComponent<ImplantSystem>().lifePoints > 0)
                        {
                            nextSelectedImplant = implantsList[i];
                            break;
                        }
                    }
                }
                break;
        }

        if (nextSelectedImplant != null)
        {
            if (selectedImplantUI == implants[implants.Length - 2])
            {
                selectedImplantUI.GetComponent<Image>().color = lightColor;
                selectedImplantUI = nextSelectedImplant;
            }
            else
            {
                switch (targetUnit.GetComponent<UnitGridSystem>().team)
                {
                    case UnitGridSystem.Team.Blue:
                        selectedImplantUI.GetComponent<Image>().color = lightBlue;
                        selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = lightBlue;
                        selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.grey;
                        selectedImplantUI = nextSelectedImplant;
                        break;

                    case UnitGridSystem.Team.Orange:
                        selectedImplantUI.GetComponent<Image>().color = lightOrange;
                        selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = lightOrange;
                        selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.grey;
                        selectedImplantUI = nextSelectedImplant;
                        break;
                }
            }

            if (selectedImplantUI == implants[implants.Length - 2])
            {
                selectedImplantUI.GetComponent<Image>().color = darkColor;
                selectedImplantInfoText.transform.parent.GetComponent<Image>().color = lightColor;
            }
            else
            {
                switch (targetUnit.GetComponent<UnitGridSystem>().team)
                {
                    case UnitGridSystem.Team.Blue:
                        selectedImplantUI.GetComponent<Image>().color = darkBlue;
                        selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkBlue;
                        selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                        selectedImplantInfoText.transform.parent.GetComponent<Image>().color = lightBlue;
                        break;

                    case UnitGridSystem.Team.Orange:
                        selectedImplantUI.GetComponent<Image>().color = darkOrange;
                        selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkOrange;
                        selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                        selectedImplantInfoText.transform.parent.GetComponent<Image>().color = lightOrange;
                        break;
                }
            }

            SetSelectedImplant();
        }
        else
        {
            Debug.Log("NEXT IMPLANT NOT FOUND");
        }
    }

    private void SelectUIWithCursor()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = playerController.virtualMouse.position.ReadValue();
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        GameObject nextSelectedObject = null; ;

        if (playingUnitSystem.state == UnitGridSystem.State.PopUp)
        {
            if (raycastResults.Count > 0)
            {
                foreach (RaycastResult go in raycastResults)
                {
                    if (go.gameObject != selectedPopUp && go.gameObject.layer != 2)
                    {
                        Debug.Log(go.gameObject.name);
                        nextSelectedObject = go.gameObject;
                    }
                }

                if (nextSelectedObject != null)
                {
                    SetSelectedPopUp(nextSelectedObject);
                }
            }
        }
        else if (playingUnitSystem.state == UnitGridSystem.State.MoveUI)
        {
            if (raycastResults.Count > 0)
            {
                foreach (RaycastResult go in raycastResults)
                {
                    if (go.gameObject != selectedAbility && go.gameObject.layer != 2)
                    {
                        Debug.Log(go.gameObject.name);
                        nextSelectedObject = go.gameObject;
                    }
                }

                if (nextSelectedObject != null)
                {
                    selectedAbility.transform.GetChild(0).gameObject.SetActive(false);
                    selectedAbility.GetComponent<Image>().color = lightColor;
                    selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                    selectedAbility = nextSelectedObject;
                    selectedAbility.GetComponent<Image>().color = darkColor;
                    selectedAbility.transform.GetChild(0).gameObject.SetActive(true);
                    selectedAbility.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkColor;
                    selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                    SetSelectedMoveAbility();
                    playingUnitSystem.HoverMoveAbility(selectedMoveData);
                }
            }
        }
        else if (playingUnitSystem.state == UnitGridSystem.State.ActionUI)
        {
            if (raycastResults.Count > 0)
            {
                foreach (RaycastResult go in raycastResults)
                {
                    if (go.gameObject != selectedAbility && go.gameObject.layer != 2)
                    {
                        nextSelectedObject = go.gameObject;
                    }
                }

                if (nextSelectedObject != null)
                {
                    selectedAbility.transform.GetChild(0).gameObject.SetActive(false);
                    selectedAbility.GetComponent<Image>().color = lightColor;
                    selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                    selectedAbility = nextSelectedObject;
                    selectedAbility.GetComponent<Image>().color = darkColor;
                    selectedAbility.transform.GetChild(0).gameObject.SetActive(true);
                    selectedAbility.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkColor;
                    selectedAbility.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                    SetSelectedActionAbility();
                    playingUnitSystem.HoverActionAbility(selectedActionData);
                }
            }
        }
        else if (playingUnitSystem.state == UnitGridSystem.State.HitUI || playingUnitSystem.state == UnitGridSystem.State.OtherInfo || playingUnitSystem.state == UnitGridSystem.State.SelfInfo)
        {
            if (raycastResults.Count > 0)
            {
                foreach (RaycastResult go in raycastResults)
                {
                    if (go.gameObject != selectedImplantUI && go.gameObject.layer != 2)
                    {
                        if (go.gameObject == otherImplant && otherImplant != null)
                        {
                            nextSelectedObject = go.gameObject;
                        }
                        else if (targetUnit.GetComponent<UnitGridSystem>().implants[GetIndexOfObjectInArray(go.gameObject, implants)].GetComponent<ImplantSystem>().lifePoints > 0)
                        {
                            nextSelectedObject = go.gameObject;
                        }
                    }
                }
                
                if (nextSelectedObject != null)
                {
                    if (selectedImplantUI == implants[implants.Length - 2])
                    {
                        selectedImplantUI.GetComponent<Image>().color = lightColor;
                        selectedImplantUI = nextSelectedObject;
                    }
                    else
                    {
                        switch (targetUnit.GetComponent<UnitGridSystem>().team)
                        {
                            case UnitGridSystem.Team.Blue:
                                selectedImplantUI.GetComponent<Image>().color = lightBlue;
                                selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = lightBlue;
                                selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.grey;
                                selectedImplantUI = nextSelectedObject;
                                break;

                            case UnitGridSystem.Team.Orange:
                                selectedImplantUI.GetComponent<Image>().color = lightOrange;
                                selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = lightOrange;
                                selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.grey;
                                selectedImplantUI = nextSelectedObject;
                                break;
                        }
                    }

                    if (selectedImplantUI == implants[implants.Length - 2])
                    {
                        selectedImplantUI.GetComponent<Image>().color = darkColor;
                        selectedImplantInfoText.transform.parent.GetComponent<Image>().color = darkColor;
                    }
                    else
                    {
                        switch (targetUnit.GetComponent<UnitGridSystem>().team)
                        {
                            case UnitGridSystem.Team.Blue:
                                selectedImplantUI.GetComponent<Image>().color = darkBlue;
                                selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkBlue;
                                selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                                selectedImplantInfoText.transform.parent.GetComponent<Image>().color = lightBlue;
                                break;

                            case UnitGridSystem.Team.Orange:
                                selectedImplantUI.GetComponent<Image>().color = darkOrange;
                                selectedImplantUI.transform.GetChild(0).gameObject.GetComponent<Image>().color = darkOrange;
                                selectedImplantUI.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().color = Color.black;
                                selectedImplantInfoText.transform.parent.GetComponent<Image>().color = lightOrange;
                                break;
                        }
                    }

                    SetSelectedImplant();
                }
            }
        }
    }

    public void SetSelectedMoveAbility()
    {
        if (selectedAbility.transform.IsChildOf(movePoints.transform))
        {
            selectedMoveData = playingUnitSystem.moveDataList[GetIndexOfObjectInArray(selectedAbility, moveAbilities)];
            selectedMoveAbility = playingUnitSystem.moveAbilityList[GetIndexOfObjectInArray(selectedAbility, moveAbilities)];
        }
        else
        {
            Debug.Log("SELECTED_ABILITY IS NOT MOVE_ABILITY");
        }
    }

    public void SetSelectedActionAbility()
    {
        if (selectedAbility.transform.IsChildOf(actionPoints.transform))
        {
            selectedActionData = playingUnitSystem.actionDataList[GetIndexOfObjectInArray(selectedAbility, actionAbilities)];
            selectedActionAbility = playingUnitSystem.actionAbilityList[GetIndexOfObjectInArray(selectedAbility, actionAbilities)];
        }
        else
        {
            Debug.Log("SELECTED_ABILITY IS NOT ACTION_ABILITY");
        }
    }

    public void SetSelectedImplant()
    {
        int implantIndexArray = GetIndexOfObjectInArray(selectedImplantUI, implants);
        int implantIndexList = GetIndexOfObjectInList(selectedImplantUI, uiImplants);
        string implantText = "Liste des capacités de l'implant : \n\n";
        if (otherImplant != null)
        {
            if (playingUnitSystem.state == UnitGridSystem.State.Repair)
            {
                selectedImplant = targetUnit.GetComponent<UnitGridSystem>().implants[implantIndexArray].gameObject;
                selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;

                if (selectedImplant.transform.TryGetComponent<MoveAbility>(out MoveAbility move) && move.moveAbility != null)
                {
                    if (move.moveAbility != null)
                    {
                        implantText = implantText + move.moveAbility.abilityName + "\n\n" + move.moveAbility.stats.text + "\n\n" + move.moveAbility.description.text;
                        selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                        selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                    }
                    else
                    {
                        selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                        selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                    }
                }

                if (selectedImplant.transform.TryGetComponent<ActionAbility>(out ActionAbility action))
                {
                    if (action.actionAbility != null)
                    {
                        if (action.actionAbility.stats == null)
                        {
                            implantText = implantText + action.actionAbility.abilityName + "\n\n" + action.actionAbility.description.text;
                        }
                        else
                        {
                            implantText = implantText + action.actionAbility.abilityName + "\n\n" + action.actionAbility.stats.text + "\n\n" + action.actionAbility.description.text;
                        }
                        selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                        selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                    }
                    else
                    {
                        selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                        selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                    }
                }
            }
            else
            {
                if (implantIndexList < uiImplants.Count - 1)
                {
                    selectedImplant = targetUnit.GetComponent<UnitGridSystem>().implants[implantIndexArray].gameObject;
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;

                    if (selectedImplant.transform.TryGetComponent<MoveAbility>(out MoveAbility move) && move.moveAbility != null)
                    {
                        if (move.moveAbility != null)
                        {
                            implantText = implantText + move.moveAbility.abilityName + "\n\n" + move.moveAbility.stats.text + "\n\n" + move.moveAbility.description.text;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                        }
                        else
                        {
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                        }
                    }

                    if (selectedImplant.transform.TryGetComponent<ActionAbility>(out ActionAbility action))
                    {
                        if (action.actionAbility != null)
                        {
                            if (action.actionAbility.stats == null)
                            {
                                implantText = implantText + action.actionAbility.abilityName + "\n\n" + action.actionAbility.description.text;
                            }
                            else
                            {
                                implantText = implantText + action.actionAbility.abilityName + "\n\n" + action.actionAbility.stats.text + "\n\n" + action.actionAbility.description.text;
                            }
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                        }
                        else
                        {
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                        }
                    }
                }
                else if (implantIndexList == uiImplants.Count - 1)
                {
                    selectedImplant = targetUnit.GetComponent<UnitGridSystem>().implants[implantIndexArray].gameObject;
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;

                    if (selectedImplant.transform.TryGetComponent<MoveAbility>(out MoveAbility move) && move.moveAbility != null)
                    {
                        if (move.moveAbility != null)
                        {
                            implantText = implantText + move.moveAbility.abilityName + "\n\n" + move.moveAbility.stats.text + "\n\n" + move.moveAbility.description.text;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                        }
                        else
                        {
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                        }
                    }

                    if (selectedImplant.transform.TryGetComponent<ActionAbility>(out ActionAbility action))
                    {
                        if (action.actionAbility != null)
                        {
                            if (action.actionAbility.stats == null)
                            {
                                implantText = implantText + action.actionAbility.abilityName + "\n\n" + action.actionAbility.description.text;
                            }
                            else
                            {
                                implantText = implantText + action.actionAbility.abilityName + "\n\n" + action.actionAbility.stats.text + "\n\n" + action.actionAbility.description.text;
                            }
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                        }
                        else
                        {
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                        }
                    }
                }
            }
        }
        else
        {
            selectedImplant = targetUnit.GetComponent<UnitGridSystem>().implants[implantIndexArray].gameObject;
            selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;

            if (selectedImplant.transform.TryGetComponent<MoveAbility>(out MoveAbility move) && move.moveAbility != null)
            {
                if (move.moveAbility != null)
                {
                    implantText = implantText + move.moveAbility.abilityName + "\n\n" + move.moveAbility.stats.text + "\n\n" + move.moveAbility.description.text;
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                }
                else
                {
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                }
            }

            if (selectedImplant.transform.TryGetComponent<ActionAbility>(out ActionAbility action))
            {
                if (action.actionAbility != null)
                {
                    if (action.actionAbility.stats == null)
                    {
                        implantText = implantText + action.actionAbility.abilityName + "\n\n" + action.actionAbility.description.text;
                    }
                    else
                    {
                        implantText = implantText + action.actionAbility.abilityName + "\n\n" + action.actionAbility.stats.text + "\n\n" + action.actionAbility.description.text;
                    }
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                }
                else
                {
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().text = implantText;
                    selectedImplantInfoText.GetComponent<TextMeshProUGUI>().color = Color.black;
                }
            }
        }
    }

    public int GetIndexOfObjectInArray(GameObject gameObject, GameObject[] array)
    {
        int index = -1;

        if (array.Length > 0)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                if (gameObject == array[i])
                {
                    index = i;
                    break;
                }
            }
        }

        return index;
    }

    public int GetIndexOfObjectInList(GameObject gameObject, List<GameObject> list)
    {
        int index = -1;

        if (list.Count > 0)
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