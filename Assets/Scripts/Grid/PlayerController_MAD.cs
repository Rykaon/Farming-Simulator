using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using Assets.Scripts;

public class PlayerController_MAD : MonoBehaviour
{
    CameraManager cameraManager;
    public PlayerControls playerControls;
    GridSystem gridSystem;
    Pathfinding pathfinding;
    public GameObject playingUnit;
    public UnitGridSystem playingUnitSystem;
    [SerializeField] GameObject uiObject;
    UISystem uiSystem;

    public GameObject selectedTile;
    public Color selectedTileColor;
    public Color transparentWhite;
    public Color opaqueWhite;
    public Color red;
    public Color darkOrange;
    public Color lightOrange;
    public Color darkBlue;
    public Color lightBlue;
    public bool isUnhover = false;

    public Sprite cursorAim;
    public Sprite cursorAimPressed;
    public Sprite cursorArrow;
    public Sprite cursorArrowPressed;
    public Sprite cursor;
    public Sprite cursorPressed;

    public Mouse virtualMouse;
    public Mouse currentMouse;
    [SerializeField] PlayerInput playerInput;
    public RectTransform cursorRectTransform;
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform canvasRectTransform;
    [SerializeField] Camera mainCamera;
    [SerializeField] float cameraSpeed;
    [SerializeField] float cursorSpeed;
    [SerializeField] float padding;
    private bool previousMouseState;
    public string previousControlScheme = "";
    private const string gamepadScheme = "Gamepad";
    private const string mouseScheme = "Keyboard&Mouse";

    public bool isStickPress = false;

    public PathNode targetPathNode;

    public Gamepad gamepad;

    private void Awake()
    {
        cameraManager = transform.GetComponent<CameraManager>();
        gridSystem = transform.GetComponent<GridSystem>();
        uiSystem = uiObject.GetComponent<UISystem>();
        playerControls = new PlayerControls();

        switch (gridSystem.team)
        {
            case GridSystem.Team.Orange:
                gamepad = Gamepad.all[0];
                break;

            case GridSystem.Team.Blue:
                gamepad = Gamepad.all[1];
                break;
        }

        playerControls.devices = new[] { gamepad };

        ChangeCursor(cursorAim);
    }

