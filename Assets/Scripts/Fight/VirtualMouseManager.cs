using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static PlayerManager;

public class VirtualMouseManager : MonoBehaviour
{
    public static VirtualMouseManager instance;

    public enum TypeToSelect
    {
        Plant,
        Unit,
        Tile
    }

    public TypeToSelect typeToSelect;
    private string tagToSelect;
    private int layerToRaycast;
    private MenuAction action;

    [Header("Component References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private PlayerControls playerControls;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private RectTransform cursorRectTransform;
    [SerializeField] private GraphicRaycaster raycaster;

    [Header("Properties")]
    [SerializeField] private float cursorSpeed = 1000f;
    [SerializeField] private float padding;

    private bool previousMouseState;
    private Mouse virtualMouse;
    private Camera mainCamera;

    public GameObject target;
    public List<GameObject> targetList;

    private MapUIElement currentMapUIElement;
    
    public bool isActive { get; private set; }

    private void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        isActive = false;
        currentMapUIElement = null;
    }

    private void Start()
    {
        playerControls = playerManager.playerControls;

        if (virtualMouse == null)
        {
            virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else if (!virtualMouse.added)
        {
            InputSystem.AddDevice(virtualMouse);
        }
    }

    public void Enable(TypeToSelect typeToSelect, List<GameObject> list, MenuAction action)
    {
        cursorRectTransform.gameObject.SetActive(true);
        playerControls.Gamepad.Disable();
        playerControls.UI.Enable();
        PlayerController_Fight.instance.isActive = false;
        this.action = action;
        targetList = list;
        target = null;

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
            Vector2 position = new Vector2((Screen.width / 2), (Screen.height / 2));
            InputState.Change(virtualMouse.position, position);
            cursorRectTransform.position = position;
        }

        this.typeToSelect = typeToSelect;

        switch (typeToSelect)
        {
            case TypeToSelect.Tile:
                tagToSelect = "Tile";
                layerToRaycast = 10;
                break;

            case TypeToSelect.Plant:
                tagToSelect = "Plant";
                layerToRaycast = 11;
                break;

            case TypeToSelect.Unit:
                tagToSelect = "Unit";
                layerToRaycast = 12;
                break;
        }

        OutlineByType(true, typeToSelect, Color.white);

        isActive = true;
    }

    public void Enable()
    {
        cursorRectTransform.gameObject.SetActive(true);
        playerControls.Gamepad.Disable();
        playerControls.UI.Enable();

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
            Vector2 position = new Vector2((Screen.width / 2), (Screen.height / 2));
            InputState.Change(virtualMouse.position, position);
            cursorRectTransform.position = position;
        }

        isActive = true;
    }

    public void Disable(bool value)
    {
        playerControls.Gamepad.Enable();
        playerControls.UI.Disable();

        if (value)
        {
            PlayerController_Fight.instance.isActive = true;
        }

        if (cursorRectTransform != null)
        {
            Vector2 position = new Vector2((Screen.width / 2), (Screen.height / 2));
            InputState.Change(virtualMouse.position, position);
            cursorRectTransform.position = position;
        }

        if (value)
        {
            OutlineByType(false, typeToSelect, Color.white);
        }

        InputSystem.RemoveDevice(virtualMouse);
        cursorRectTransform.gameObject.SetActive(false);
        isActive = false;
    }

    public void OutlineByType(bool enable, TypeToSelect typeToSelect, Color color)
    {
        foreach (GameObject element in targetList)
        {
            for (int i = 0; i < element.transform.childCount; i++)
            {
                for (int j = 0; j < element.transform.GetChild(i).GetComponent<MeshRenderer>().materials.Length; j++)
                {
                    if (enable)
                    {
                        Utilities.SetEmission(element.transform.GetChild(i).GetComponent<MeshRenderer>().materials[j], 0.15f);
                    }
                    else
                    {
                        Utilities.SetEmission(element.transform.GetChild(i).GetComponent<MeshRenderer>().materials[j], 0f);
                    }
                }
            }
        }
    }

    public void OutlineElement(bool enable, GameObject element, Color color)
    {
        element.transform.GetChild(0).GetComponent<Outline>().enabled = enable;

        if (enable)
        {
            element.transform.GetChild(0).GetComponent<Outline>().OutlineColor = color;
        }
    }

    private void UpdateCursorPosition()
    {
        if (virtualMouse == null || Gamepad.current == null)
        {
            return;
        }

        Vector2 deltaValue = playerControls.UI.Navigate.ReadValue<Vector2>();
        deltaValue *= cursorSpeed * Time.deltaTime;
        Vector2 currentPosition = virtualMouse.position.ReadValue();
        Vector2 newPosition = currentPosition + deltaValue;

        newPosition.x = Mathf.Clamp(newPosition.x, padding, Screen.width - padding);
        newPosition.y = Mathf.Clamp(newPosition.y, padding, Screen.height - padding);

        InputState.Change(virtualMouse.position, newPosition);
        InputState.Change(virtualMouse.delta, deltaValue);

        cursorRectTransform.position = newPosition;

        AnchoredCursor(newPosition);
    }

