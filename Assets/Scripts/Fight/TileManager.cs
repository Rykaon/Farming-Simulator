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

                if (seedType == SeedType.Seeded)
                {
                    StartCoroutine(ChangeGrowState());
                }
                break;

            case TileState.SunDirt:
                meshRenderer.material = sunDirtMat;
                sunToDirt = StartCoroutine(SunToDirt());

                if (seedType == SeedType.Seeded)
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
        yield return new WaitForSecondsRealtime(wetToDirtDelay);

        ChangeTileState(TileState.Dirt);

        wetToDirt = null;
    }

    public IEnumerator SunToDirt()
    {
        yield return new WaitForSecondsRealtime(wetToDirtDelay);

        ChangeTileState(TileState.Dirt);

        sunToDirt = null;
    }

    public void ChangeSeedType(SeedType seedType)
    {
        PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        switch (seedType)
        {
            case SeedType.None:
                dirtToGrass = StartCoroutine(DirtToGrass());
                node.tileManager.transform.DOScale(0f, wetToDirtDelay).SetEase(Ease.OutSine);
                PlayerManager.instance.plantList.Remove(plant);
                Destroy(node.plant);
                node.isSeeded = false;
                node.isPlant = false;
                node.plant = null;
                break;

            case SeedType.Seeded:
                node.isSeeded = true;
                node.isPlant = false;
                Debug.Log(node.tileManager.plant);
                node.tileManager.plant.transform.localScale = Vector3.zero;
                node.tileManager.plant.transform.DOScale(0.5f, wetToDirtDelay).SetEase(Ease.OutSine);
                growState = GrowState.Low;
                break;
        }

        this.seedType = seedType;
    }

    public IEnumerator ChangeGrowState()
    {
        float elapsedTime = 0f;
        PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

        switch (growState)
        {
            case GrowState.High:
                node.tileManager.plant.transform.DOScale(wetToDirtDelay, wetToDirtDelay);
                
                while (elapsedTime < wetToDirtDelay)
                {
                    elapsedTime += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                
                node.plant = plant;
                PlayerManager.instance.plantList.Add(node.plant);
                break;
        }

        growState = GrowState.High;
        yield return null;
    }
}