    private void OnEnable()
    {
        playerControls.Enable();
        /*playerControls.Player.Validate.started += _ => ValidateStarted();
        playerControls.Player.Validate.performed += _ => ValidateEnded();
        playerControls.Player.MovementCamera.performed += CameraMovement;
        playerControls.Player.Zoom.started += _ => StickPressStarted();
        playerControls.Player.Zoom.performed += _ => StickPressEnded();*/

        currentMouse = Mouse.current;

        if (virtualMouse == null)
        {
            virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else if (!virtualMouse.added)
        {
            InputSystem.AddDevice(virtualMouse);
        }

        InputUser.PerformPairingWithDevice(virtualMouse, playerInput.user);

        if (cursorRectTransform != null)
        {
            Vector2 position = cursorRectTransform.anchoredPosition;
            //Vector2 position = new Vector2(0f, 0f);
            InputState.Change(virtualMouse.position, position);
            cursorRectTransform.position = position;
        }

        InputSystem.onAfterUpdate += UpdateCursorPosition;
        InputSystem.onActionChange += OnControlsChanged;
    }

    private void OnDisable()
    {
        if (virtualMouse != null && virtualMouse.added)
        {
            InputSystem.RemoveDevice(virtualMouse);
        }
        InputSystem.onAfterUpdate -= UpdateCursorPosition;
        InputSystem.onActionChange -= OnControlsChanged;
        playerControls.Disable();
    }

    private void CameraMovement(InputAction.CallbackContext context)
    {
        if (!isStickPress)
        {
            Vector3 newPos = Vector3.zero;
            switch (Mathf.Round(transform.rotation.eulerAngles.y))
            {
                case 45:
                    //newPos = cameraManager.position + new Vector3(playerControls.Player.MovementCamera.ReadValue<Vector2>().y, 0f, -playerControls.Player.MovementCamera.ReadValue<Vector2>().x) * cameraSpeed;
                    newPos.x = Mathf.Clamp(newPos.x, 1, 30);
                    newPos.z = Mathf.Clamp(newPos.z, -1, 30);
                    cameraManager.position = newPos;
                    break;

                case 135:
                    //newPos = cameraManager.position + new Vector3(-playerControls.Player.MovementCamera.ReadValue<Vector2>().x, 0f, -playerControls.Player.MovementCamera.ReadValue<Vector2>().y) * cameraSpeed;
                    newPos.x = Mathf.Clamp(newPos.x, 1, 30);
                    newPos.z = Mathf.Clamp(newPos.z, -15, 15);
                    cameraManager.position = newPos;
                    break;

                case 225:
                    //newPos = cameraManager.position + new Vector3(-playerControls.Player.MovementCamera.ReadValue<Vector2>().y, 0f, playerControls.Player.MovementCamera.ReadValue<Vector2>().x) * cameraSpeed;
                    newPos.x = Mathf.Clamp(newPos.x, -15, 15);
                    newPos.z = Mathf.Clamp(newPos.z, -15, 15);
                    cameraManager.position = newPos;
                    break;

                case 315:
                    //newPos = cameraManager.position + new Vector3(playerControls.Player.MovementCamera.ReadValue<Vector2>().x, 0f, playerControls.Player.MovementCamera.ReadValue<Vector2>().y) * cameraSpeed;
                    newPos.x = Mathf.Clamp(newPos.x, -15, 15);
                    newPos.z = Mathf.Clamp(newPos.z, -1, 30);
                    cameraManager.position = newPos;
                    break;
            }
        }
    }

    private void CameraRotation(float value)
    {
        if (cameraManager.isReadyToRotate)
        {
            GameObject holder = new GameObject();
            holder.transform.position = transform.position;
            holder.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles);

            if (value < 0)
            {
                holder.transform.RotateAround(gridSystem.cameraPivotPos, Vector3.up, 90);
            }
            else if (value > 0)
            {
                holder.transform.RotateAround(gridSystem.cameraPivotPos, Vector3.up, -90);
            }

            Vector3 newPos = holder.transform.position;

            switch (Mathf.Round(holder.transform.rotation.eulerAngles.y))
            {
                case 45:
                    newPos.x = Mathf.Clamp(newPos.x, 1, 30);
                    newPos.z = Mathf.Clamp(newPos.z, -1, 30);
                    break;

                case 135:
                    newPos.x = Mathf.Clamp(newPos.x, 1, 30);
                    newPos.z = Mathf.Clamp(newPos.z, -15, 15);
                    break;

                case 225:
                    newPos.x = Mathf.Clamp(newPos.x, -15, 15);
                    newPos.z = Mathf.Clamp(newPos.z, -15, 15);
                    break;

                case 315:
                    newPos.x = Mathf.Clamp(newPos.x, -15, 15);
                    newPos.z = Mathf.Clamp(newPos.z, -1, 30);
                    break;
            }
            cameraManager.position = holder.transform.position;
            cameraManager.rotation = holder.transform.rotation.eulerAngles;
            Destroy(holder);
            cameraManager.isReadyToRotate = false;
        }
    }

    private void UpdateCursorPosition()
    {
        if (virtualMouse == null || gamepad == null)
        {
            return;
        }

        Vector2 deltaValue = gamepad.leftStick.ReadValue();
        deltaValue *= cursorSpeed * Time.deltaTime;
        Vector2 currentPosition = virtualMouse.position.ReadValue();
        Vector2 newPosition = currentPosition + deltaValue;

        newPosition.x = Mathf.Clamp(newPosition.x, padding, Screen.width - padding);
        newPosition.y = Mathf.Clamp(newPosition.y, padding, Screen.height - padding);

        InputState.Change(virtualMouse.position, newPosition);
        InputState.Change(virtualMouse.delta, deltaValue);

        if (previousMouseState != gamepad.aButton.isPressed)
        {
            virtualMouse.CopyState<MouseState>(out var mouseState);
            mouseState.WithButton(MouseButton.Left, Gamepad.current.aButton.IsPressed());
            InputState.Change(virtualMouse, mouseState);
            previousMouseState = Gamepad.current.aButton.IsPressed();
        }

        AnchoredCursor(newPosition);
    }

