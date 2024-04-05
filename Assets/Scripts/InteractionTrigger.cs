using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractionTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    // Doit être inactive dans le préfab
    [SerializeField] protected GameObject visualCue;

    protected bool playerInRange;

    protected PlayerManager PC_Manager;
    protected PlayerControls playerControls;

    protected virtual void Awake()
    {
        playerInRange = false;
    }

    protected virtual void Start()
    {
        PC_Manager = PlayerManager.instance;
        playerControls = PC_Manager.playerControls;
    }

    protected virtual void Update()
    {
        
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerInRange = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerInRange = false;
        }
    }
}
