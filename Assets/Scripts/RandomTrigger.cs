using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static PlayerManager;

public class RandomTrigger : InteractionTrigger
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

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
                if (playerControls.Gamepad.A.WasPressedThisFrame())
                {
                    if (!PC_Manager.mapGenerator.currentNode.mapEvent.isEventCheck)
                    {
                        PC_Manager.mapGenerator.currentNode.mapEvent.isEventCheck = true;
                        DialogueManager.instance.EnterDialogueMode(inkJSON);
                        // Lancer le dialogue du randomEvent

                        // Il faudra appeler en fin de dialogue la fonction TakeReward du MapGenerator
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
