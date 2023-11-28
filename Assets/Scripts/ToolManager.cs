using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public static ToolManager instance;

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

    void Awake()
    {
        instance = this;
    }
}
