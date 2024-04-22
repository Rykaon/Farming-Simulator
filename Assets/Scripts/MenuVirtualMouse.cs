using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class MenuVirtualMouse : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private MenuManager menuManager;
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

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        playerControls = new PlayerControls();

        if (virtualMouse == null)
        {
            virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else if (!virtualMouse.added)
        {
            InputSystem.AddDevice(virtualMouse);
        }

        playerControls.Gamepad.Disable();
        playerControls.UI.Enable();

        InputUser.PerformPairingWithDevice(virtualMouse, playerInput.user);

        if (cursorRectTransform != null)
        {
            Vector2 position = new Vector2((Screen.width / 2), (Screen.height / 2));
            InputState.Change(virtualMouse.position, position);
            cursorRectTransform.position = position;
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
    }

    private void RaycastButton()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = virtualMouse.position.ReadValue();
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        raycaster.Raycast(pointer, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject.name == "NewRun" && menuManager.loadRoutine == null)
            {
                menuManager.NewRun();
            }
            else if (result.gameObject.name == "ContinueRun" && menuManager.loadRoutine == null && menuManager.hasRun)
            {
                menuManager.LoadRun();
            }
        }
    }

    private void Update()
    {
        UpdateCursorPosition();

        if (playerControls.UI.A.WasPressedThisFrame())
        {
            RaycastButton();
        }
    }
}
