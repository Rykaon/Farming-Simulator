using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public enum TileState
    {
        Grass,
        Dirt,
        WetDirt
    }

    public enum SeedType
    {
        None,
        Seeded
    }

    public enum GrowState
    {
        Low,
        Medium,
        High
    }

    public TileState tileState;
    public SeedType seedType;
    public GrowState growState;

    [SerializeField] Material grassMat;
    [SerializeField] Material dirtMat;
    [SerializeField] Material wetDirtMat;

    [SerializeField] float wetDelay;

    public void ChangeTileState(TileState tileState)
    {
        switch (tileState)
        {
            case TileState.Grass:
                break;

            case TileState.Dirt:
                break;

            case TileState.WetDirt:
                break;
        }
    }

    public void ChangeSeedType(SeedType seedType)
    {
        switch (seedType)
        {
            case SeedType.None:
                break;

            case SeedType.Seeded:
                break;
        }
    }

    public void ChangeGrowState(GrowState growState)
    {
        switch (growState)
        {
            case GrowState.Low:
                break;

            case GrowState.Medium:
                break;

            case GrowState.High:
                break;
        }
    }
}