    private void AnchoredCursor(Vector2 position)
    {
        Vector2 anchoredPositon;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera, out anchoredPositon);
        cursorRectTransform.anchoredPosition = anchoredPositon;
    }

    private void OnControlsChanged(object input, InputActionChange inputActionChange)
    {
        if(inputActionChange == InputActionChange.BoundControlsChanged)
        {
            if (playerInput.currentControlScheme == mouseScheme && previousControlScheme != mouseScheme)
            {
                cursorRectTransform.gameObject.SetActive(false);
                Cursor.visible = false;
                currentMouse.WarpCursorPosition(virtualMouse.position.ReadValue());
                previousControlScheme = mouseScheme;
            }
            else if (playerInput.currentControlScheme == gamepadScheme && previousControlScheme != gamepadScheme)
            {
                cursorRectTransform.gameObject.SetActive(true);
                Cursor.visible = false;
                InputState.Change(virtualMouse.position, currentMouse.position.ReadValue());
                AnchoredCursor(currentMouse.position.ReadValue());
                previousControlScheme = gamepadScheme;
            }
        }
        
    }

    public void ChangeCursor(Sprite sprite)
    {
        if (sprite == cursorAim)
        {
            cursor = cursorAim;
            cursorPressed = cursorAimPressed;
        }
        else if (sprite == cursorArrow)
        {
            cursor = cursorArrow;
            cursorPressed = cursorArrowPressed;
        }

        cursorRectTransform.gameObject.GetComponent<Image>().sprite = cursor;
    }

    private void ValidateStarted()
    {
        cursorRectTransform.gameObject.GetComponent<Image>().sprite = cursorPressed;
    }

    private void ValidateEnded()
    {
        cursorRectTransform.gameObject.GetComponent<Image>().sprite = cursor;
    }

    private void StickPressStarted()
    {
        isStickPress = true;
    }

    private void StickPressEnded()
    {
        isStickPress = false;
    }

    private void HoverSprite()
    {
        if(GetMouseWorldPosition() != new Vector3(-1f, -1f, -1f))
        {
            pathfinding.GetGrid().GetXY(GetMouseWorldPosition(), out int x, out int y);
            if (pathfinding.GetNodeWithCoords(x, y) != null)
            {
                UnhoverSprite(selectedTile);
                if (pathfinding.GetNodeWithCoords(x, y).unit != playingUnit || (!gridSystem.spawnOne || !gridSystem.spawnTwo))
                {
                    selectedTile = gridSystem.tileObject[x, y];
                    selectedTileColor = selectedTile.transform.GetChild(0).GetComponent<Outline>().OutlineColor;
                    if (selectedTileColor == lightOrange)
                    {
                        for (int i = 0; i < selectedTile.transform.childCount; ++i)
                        {
                            selectedTile.transform.GetChild(i).GetComponent<Outline>().OutlineColor = darkOrange;
                        }
                    }
                    else if (selectedTileColor == lightBlue)
                    {
                        for (int i = 0; i < selectedTile.transform.childCount; ++i)
                        {
                            selectedTile.transform.GetChild(i).GetComponent<Outline>().OutlineColor = darkBlue;
                        }
                    }
                    else if (selectedTileColor == transparentWhite)
                    {
                        if (!gridSystem.spawnOne || !gridSystem.spawnTwo || playingUnitSystem.state == UnitGridSystem.State.ActionWorld || playingUnitSystem.state == UnitGridSystem.State.ActionUI || playingUnitSystem.state == UnitGridSystem.State.MoveWorld || playingUnitSystem.state == UnitGridSystem.State.MoveUI)
                        {
                            for (int i = 0; i < selectedTile.transform.childCount; ++i)
                            {
                                selectedTile.transform.GetChild(i).GetComponent<Outline>().OutlineColor = red;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < selectedTile.transform.childCount; ++i)
                            {
                                selectedTile.transform.GetChild(i).GetComponent<Outline>().OutlineColor = opaqueWhite;
                            }
                        }
                    }
                    //gridSystem.tileObject[x, y].transform.GetChild(0).GetComponent<Outline>().OutlineColor = Color.white;
                    isUnhover = false;
                }
            }
        }
        else
        {
            if (!isUnhover)
            {
                UnhoverSprite(selectedTile);
            }
        }
    }

    private void UnhoverSprite(GameObject tile)
    {
        if (tile != null)
        {
            for (int i = 0; i < tile.transform.childCount; ++i)
            {
                tile.transform.GetChild(i).GetComponent<Outline>().OutlineColor = selectedTileColor;
            }
        }

        isUnhover = true;
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPosition = new Vector3(-1f, -1f, -1f);
        switch (previousControlScheme)
        {
            case "Keyboard&Mouse":
                if (RaycastToWorldPosition(currentMouse.position.ReadValue(), out Vector3 currentMousePos) != new Vector3(-1f, -1f, -1f))
                {
                    worldPosition = new Vector3(Mathf.Round(currentMousePos.x), 0, Mathf.Round(currentMousePos.z));
                }
                break;

            case "Gamepad":
                if (RaycastToWorldPosition(virtualMouse.position.ReadValue(), out Vector3 virtualMousePos) != new Vector3(-1f, -1f, -1f))
                {
                    worldPosition = new Vector3(Mathf.Round(virtualMousePos.x), 0, Mathf.Round(virtualMousePos.z));
                }
                break;
        }

        if (worldPosition.x >= 0 && worldPosition.x < Pathfinding.instance.GetGrid().GetWidth() && worldPosition.z >= 0 && worldPosition.z < Pathfinding.instance.GetGrid().GetHeight())
        {
            return worldPosition;
        }
        else
        {
            return new Vector3(-1f, -1f, -1f);
        }
    }

    Vector3 RaycastToWorldPosition(Vector3 position, out Vector3 worldPosition)
    {
        worldPosition = new Vector3(-1f, -1f, -1f);

        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null)
            {
                worldPosition = new Vector3(hit.transform.position.x, 0f, hit.transform.position.z);
            }
        }

        return worldPosition;
    }

    void Start()
    {
        pathfinding = Pathfinding.instance;
        selectedTile = gridSystem.tileObject[0, 0];
    }

    void Update()
    {
        if (isStickPress)
        {
            //cameraManager.Zoom(playerControls.Player.MovementCamera.ReadValue<Vector2>().y);
        }

        /*if (playerControls.Player.RotationCamera.triggered)
        {
            //CameraRotation(playerControls.Player.RotationCamera.ReadValue<float>());
        }*/

        if (gridSystem.spawnOne && gridSystem.spawnTwo)
        {
            if (playingUnitSystem.state == UnitGridSystem.State.WorldMode)
            {
                HoverSprite();

                if (/*playerControls.Player.OtherInfo.triggered*/isStickPress)
                {
                    if (GetMouseWorldPosition() != new Vector3(-1f, -1f, -1f))
                    {
                        PathNode pathNode = pathfinding.GetNodeWithCoords((int)GetMouseWorldPosition().x, (int)GetMouseWorldPosition().z);
                        /*if (pathNode != pathfinding.GetNode((int)playingUnit.transform.position.x, (int)playingUnit.transform.position.z) && (pathNode.isContainingUnit || pathNode.isContainingImplant))
                        {
                            uiSystem.SetModeToHitUI(true, pathNode.isContainingUnit, pathNode.unit, pathNode.isContainingImplant, pathNode.implant);
                            playingUnitSystem.ChangeState(UnitGridSystem.State.OtherInfo);
                            targetPathNode = pathNode;
                        }
                        else if (pathNode != pathfinding.GetNode((int)playingUnit.transform.position.x, (int)playingUnit.transform.position.z) && pathNode.isNexus)
                        {
                            if (pathNode.nexusOrange != null && pathNode.nexusBlue == null)
                            {
                                uiSystem.DisplayNexusUI(true, pathNode.nexusOrange);
                            }
                            else if (pathNode.nexusOrange == null && pathNode.nexusBlue != null)
                            {
                                uiSystem.DisplayNexusUI(true, pathNode.nexusBlue);
                            }
                            playingUnitSystem.ChangeState(UnitGridSystem.State.NexusInfo);
                        }*/
                    }
                    //Debug.Log("OTHER INFO");
                }

                if (/*playerControls.Player.SelfInfo.triggered*/isStickPress)
                {
                    PathNode pathNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(playingUnit.transform.position.x), (int)Mathf.Round(playingUnit.transform.position.z));
                    //uiSystem.SetModeToHitUI(true, pathNode.isContainingUnit, pathNode.unit, pathNode.isContainingImplant, pathNode.implant);
                    playingUnitSystem.ChangeState(UnitGridSystem.State.SelfInfo);
                    targetPathNode = pathNode;
                    //Debug.Log("SELF INFO");
                }

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.PopUp));
                    uiSystem.PopUp(true);
                }

                if (/*playerControls.Player.SwitchUnit.triggered*/isStickPress)
                {
                    switch (gridSystem.team)
                    {
                        case GridSystem.Team.Orange:
                            //gridSystem.ChangePlayingUnit(gridSystem.unitListPlayerOne, playerControls.Player.SwitchUnit.ReadValue<float>());
                            break;

                        case GridSystem.Team.Blue:
                            //gridSystem.ChangePlayingUnit(gridSystem.unitListPlayerTwo, playerControls.Player.SwitchUnit.ReadValue<float>());
                            break;
                    }
                    //Debug.Log("SWITCH UNIT = " + playerControls.Player.SwitchUnit.ReadValue<float>());
                }

                if (/*playerControls.Player.MoveMode.triggered*/isStickPress)
                {
                    playingUnitSystem.ChangeState(UnitGridSystem.State.MoveUI);
                    uiSystem.SetModeToMoveUI(playingUnitSystem.moveDataList, null);
                    uiSystem.SetSelectedMoveAbility();
                    playingUnitSystem.HoverMoveAbility(uiSystem.selectedMoveData);
                    //Debug.Log("ENTER IN MOVE UI STATE");
                }

                if (/*playerControls.Player.ActionMode.triggered*/isStickPress)
                {
                    playingUnitSystem.ChangeState(UnitGridSystem.State.ActionUI);
                    uiSystem.SetModeToActionUI(playingUnitSystem.actionDataList, null);
                    uiSystem.SetSelectedActionAbility();
                    playingUnitSystem.HoverActionAbility(uiSystem.selectedActionData);
                    //Debug.Log("ENTER IN ACTION UI STATE");
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.PopUp)
            {
                if (/*playerControls.Player.Validate.triggered*/isStickPress)
                {
                    uiSystem.PopUp(false);
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.WorldMode));

                    if (uiSystem.selectedPopUp == uiSystem.changeTurnYes)
                    {
                        uiSystem.ChangePlayer(gridSystem.team);
                    }
                    //Debug.Log("NEXUS INFO MODE CANCELED");
                }

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.PopUp(false);
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.WorldMode));
                    //Debug.Log("NEXUS INFO MODE CANCELED");
                }

                if (/*playerControls.Player.VerticalMenu.triggered*/isStickPress)
                {
                    uiSystem.ChangeSelectedPopUp();
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.NexusInfo)
            {
                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.DisplayNexusUI(false, null);
                    playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                    //Debug.Log("NEXUS INFO MODE CANCELED");
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.OtherInfo)
            {
                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.SetModeToHitUI(false, false, null, false, null);
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.WorldMode));
                    targetPathNode = null;
                    //Debug.Log("OTHER INFO MODE CANCELED");
                }

                if (/*playerControls.Player.VerticalMenu.triggered*/isStickPress)
                {
                    if (targetPathNode.unit != null)
                    {
                        //uiSystem.ChangeSelectedImplant(targetPathNode.unit.GetComponent<UnitGridSystem>().implants, playerControls.Player.VerticalMenu.ReadValue<float>());
                    }
                    else
                    {
                        //uiSystem.ChangeSelectedImplant(null, playerControls.Player.VerticalMenu.ReadValue<float>());
                    }
                    
                    //Debug.Log("VERTICAL MENU INPUT = " + playerControls.Player.VerticalMenu.ReadValue<float>());
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.SelfInfo)
            {
                if (/*playerControls.Player.Validate.triggered*/isStickPress)
                {
                    if (uiSystem.isContainingImplant)
                    {
                        uiSystem.SetSelectedImplant();
                        if (uiSystem.selectedImplant == uiSystem.otherImplant) 
                        {
                            uiSystem.uiImplants = new List<GameObject>();
                            for (int i = 0; i < playingUnitSystem.implants.Length; ++i)
                            {
                                uiSystem.implants[i].GetComponent<Image>().color = Color.white;
                                if (playingUnitSystem.implants[i].tag == uiSystem.otherImplantTag)
                                {
                                    uiSystem.uiImplants.Add(uiSystem.implants[i]);
                                    uiSystem.implants[i].GetComponent<Image>().color = uiSystem.lightColor;
                                }
                            }
                        }
                        Debug.Log(uiSystem.uiImplants[0]);
                        uiSystem.uiImplants[0].GetComponent<Image>().color = uiSystem.darkColor;
                        uiSystem.selectedImplantUI = uiSystem.uiImplants[0];
                        uiSystem.SetSelectedImplant();
                        StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.Repair));
                    }
                }

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.SetModeToHitUI(false, false, null, false, null);
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.WorldMode));
                    targetPathNode = null;
                }

                if (/*playerControls.Player.VerticalMenu.triggered*/isStickPress)
                {
                    //uiSystem.ChangeSelectedImplant(playingUnit.GetComponent<UnitGridSystem>().implants, playerControls.Player.VerticalMenu.ReadValue<float>());
                    //Debug.Log("VERTICAL MENU INPUT = " + playerControls.Player.VerticalMenu.ReadValue<float>());
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.Repair)
            {
                if (/*playerControls.Player.Validate.triggered*/isStickPress)
                {
                    uiSystem.SetSelectedImplant();
                    uiSystem.selectedImplant.GetComponent<ImplantSystem>().ReplaceImplant(uiSystem.otherImplant);
                    uiSystem.SetModeToHitUI(false, false, null, false, null);
                    playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                }

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    PathNode pathNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(playingUnit.transform.position.x), (int)Mathf.Round(playingUnit.transform.position.z));
                    uiSystem.SetModeToHitUI(false, false, null, false, null);
                    playingUnitSystem.ChangeState(UnitGridSystem.State.SelfInfo);
                    //uiSystem.SetModeToHitUI(true, pathNode.isContainingUnit, pathNode.unit, pathNode.isContainingImplant, pathNode.implant);
                    uiSystem.selectedImplantUI.GetComponent<Image>().color = uiSystem.lightColor;
                    uiSystem.selectedImplantUI = uiSystem.implants[uiSystem.implants.Length - 2];
                    uiSystem.selectedImplantUI.GetComponent<Image>().color = uiSystem.darkColor;
                    uiSystem.SetSelectedImplant();
                    playingUnitSystem.ChangeState(UnitGridSystem.State.SelfInfo);
                    targetPathNode = pathNode;
                }

                if (/*playerControls.Player.VerticalMenu.triggered*/isStickPress)
                {
                    //uiSystem.ChangeSelectedImplant(playingUnit.GetComponent<UnitGridSystem>().implants, playerControls.Player.VerticalMenu.ReadValue<float>());
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.MoveUI)
            {             
                if (/*playerControls.Player.Validate.triggered*/isStickPress)
                {
                    uiSystem.SetSelectedMoveAbility();
                    uiSystem.CloseUIMode(uiSystem.moveAbilities);
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.MoveWorld));
                }

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.CloseUIMode(uiSystem.moveAbilities);
                    gridSystem.ResetTileOutline();
                    playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                }
                
                if (/*playerControls.Player.VerticalMenu.triggered*/isStickPress)
                {
                    //uiSystem.ChangeSelectedMoveAbility(uiSystem.moveAbilities, playingUnitSystem.moveDataList, playerControls.Player.VerticalMenu.ReadValue<float>());
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.MoveWorld)
            {
                HoverSprite();

                if (/*playerControls.Player.Validate.triggered*/isStickPress)
                {
                    if (playingUnitSystem.DoMoveAbility(uiSystem.selectedMoveData))
                    {
                        playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                    }
                    else
                    {
                        gridSystem.ResetTileOutline();
                        playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                        Debug.Log("MOVE ABILITY UNVALID POSITION");
                    }
                }

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.SetModeToMoveUI(playingUnitSystem.moveDataList, uiSystem.selectedAbility);
                    playingUnitSystem.ChangeState(UnitGridSystem.State.MoveUI);
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.ActionUI)
            {
                if (/*playerControls.Player.Validate.triggered*/isStickPress)
                {
                    uiSystem.SetSelectedActionAbility();
                    uiSystem.CloseUIMode(uiSystem.actionAbilities);
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.ActionWorld));
                }

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.CloseUIMode(uiSystem.actionAbilities);
                    gridSystem.ResetTileOutline();
                    playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                }

                if (/*playerControls.Player.VerticalMenu.triggered*/isStickPress)
                {
                    //uiSystem.ChangeSelectedActionAbility(uiSystem.actionAbilities, playingUnitSystem.actionDataList, playerControls.Player.VerticalMenu.ReadValue<float>());
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.ActionWorld)
            {
                HoverSprite();
                
                if (uiSystem.selectedActionData.isWorld)
                {
                    if (/*playerControls.Player.Validate.triggered*/isStickPress)
                    {
                        if (playingUnitSystem.DoActionAbility(uiSystem.selectedActionData))
                        {
                            gridSystem.ResetTileOutline();
                            playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                        }
                        else
                        {
                            gridSystem.ResetTileOutline();
                            playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                            Debug.Log("ACTION ABILITY UNVALID POSITION");
                        }
                    }
                }
                else
                {
                    if (playingUnitSystem.DoActionAbility(uiSystem.selectedActionData))
                    {
                        gridSystem.ResetTileOutline();
                        playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                    }
                    else
                    {
                        gridSystem.ResetTileOutline();
                        playingUnitSystem.ChangeState(UnitGridSystem.State.WorldMode);
                    }
                }
                

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.SetModeToActionUI(playingUnitSystem.actionDataList, uiSystem.selectedAbility);
                    playingUnitSystem.ChangeState(UnitGridSystem.State.ActionUI);
                }
            }

            if (playingUnitSystem.state == UnitGridSystem.State.HitUI)
            {
                if (/*playerControls.Player.Validate.triggered*/isStickPress)
                {
                    uiSystem.SetSelectedImplant();
                    playingUnitSystem.remainingActionPoints -= uiSystem.selectedActionData.cost;
                    Debug.Log(uiSystem.selectedActionAbility.actionAbility.abilityName);
                    //uiSystem.selectedImplant.GetComponent<ImplantSystem>().InflictDamage(uiSystem.selectedActionData.damage);
                    uiSystem.selectedActionAbility.ApplyAbility(uiSystem.selectedImplant.transform.parent.gameObject, uiSystem.GetIndexOfObjectInArray(uiSystem.selectedImplant, uiSystem.selectedImplant.transform.parent.GetComponent<UnitGridSystem>().implants), null, pathfinding.GetNodeWithCoords((int)Mathf.Round(uiSystem.selectedImplant.transform.parent.position.x), (int)Mathf.Round(uiSystem.selectedImplant.transform.parent.position.z)));
                    uiSystem.SetModeToHitUI(false, false, null, false, null);
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.WorldMode));
                }

                if (/*playerControls.Player.Cancel.triggered*/isStickPress)
                {
                    uiSystem.CloseUIMode(uiSystem.actionAbilities);
                    uiSystem.SetModeToHitUI(false, false, null, false, null);
                    uiSystem.SetSelectedActionAbility();
                    StartCoroutine(playingUnitSystem.ChangeStateWithDelay(UnitGridSystem.State.ActionWorld));
                }

                if (/*playerControls.Player.VerticalMenu.triggered*/isStickPress)
                {
                    //uiSystem.ChangeSelectedImplant(uiSystem.targetUnit.GetComponent<UnitGridSystem>().implants, playerControls.Player.VerticalMenu.ReadValue<float>());
                }
            }
        }
        else
        {
            HoverSprite();
        }
    }
}