using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerManager;

public class MapTrigger : InteractionTrigger
{
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
        if (playerInRange)
        {
            if (PC_Manager.controlState == PlayerManager.ControlState.World || PC_Manager.controlState == PlayerManager.ControlState.Farm)
            {
                visualCue.SetActive(true);
                if (playerControls.Gamepad.A.IsPressed())
                {
                    if (PC_Manager.controlState == PlayerManager.ControlState.Farm)
                    {
                        if (PC_Manager.PC_farm.isPlacing)
                        {
                            return;
                        }

                        // Mettre en pause le timer du farm
                    }

                    PC_Manager.mapGenerator.ShowHideUIMap(true);
                    PC_Manager.ChangeState(ControlState.WorldUI);
                    PC_Manager.virtualMouseManager.Enable();
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
