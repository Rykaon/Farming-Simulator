using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController_Farm : MonoBehaviour
{
    public static PlayerController_Farm instance;

    [Header("Component References")]
    [SerializeField] private PlayerManager PC_Manager;

    private PlayerControls playerControls;
    private Pathfinding pathfinding;
    public RadialMenu rm;

    [Header("Properties")]
    [SerializeField] float moveSpeed;
    [SerializeField] float collisionDetectionDistance;

    private Vector3 movement;
    public GameObject previousTarget = null;
    //public TTileManager currentTileManager;

    public bool isPlacing = false;
    public BuildObject buildObject;

    private const string isWalking = "isWalking";
    private const string isRunning = "isRunning";

    public bool isActive = true;
    public bool canUseTool = true;

    public bool isPlaying = false;

    public bool LBRBisPressed = false;

    public bool hasMovementBeenReset;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        playerControls = PC_Manager.playerControls;
        pathfinding = PC_Manager.pathfinding;
        playerControls.Gamepad.Enable();
        playerControls.UI.Disable();
    }

    public PathNode GetCurrentNode()
    {
        return PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

    private void Move()
    {
        movement = Vector3.zero;

        if (isActive)
        {
            PathNode node = GetCurrentNode();
            GameObject target = null;

            if (playerControls.Gamepad.LeftStick.ReadValue<Vector2>() != Vector2.zero)
            {
                movement += new Vector3(playerControls.Gamepad.LeftStick.ReadValue<Vector2>().x, 0f, playerControls.Gamepad.LeftStick.ReadValue<Vector2>().y);
            }

            if (movement.magnitude > 0.1f && movement.magnitude < 0.5f)
            {
                PC_Manager.animator.SetBool(isWalking, true);
                PC_Manager.animator.SetBool(isRunning, false);
            }
            else if (movement.magnitude > 0.5f)
            {
                PC_Manager.animator.SetBool(isWalking, false);
                PC_Manager.animator.SetBool(isRunning, true);
            }
            else
            {
                PC_Manager.animator.SetBool(isWalking, false);
                PC_Manager.animator.SetBool(isRunning, false);
            }

            if (movement != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement.normalized), 0.15f);
            }

            if (!RaycastCollision())
            {
                PC_Manager.rigidBody.velocity = movement * moveSpeed * Time.deltaTime;
            }
            else
            {
                PC_Manager.rigidBody.velocity = Vector3.zero;
            }

            if (node != null)
            {
                if (!node.isVirtual)
                {
                    if (isPlacing)
                    {
                        if (!node.isSeeded)
                        {
                            target = node.tile;
                        }
                    }
                    else
                    {
                        target = node.tile;
                    }
                }
            }

            if (previousTarget != null)
            {
                previousTarget.transform.GetChild(0).GetComponent<Outline>().enabled = false;
            }
            if (target != null)
            {
                target.transform.GetChild(0).GetComponent<Outline>().enabled = true;
            }
            previousTarget = target;
        }
        else
        {
            if (previousTarget != null)
            {
                if (PC_Manager.virtualMouseManager.isActive)
                {
                    previousTarget.transform.GetChild(0).GetComponent<Outline>().enabled = false;
                }
                else
                {
                    previousTarget.transform.GetChild(0).GetComponent<Outline>().enabled = true;
                }
            }
        }
    }

    private void ResetMovement()
    {
        movement = Vector3.zero;
        PC_Manager.animator.SetBool(isWalking, false);
        PC_Manager.animator.SetBool(isRunning, false);
        PC_Manager.rigidBody.velocity = Vector3.zero;
        hasMovementBeenReset = true;
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

    IEnumerator ExecuteAction()
    {
        PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        switch (PC_Manager.actionState)
        {
            case PlayerManager.ActionState.Plant:
                if (PC_Manager.inventory.GetIndexByObject(PC_Manager.plant) > -1)
                {
                    int index = PC_Manager.inventory.GetIndexByObject(PC_Manager.plant);
                    if (PC_Manager.inventory.itemNbrList[index] > 0)
                    {
                        InitializeWithObject(PC_Manager.plant);
                    }
                }
                break;

            case PlayerManager.ActionState.Object:
                //InitializeWithObject(PC_Manager.object);
                break;

            case PlayerManager.ActionState.Collect:
                if (node != null)
                {
                    if (!node.isVirtual)
                    {
                        if (node.isPlant)
                        {
                            if (node.tileManager.growState == TileManager.GrowState.High)
                            {
                                node.tileManager.ChangeSeedType(TileManager.SeedType.None);
                            }
                        }
                    }
                }
                break;

            case PlayerManager.ActionState.Water:
                if (node != null)
                {
                    if (!node.isVirtual)
                    {
                        if (node.tileManager.seedType == TileManager.SeedType.Seeded)
                        {
                            if (node.tileManager.wetToDirt != null)
                            {
                                StopCoroutine(node.tileManager.wetToDirt);
                            }

                            if (node.tileManager.sunToDirt != null)
                            {
                                StopCoroutine(node.tileManager.sunToDirt);
                            }

                            node.tileManager.ChangeTileState(TileManager.TileState.WetDirt);
                        }
                    }
                }
                break;

            case PlayerManager.ActionState.Sun:
                if (node != null)
                {
                    if (!node.isVirtual)
                    {
                        if (node.tileManager.seedType == TileManager.SeedType.Seeded)
                        {
                            if (node.tileManager.wetToDirt != null)
                            {
                                StopCoroutine(node.tileManager.wetToDirt);
                            }

                            if (node.tileManager.sunToDirt != null)
                            {
                                StopCoroutine(node.tileManager.sunToDirt);
                            }

                            node.tileManager.ChangeTileState(TileManager.TileState.SunDirt);
                        }
                    }
                }
                break;
        }
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        isPlaying = false;
        yield return null;
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        if (buildObject != null)
        {
            /*if (GetCurrentNode() != null)
            {
                position = new Vector3(position.x, 0, position.z);

                buildObject.transform.localScale = Vector3.one;
            }
            else
            {
                position = new Vector3(transform.position.x, 0, transform.position.z);
                buildObject.transform.localScale = Vector3.zero;
            }*/

            position = new Vector3(position.x, 0, position.z);

            buildObject.transform.localScale = Vector3.one;
        }

        return position;
    }

    public void InitializeWithObject(GameObject tile)
    {
        isPlacing = true;

        GameObject obj;

        obj = Instantiate(tile, new Vector3(Mathf.RoundToInt(PC_Manager.transform.position.x), 0, Mathf.RoundToInt(PC_Manager.transform.position.z)), Quaternion.identity);
        obj.transform.localScale = new Vector3(0, 0, 0);

        buildObject = obj.GetComponent<BuildObject>();
        obj.AddComponent<BuildObjectDrag>();
    }

    public bool CanBePlaced()
    {
        PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        if (node != null)
        {
            if (!node.isVirtual)
            {
                if (!node.isSeeded)
                {
                    if (PC_Manager.inventory.GetIndexByObject(PC_Manager.plant) > -1)
                    {
                        int index = PC_Manager.inventory.GetIndexByObject(PC_Manager.plant);
                        if (PC_Manager.inventory.itemNbrList[index] > 0)
                        {
                            PC_Manager.inventory.itemNbrList[index]--;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private void Update()
    {
        Move();

        if (isActive)
        {
            hasMovementBeenReset = false;

            if (isPlacing)
            {
                PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

                if ((!playerControls.Gamepad.LB.IsPressed() && !playerControls.Gamepad.RB.IsPressed()) && LBRBisPressed)
                {
                    LBRBisPressed = false;
                }

                if (playerControls.Gamepad.A.IsPressed())
                {
                    if (CanBePlaced())
                    {
                        
                        buildObject.Place();
                        node.tileManager.ChangeSeedType(TileManager.SeedType.Seeded);
                        buildObject = null;
                        LBRBisPressed = false;
                        isPlacing = false;
                    }
                    else
                    {
                        Destroy(buildObject.gameObject);
                        buildObject = null;
                        LBRBisPressed = false;
                        isPlacing = false;
                    }
                }
                else if (playerControls.Gamepad.B.IsPressed())
                {
                    Destroy(buildObject.gameObject);
                    buildObject = null;
                    LBRBisPressed = false;
                    isPlacing = false;
                }
                else if (playerControls.Gamepad.LB.IsPressed() && !LBRBisPressed)
                {
                    buildObject.Rotate(-1);

                    LBRBisPressed = true;
                }
                else if (playerControls.Gamepad.RB.IsPressed() && !LBRBisPressed)
                {
                    buildObject.Rotate(1);

                    LBRBisPressed = true;
                }
            }
            else
            {
                if (playerControls.Gamepad.Y.IsPressed())
                {
                    rm.gameObject.SetActive(true);
                }

                if (playerControls.Gamepad.X.IsPressed() && !isPlaying)
                {
                    isPlaying = true;
                    StartCoroutine(ExecuteAction());
                }
            }
        }
        else
        {
            if (!hasMovementBeenReset)
            {
                ResetMovement();
            }
        }
    }
}
