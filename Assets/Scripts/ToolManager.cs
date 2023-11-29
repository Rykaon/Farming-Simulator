using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public static ToolManager instance;
    [SerializeField] PlayerController playerController;
    [SerializeField] BuildingSystem buildingSystem;

    public enum Tools
    {
        None,
        Shovel,
        Water,
        Seed,
        Animal,
        Fence,
        FenceCorner,
        FenceDoor
    }

    public Tools selectedTool;

    [SerializeField] GameObject shovelObject;
    [SerializeField] GameObject waterObject;
    [SerializeField] GameObject seedObject;
    [SerializeField] GameObject animalObject;
    [SerializeField] GameObject fenceObject;
    [SerializeField] GameObject fenceCornerObject;
    [SerializeField] GameObject fenceDoorObject;
    [SerializeField] GameObject selectedToolObject;

    void Awake()
    {
        instance = this;
    }
}