    private void AnchoredCursor(Vector2 position)
    {
        Vector2 anchoredPositon;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main, out anchoredPositon);
        //cursorRectTransform.anchoredPosition = anchoredPositon;
    }

    GameObject RaycastToWorldPosition(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue))
        {
            if (hit.collider != null)
            {
                if (hit.collider.tag == tagToSelect)
                {
                    if (targetList.Contains(hit.collider.transform.parent.gameObject))
                    {
                        if (hit.collider.transform.parent.gameObject != target)
                        {
                            if (target != null)
                            {
                                for (int i = 0; i < target.transform.childCount; i++)
                                {
                                    for (int j = 0; j < target.transform.GetChild(i).GetComponent<MeshRenderer>().materials.Length; j++)
                                    {
                                        Utilities.SetEmission(target.transform.GetChild(i).GetComponent<MeshRenderer>().materials[j], 00.15f);
                                    }
                                }
                            }

                            target = hit.collider.transform.parent.gameObject;
                            for (int i = 0; i < target.transform.childCount; i++)
                            {
                                for (int j = 0; j < target.transform.GetChild(i).GetComponent<MeshRenderer>().materials.Length; j++)
                                {
                                    Utilities.SetEmission(target.transform.GetChild(i).GetComponent<MeshRenderer>().materials[j], 0.35f);
                                }
                            }
                        }
                        return hit.collider.transform.parent.gameObject;
                    }
                }
            }
            else
            {
                if (target != null)
                {
                    target.transform.GetChild(0).GetComponent<Outline>().OutlineColor = Color.white;
                }
            }
        }

        return null;
    }

    MapUIElement RaycastToScreenPosition()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = virtualMouse.position.ReadValue();
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        //EventSystem.current.RaycastAll(pointer, raycastResults);
        raycaster.Raycast(pointer, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject.tag == "MapEvent")
            {
                if (result.gameObject.transform.childCount != 0)
                {
                    return result.gameObject.transform.parent.GetComponent<MapUIElement>();
                }
                else
                {
                    return result.gameObject.GetComponent<MapUIElement>();
                }
            }
        }

        return null;
    }

    private void Update()
    {
        if (isActive)
        {
            UpdateCursorPosition();

            if (playerManager.controlState == PlayerManager.ControlState.Fight)
            {
                RaycastToWorldPosition(virtualMouse.position.ReadValue());

                if (playerControls.UI.B.IsPressed())
                {
                    Disable(true);
                }
                else if (playerControls.UI.A.IsPressed())
                {
                    if (RaycastToWorldPosition(virtualMouse.position.ReadValue()) != null && playerManager.currentActionsPoints > 0)
                    {
                        action.ExecuteAction(RaycastToWorldPosition(virtualMouse.position.ReadValue()));
                        Disable(true);
                    }
                }
            }
            else if (playerManager.controlState == PlayerManager.ControlState.World)
            {
                MapUIElement mapUIElement = RaycastToScreenPosition();
                bool hasCurrentMapElementChanged = false;
                if (currentMapUIElement != null)
                {
                    if ((mapUIElement != null && currentMapUIElement != mapUIElement) || mapUIElement == null)
                    {
                        currentMapUIElement.Hover(false);
                        currentMapUIElement = mapUIElement;
                        hasCurrentMapElementChanged = true;
                    }
                }
                else
                {
                    if (mapUIElement != null)
                    {
                        currentMapUIElement = mapUIElement;
                        hasCurrentMapElementChanged = true;
                    }
                }

                if (hasCurrentMapElementChanged)
                {
                    if (currentMapUIElement != null)
                    {
                        currentMapUIElement.Hover(true);
                        currentMapUIElement.generator.SetUIMapEventInfo(currentMapUIElement.mapEvent);
                    }
                    else
                    {
                        playerManager.mapGenerator.SetUIMapEventInfo(null);
                    }
                }

                if (playerControls.UI.B.IsPressed())
                {
                    if (currentMapUIElement != null)
                    {
                        currentMapUIElement.Hover(false);
                        currentMapUIElement = null;
                    }

                    playerManager.mapGenerator.ShowHideUIMap(false);
                    playerManager.ChangeState(ControlState.Farm);
                    playerManager.virtualMouseManager.Disable(false);
                    Disable(false);
                }
                else if (playerControls.UI.A.IsPressed())
                {
                    if (currentMapUIElement != null)
                    {
                        if (currentMapUIElement.mapEvent.eventNode.x - playerManager.mapGenerator.currentNode.x == 1)
                        {
                            currentMapUIElement.Select();
                        }
                    }
                }
            }
        }
    }
}
