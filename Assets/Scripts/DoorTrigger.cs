using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerManager;

public class DoorTrigger : InteractionTrigger
{
    [SerializeField] Transform outsidePoint, insidePoint;
    private bool southButtonIsPressed = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (southButtonIsPressed && !playerControls.Gamepad.A.IsPressed())
        {
            southButtonIsPressed = false;
        }

        if (playerInRange)
        {
            if (PC_Manager.controlState == ControlState.World)
            {
                visualCue.SetActive(true);

                if (playerControls.Gamepad.A.IsPressed() && !southButtonIsPressed)
                {
                    southButtonIsPressed = true;
                    if (PC_Manager.position == Position.Inside)
                    {
                        PC_Manager.rigidBody.position = new Vector3(outsidePoint.position.x, PC_Manager.rigidBody.position.y, outsidePoint.position.z);
                        PC_Manager.position = Position.Outside;
                    }
                    else
                    {
                        PC_Manager.rigidBody.position = new Vector3(insidePoint.position.x, PC_Manager.rigidBody.position.y, insidePoint.position.z);
                        PC_Manager.position = Position.Inside;
                    }
                }
            }
        }
        else
        {
            visualCue.SetActive(false);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }
}
