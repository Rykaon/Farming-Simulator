using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public PlayerControls playerControls { get; private set; }

    [Header("Component References")]
    private BuildingSystem buildingSystem;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody rigidBody;

    [Header("Properties")]
    [SerializeField] float moveSpeed;
    [SerializeField] float collisionDetectionDistance;
    [SerializeField] AnimationClip waterAnim;
    [SerializeField] AnimationClip digAnim;

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

    private void Awake()
    {
        instance = this;
        playerControls = new PlayerControls();
        buildingSystem = BuildingSystem.instance;
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
        forwardPos = currentPos + new Vector3Int(Mathf.RoundToInt(transform.forward.x), Mathf.RoundToInt(transform.forward.y) -1, Mathf.RoundToInt(transform.forward.z));

        if (canMove)
        {
            if (buildingSystem.objectToPlace == null)
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

    private void FixedUpdate()
    {
        Move();

        if (canUseTool && playerControls.Gamepad.X.IsPressed())
        {
            //Dash();
        }
    }
}
