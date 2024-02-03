using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.Experimental.GraphView;

public class TileManager : MonoBehaviour
{
    public enum TileState
    {
        Grass,
        Dirt,
        WetDirt,
        SunDirt
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

    [SerializeField] private Material grassMat;
    [SerializeField] private Material dirtMat;
    [SerializeField] private Material wetDirtMat;
    [SerializeField] private Material sunDirtMat;

    [HideInInspector] public GameObject plant;
    [HideInInspector] public PlantManager plantManager;
    [HideInInspector] public PathNode tileNode;

    public Coroutine dirtToGrass;
    public Coroutine wetToDirt;
    public Coroutine sunToDirt;
    public Coroutine seededToNone;
    public Coroutine noneToSeeded;

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
                if (seedType == SeedType.None)
                {
                    dirtToGrass = StartCoroutine(DirtToGrass());
                }
                break;

            case TileState.WetDirt:
                meshRenderer.material = wetDirtMat;
                wetToDirt = StartCoroutine(WetToDirt());

                if (seedType == SeedType.Seeded && growState == GrowState.Low)
                {
                    StartCoroutine(ChangeGrowState());
                }
                break;

            case TileState.SunDirt:
                meshRenderer.material = sunDirtMat;
                sunToDirt = StartCoroutine(SunToDirt());

                if (seedType == SeedType.Seeded && growState == GrowState.Low)
                {
                    StartCoroutine(ChangeGrowState());
                }
                break;
        }

        this.tileState = tileState;
    }

    public IEnumerator DirtToGrass()
    {
        float elapsedTime = 0f;

        while (elapsedTime < dirtToGrassDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        ChangeTileState(TileState.Grass);

        dirtToGrass = null;
    }

    public IEnumerator WetToDirt()
    {
        yield return new WaitForSecondsRealtime(wetToDirtDelay * 3);

        ChangeTileState(TileState.Dirt);

        wetToDirt = null;
    }

    public IEnumerator SunToDirt()
    {
        yield return new WaitForSecondsRealtime(wetToDirtDelay * 3);

        ChangeTileState(TileState.Dirt);

        sunToDirt = null;
    }

    public void ChangeSeedType(SeedType seedType)
    {
        this.seedType = seedType;

        switch (seedType)
        {
            case SeedType.None:
                Debug.Log(plant);
                StartCoroutine(DestroyPlant());
                break;

            case SeedType.Seeded:
                tileNode.isSeeded = true;
                tileNode.isPlant = false;
                plant.transform.localScale = Vector3.zero;
                plant.transform.DOScale(0.5f, wetToDirtDelay).SetEase(Ease.OutSine);
                break;
        }

        ChangeTileState(TileState.Dirt);
        growState = GrowState.Low;
    }

    public IEnumerator DestroyPlant()
    {
        float elapsedTime = 0f;
        PlayerManager.instance.plantList.Remove(plant);
        tileNode.isSeeded = false;
        tileNode.isPlant = false;
        tileNode.plant = null;
        plant.transform.DOScale(0f, wetToDirtDelay).SetEase(Ease.OutSine);
        GameObject plantToDestroy = plant;
        plant = null;

        while (elapsedTime < wetToDirtDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Destroy(plantToDestroy);
    }

    public IEnumerator ChangeGrowState()
    {
        float elapsedTime = 0f;
        growState = GrowState.High;
        tileNode.isPlant = true;
        tileNode.plant = plant;
        PlayerManager.instance.plantList.Add(plant);

        plant.transform.DOScale(wetToDirtDelay, wetToDirtDelay);

        while (elapsedTime < wetToDirtDelay)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
