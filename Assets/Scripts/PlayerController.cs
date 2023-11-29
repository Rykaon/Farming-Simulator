using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public PlayerControls playerControls { get; private set; }

    public enum ControlState
    {
        World,
        Inventory,
        Market,
        ToolMenu,
        Builder
    }

    [Header("Component References")]
    [SerializeField] BuildingSystem buildingSystem;
    [SerializeField] ToolManager toolManager;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody rigidBody;
    public ControlState controlState;

    [Header("Properties")]
    [SerializeField] float moveSpeed;
    [SerializeField] float collisionDetectionDistance;
    [SerializeField] AnimationClip waterAnim;
    [SerializeField] AnimationClip digAnim;
    [SerializeField] AnimationClip kneelAnim;
    [SerializeField] AnimationClip standAnim;
    [SerializeField] AnimationClip plantAnim;
    [SerializeField] AnimationClip pickLowAnim;
    [SerializeField] AnimationClip pickMediumAnim;
    [SerializeField] AnimationClip pickHighAnim;
    [SerializeField] AnimationClip genericToolAnim;


    private Vector3 movement;
    public Vector3Int currentPos;
    public Vector3Int forwardPos;
    public GameObject currentGroundTile;
    public TileManager currentGroundTileManager;
    public GameObject currentObjectTile;
    public BuildObject currentObjectManager;

    private const string isWalking = "isWalking";
    private const string isRunning = "isRunning";
    private const string isWatering = "isWatering";
    private const string isDigging = "isDigging";

    public bool canMove = true;
    public bool canUseTool = true;
    public bool inField = false;

    public bool LBRBisPressed = false;

    private void Awake()
    {
        instance = this;
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Move()
    {
        movement = Vector3.zero;
        if (playerControls.Gamepad.LeftStick.ReadValue<Vector2>().x != 0f)
        {
            movement += new Vector3(playerControls.Gamepad.LeftStick.ReadValue<Vector2>().x, 0f, 0f);
        }

        if (playerControls.Gamepad.LeftStick.ReadValue<Vector2>().y != 0f)
        {
            movement += new Vector3(0f, 0f, playerControls.Gamepad.LeftStick.ReadValue<Vector2>().y);
        }

        if (movement.magnitude > 0.1f && movement.magnitude < 0.5f)
        {
            animator.SetBool(isWalking, true);
            animator.SetBool(isRunning, false);
        }
        else if (movement.magnitude > 0.5f)
        {
            animator.SetBool(isWalking, false);
            animator.SetBool(isRunning, true);
        }
        else
        {
            animator.SetBool(isWalking, false);
            animator.SetBool(isRunning, false);
        }

        if (canMove)
        {
            if (movement != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement.normalized), 0.15f);
            }

            if (!RaycastCollision())
            {
                rigidBody.velocity = movement * moveSpeed * Time.deltaTime;
            }
            else
            {
                rigidBody.velocity = Vector3.zero;
            }
        }

        currentPos = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y) + 1, Mathf.RoundToInt(transform.position.z));
        forwardPos = currentPos + new Vector3Int(Mathf.RoundToInt(transform.forward.x), 0, Mathf.RoundToInt(transform.forward.z));

        if (canMove)
        {
            if (!buildingSystem.isPlacing)
            {
                if (buildingSystem.GetObjectTile() != null)
                {
                    if (buildingSystem.GetObjectTile() != currentObjectTile)
                    {
                        if (currentObjectTile != null)
                        {
                            if (currentObjectTile.tag == "Fence")
                            {
                                currentObjectTile.transform.GetChild(0).GetComponent<Outline>().enabled = false;
                            }
                            else if (currentObjectTile.tag == "Corner")
                            {
                                currentObjectTile.transform.GetChild(0).GetChild(0).GetComponent<Outline>().enabled = false;
                                currentObjectTile.transform.GetChild(0).GetChild(1).GetComponent<Outline>().enabled = false;
                            }
                            else if (currentObjectTile.tag == "Door")
                            {
                                currentObjectTile.transform.GetChild(0).GetChild(0).GetComponent<Outline>().enabled = false;
                            }
                        }

                        currentObjectTile = buildingSystem.GetObjectTile();
                        currentObjectManager = currentObjectTile.GetComponent<BuildObject>();

                        if (currentObjectTile.tag == "Fence")
                        {
                            currentObjectTile.transform.GetChild(0).GetComponent<Outline>().enabled = true;
                        }
                        else if (currentObjectTile.tag == "Corner")
                        {
                            currentObjectTile.transform.GetChild(0).GetChild(0).GetComponent<Outline>().enabled = true;
                            currentObjectTile.transform.GetChild(0).GetChild(1).GetComponent<Outline>().enabled = true;
                        }
                        else if (currentObjectTile.tag == "Door")
                        {
                            currentObjectTile.transform.GetChild(0).GetChild(0).GetComponent<Outline>().enabled = true;
                        }
                    }

                    if (currentGroundTile != null)
                    {
                        currentGroundTile.transform.GetChild(0).GetComponent<Outline>().enabled = false;
                        currentGroundTile = null;
                        currentGroundTileManager = null;
                    }
                }
                else
                {
                    if (currentObjectTile != null)
                    {
                        if (currentObjectTile.tag == "Fence")
                        {
                            currentObjectTile.transform.GetChild(0).GetComponent<Outline>().enabled = false;
                        }
                        else if (currentObjectTile.tag == "Corner")
                        {
                            currentObjectTile.transform.GetChild(0).GetChild(0).GetComponent<Outline>().enabled = false;
                            currentObjectTile.transform.GetChild(0).GetChild(1).GetComponent<Outline>().enabled = false;
                        }
                        else if (currentObjectTile.tag == "Door")
                        {
                            currentObjectTile.transform.GetChild(0).GetChild(0).GetComponent<Outline>().enabled = false;
                        }

                        currentObjectTile = null;
                        currentObjectManager = null;
                    }
                }
            }
            else
            {
                if (currentObjectTile != null)
                {
                    if (currentObjectTile.tag == "Fence")
                    {
                        currentObjectTile.transform.GetChild(0).GetComponent<Outline>().enabled = false;
                    }
                    else if (currentObjectTile.tag == "Corner")
                    {
                        currentObjectTile.transform.GetChild(0).GetChild(0).GetComponent<Outline>().enabled = false;
                        currentObjectTile.transform.GetChild(0).GetChild(1).GetComponent<Outline>().enabled = false;
                    }
                    else if (currentObjectTile.tag == "Door")
                    {
                        currentObjectTile.transform.GetChild(0).GetChild(0).GetComponent<Outline>().enabled = false;
                    }

                    currentObjectTile = null;
                    currentObjectManager = null;
                }
            }

            if (currentObjectTile == null)
            {
                if (buildingSystem.GetGroundTile() != null)
                {
                    if (buildingSystem.GetGroundTile() != currentGroundTile)
                    {
                        if (currentGroundTile != null)
                        {
                            currentGroundTile.transform.GetChild(0).GetComponent<Outline>().enabled = false;
                            currentGroundTile = null;
                            currentGroundTileManager = null;
                        }

                        currentGroundTile = buildingSystem.GetGroundTile();
                        currentGroundTileManager = currentGroundTile.GetComponent<TileManager>();
                        currentGroundTile.transform.GetChild(0).GetComponent<Outline>().enabled = true;
                    }
                }
                else
                {
                    if (currentGroundTile != null)
                    {
                        currentGroundTile.transform.GetChild(0).GetComponent<Outline>().enabled = false;
                        currentGroundTile = null;
                        currentGroundTileManager = null;
                    }
                }
            }
        }
    }

    private bool RaycastCollision()
    {
        bool isCollisionDetected = false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, collisionDetectionDistance))
        {
            Debug.DrawRay(transform.position, transform.forward * collisionDetectionDistance, Color.red);
            if (hit.collider.tag == "Level")
            {
                isCollisionDetected = true;
            }
        }

        return isCollisionDetected;
    }

    IEnumerator UseTool()
    {
        canMove = false;
        canUseTool = false;

        switch (toolManager.selectedTool)
        {
            case ToolManager.Tools.Shovel:
                if (currentGroundTileManager != null)
                {
                    if (currentGroundTileManager.tileState == TileManager.TileState.Grass)
                    {
                        animator.Play("Digging");
                        yield return new WaitForSecondsRealtime(digAnim.length);
                        currentGroundTileManager.ChangeTileState(TileManager.TileState.Dirt);
                    }
                }
                break;

            case ToolManager.Tools.Water:
                if (currentGroundTileManager != null)
                {
                    if (currentGroundTileManager.tileState == TileManager.TileState.Dirt)
                    {
                        animator.Play("Watering");
                        yield return new WaitForSecondsRealtime(waterAnim.length);

                        if (currentGroundTileManager.dirtToGrass != null)
                        {
                            StopCoroutine(currentGroundTileManager.dirtToGrass);
                        }
                        currentGroundTileManager.ChangeTileState(TileManager.TileState.WetDirt);
                    }
                    else if (currentGroundTileManager.tileState == TileManager.TileState.WetDirt)
                    {
                        animator.Play("Watering");
                        yield return new WaitForSecondsRealtime(waterAnim.length);

                        if (currentGroundTileManager.wetToDirt != null)
                        {
                            StopCoroutine(currentGroundTileManager.wetToDirt);
                        }
                        currentGroundTileManager.ChangeTileState(TileManager.TileState.WetDirt);
                    }
                }
                break;

            case ToolManager.Tools.Seed:
                if (currentGroundTileManager != null)
                {
                    if (currentGroundTileManager.seedType == TileManager.SeedType.None)
                    {
                        if (currentGroundTileManager.tileState == TileManager.TileState.Dirt)
                        {
                            animator.Play("KneelDown");
                            yield return new WaitForSecondsRealtime(kneelAnim.length + plantAnim.length + standAnim.length);

                            if (currentGroundTileManager.dirtToGrass != null)
                            {
                                StopCoroutine(currentGroundTileManager.dirtToGrass);
                            }

                            currentGroundTileManager.ChangeSeedType(TileManager.SeedType.Seeded);
                        }
                    }
                }
                break;

            case ToolManager.Tools.Animal:
                animator.Play("UseGenericTool");
                yield return new WaitForSecondsRealtime(genericToolAnim.length / 3);


                break;

            case ToolManager.Tools.Fence:
                animator.Play("UseGenericTool");
                yield return new WaitForSecondsRealtime(genericToolAnim.length / 3);

                buildingSystem.InitializeWithObject(buildingSystem.fence, buildingSystem.objectTilemap);
                controlState = ControlState.Builder;
                break;

            case ToolManager.Tools.FenceCorner:
                animator.Play("UseGenericTool");
                yield return new WaitForSecondsRealtime(genericToolAnim.length / 3);

                buildingSystem.InitializeWithObject(buildingSystem.fenceCorner, buildingSystem.objectTilemap);
                controlState = ControlState.Builder;
                break;

            case ToolManager.Tools.FenceDoor:
                animator.Play("UseGenericTool");
                yield return new WaitForSecondsRealtime(genericToolAnim.length / 3);

                buildingSystem.InitializeWithObject(buildingSystem.fenceDoor, buildingSystem.objectTilemap);
                controlState = ControlState.Builder;
                break;
        }

        canMove = true;
        canUseTool = true;
    }

    IEnumerator PickObject()
    {
        if (inField)
        {

        }
        else if (currentGroundTileManager != null)
        {
            if (currentGroundTileManager.growState == TileManager.GrowState.High)
            {
                // PickPlant

                StartCoroutine(currentGroundTileManager.ChangeGrowState(TileManager.GrowState.Low));
            }
        }

        yield return null;
    }

    private void FixedUpdate()
    {
        Move();

        if (!playerControls.Gamepad.LBRB.IsPressed())
        {
            LBRBisPressed = false;
        }

        if (controlState == ControlState.World)
        {
            if (canUseTool && playerControls.Gamepad.X.IsPressed())
            {
                StartCoroutine(UseTool());
            }
            else if (canUseTool && playerControls.Gamepad.B.IsPressed())
            {
                StartCoroutine(PickObject());
            }
        }
        else if (controlState == ControlState.Inventory)
        {

        }
        else if (controlState == ControlState.Market)
        {

        }
        else if (controlState == ControlState.ToolMenu)
        {

        }
        else if (controlState == ControlState.Builder)
        {
            if (playerControls.Gamepad.B.IsPressed() && buildingSystem.objectToPlace != null)
            {
                Destroy(buildingSystem.objectToPlace.gameObject);
                buildingSystem.objectToPlace = null;
                buildingSystem.tileToPlace = null;
                buildingSystem.isPlacing = false;
                controlState = ControlState.World;
            }
            else if (playerControls.Gamepad.A.IsPressed() && buildingSystem.objectToPlace != null)
            {
                if (buildingSystem.CanBePlaced(buildingSystem.objectToPlace, buildingSystem.objectTilemap))
                {
                    buildingSystem.objectToPlace.Place();
                    Vector3Int start = buildingSystem.gridLayout.WorldToCell(buildingSystem.objectToPlace.GetStartPosition());
                    buildingSystem.TakeArea(start, buildingSystem.objectToPlace.size, buildingSystem.objectTilemap);
                    buildingSystem.objectToPlace = null;
                    buildingSystem.tileToPlace = null;
                    buildingSystem.isPlacing = false;
                    controlState = ControlState.World;
                }
                else
                {
                    Destroy(buildingSystem.objectToPlace.gameObject);
                    buildingSystem.objectToPlace = null;
                    buildingSystem.tileToPlace = null;
                    buildingSystem.isPlacing = false;
                    controlState = ControlState.World;
                }
            }
            else if (playerControls.Gamepad.LBRB.IsPressed() && buildingSystem.objectToPlace != null && !LBRBisPressed)
            {
                buildingSystem.objectToPlace.Rotate(playerControls.Gamepad.LBRB.ReadValue<float>());
                LBRBisPressed = true;
            }
        }
    }
}
