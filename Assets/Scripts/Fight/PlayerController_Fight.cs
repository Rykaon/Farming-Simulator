using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Fight : MonoBehaviour
{
    public static PlayerController_Fight instance;
    public PlayerControls playerControls { get; private set; }

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
        Sun,
        Water,
        Seed,
        Object,
        Collect,
    }

    [Header("Component References")]
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody rigidBody;
    Pathfinding pathfinding;
    public ControlState controlState;
    public ActionState actionState;
    [SerializeField] GameObject groundTilePrefab;

    [Header("Properties")]
    [SerializeField] float moveSpeed;
    [SerializeField] float collisionDetectionDistance;

    private Vector3 movement;
    public GameObject currentTile;
    public TileManager currentTileManager;

    private const string isWalking = "isWalking";
    private const string isRunning = "isRunning";

    public bool canMove = true;
    public bool canUseTool = true;
    public bool inField = false;

    public bool LBRBisPressed = false;

    private void Awake()
    {
        instance = this;
        playerControls = new PlayerControls();
        pathfinding = new Pathfinding(9, 15);
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 15; ++j)
            {
                GameObject tile = Instantiate(groundTilePrefab, new Vector3(i + 0.5f, 0, j + 0.5f), Quaternion.identity);
                pathfinding.GetNodeWithCoords(i, j).SetTile(tile);
                pathfinding.GetNodeWithCoords(i, j).SetTileManager(tile.GetComponent<TileManager>());
            }
        }
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

        if (canMove)
        {
            if (currentTile != null)
            {
                currentTile.transform.GetChild(0).GetComponent<Outline>().enabled = false;
            }

            if (pathfinding.GetNodeWithPlayerWorldPos(transform.position) != null)
            {
                currentTile = pathfinding.GetNodeWithPlayerWorldPos(transform.position).tile;
                currentTileManager = pathfinding.GetTileWithPlayerWorldPos(transform.position);
            }
            else
            {
                currentTile = null;
                currentTileManager = null;
            }

            if (currentTile != null)
            {
                currentTile.transform.GetChild(0).GetComponent<Outline>().enabled = true;
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

    IEnumerator ExecuteAction()
    {
        canMove = false;
        canUseTool = false;

        switch (actionState)
        {
            case ActionState.Sun: // DIG (faire d'une tuile sauvage de la terre prête à être semée
                if (currentTileManager != null)
                {
                    if (currentTileManager.tileState == TileManager.TileState.Grass)
                    {
                        animator.Play("Digging");
                        currentTileManager.ChangeTileState(TileManager.TileState.Dirt);
                    }
                }
                break;

            case ActionState.Water: //Arroser une case, la faisant passée à l'état mouillée si elle ne l'était pas. Lance une coroutine avant que la terre redevienne sèche. Si terre déjà mouiller, reset de la couroutine
                if (currentTileManager != null)
                {
                    if (currentTileManager.tileState == TileManager.TileState.Dirt)
                    {
                        if (currentTileManager.dirtToGrass != null)
                        {
                            StopCoroutine(currentTileManager.dirtToGrass);
                        }
                        currentTileManager.ChangeTileState(TileManager.TileState.WetDirt);
                    }
                    else if (currentTileManager.tileState == TileManager.TileState.WetDirt)
                    {
                        if (currentTileManager.wetToDirt != null)
                        {
                            StopCoroutine(currentTileManager.wetToDirt);
                        }
                        currentTileManager.ChangeTileState(TileManager.TileState.WetDirt);
                    }
                }
                break;

            case ActionState.Seed: //Planter une plante 
                if (currentTileManager != null)
                {
                    if (currentTileManager.seedType == TileManager.SeedType.None)
                    {
                        if (currentTileManager.tileState == TileManager.TileState.Dirt)
                        {
                            if (currentTileManager.dirtToGrass != null)
                            {
                                StopCoroutine(currentTileManager.dirtToGrass);
                            }

                            currentTileManager.ChangeSeedType(TileManager.SeedType.Seeded);
                        }
                    }
                }
                break;

            case ActionState.Object:

                break;
        }

        canMove = true;
        canUseTool = true;
        return null;
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
                StartCoroutine(ExecuteAction());
            }
        }
    }
}
