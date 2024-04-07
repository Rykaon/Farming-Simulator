using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTrigger : InteractionTrigger
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
        if (playerInRange && !DialogueManager.instance.isActive)
        {
            if (PC_Manager.controlState == PlayerManager.ControlState.World)
            {
                visualCue.SetActive(true);
                if (playerControls.Gamepad.A.IsPressed())
                {
                    if (!PC_Manager.mapGenerator.currentNode.mapEvent.isEventCheck)
                    {
                        if (PC_Manager.inventory.nbArgent < PC_Manager.mapGenerator.currentNode.mapEvent.nbrReward)
                        {
                            // Lancer un dialogue qui dit "T'as pas assez de thune sur toi, passe au marchand d'abord"
                        }
                        else
                        {
                            PC_Manager.mapGenerator.currentNode.mapEvent.isEventCheck = true;
                            // Lancer le dialogue pour finir la run
                        }
                    }
                    else
                    {
                        // Lancer un dialogue pour dire "Casse-toi y'a plus rien à voir ici"
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
