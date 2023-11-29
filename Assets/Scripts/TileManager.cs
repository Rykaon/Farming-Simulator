using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    [SerializeField] MeshRenderer meshRenderer;

    [SerializeField] Material grassMat;
    [SerializeField] Material dirtMat;
    [SerializeField] Material wetDirtMat;

    public Coroutine dirtToGrass;
    public Coroutine wetToDirt;

    public bool isGrowing = false;

    [SerializeField] float dirtToGrassDelay;
    [SerializeField] float wetToDirtDelay;

    public void ChangeTileState(TileState tileState)
    {
        switch (tileState)
        {
            case TileState.Grass:
                meshRenderer.material = grassMat;
                break;

            case TileState.Dirt:
                meshRenderer.material = dirtMat;
                dirtToGrass = StartCoroutine(DirtToGrass());
                break;

            case TileState.WetDirt:
                meshRenderer.material = wetDirtMat;
                wetToDirt = StartCoroutine(WetToDirt());

                if (seedType == SeedType.Seeded && !isGrowing)
                {
                    switch (growState)
                    {
                        case GrowState.Low:
                            StartCoroutine(ChangeGrowState(GrowState.Medium));
                            break;

                        case GrowState.Medium:
                            StartCoroutine(ChangeGrowState(GrowState.High));
                            break;
                    }
                }
                break;
        }

        this.tileState = tileState;
    }

    public IEnumerator DirtToGrass()
    {
        yield return new WaitForSecondsRealtime(dirtToGrassDelay);

        ChangeTileState(TileState.Grass);

        dirtToGrass = null;
    }

    public IEnumerator WetToDirt()
    {
        yield return new WaitForSecondsRealtime(wetToDirtDelay);

        ChangeTileState(TileState.Dirt);

        wetToDirt = null;
    }

    public void ChangeSeedType(SeedType seedType)
    {
        switch (seedType)
        {
            case SeedType.None:
                if (tileState == TileState.Dirt)
                {
                    dirtToGrass = StartCoroutine(DirtToGrass());
                }
                break;

            case SeedType.Seeded:
                break;
        }

        this.seedType = seedType;
    }

    public IEnumerator ChangeGrowState(GrowState growState)
    {
        isGrowing = true;

        switch (growState)
        {
            case GrowState.Low:
                ChangeSeedType(SeedType.None);
                break;

            case GrowState.Medium:
                yield return new WaitForSecondsRealtime(/*seed.growDelay*/ 0);
                break;

            case GrowState.High:
                yield return new WaitForSecondsRealtime(/*seed.growDelay*/ 0);
                break;
        }

        this.growState = growState;
        isGrowing = false;
    }
}
