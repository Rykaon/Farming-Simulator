using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.Experimental.GraphView;
using Assets.Scripts;

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

    //////////////////////////////////////////////////////////////////////////////////////////////////////
    // TileState gère l'état du sol de la case pour update son matérial en fonction de si le sol        //
    // est arrosé, ensolleillé, plein d'herbe si la case est vierge ou de terre si une plante à été     //
    // plantée.                                                                                         //
    //                                                                                                  //
    // SeedType gère si oui ou non une plante à été plantée sur la case.                                //
    //                                                                                                  //
    // GrowState gère l'état de maturité de la plante (si elle a été arrosée/ensoleillée ou pas).       //
    // Il me semble que j'avais commencé en mettant 3 stades de maturités, mais que dans les faits      //
    // pour le proto je n'utilise que GrowState.Low et GrowState.High parce qu'on est pas sûr           //
    // niveau GD. Si t'es sûr de toi et que t'as envie de régler ça, je te fais confiance pour          //
    // essayer d'implémenter ta propre idée.                                                            //
    //                                                                                                  //
    // Tous les changements d'états se font par des fonctions publiques appelées par le script          //
    // PlayerController_Farm (cf. ExecuteAction dans ce script) et qui appellent ensuitent une          //
    // coroutine privée pour faire le changement de façon fluide (pour le grossissement des plantes)    //
    // ou gérer le temps écoulé (pour les changements de Material de la case pour faire sécher          //
    // terre après avoir arrosé la case ou repousser l'herbe après avoir récolter une plante).          //
    //                                                                                                  //
    // Honnêtement, je trouve le script très lisible, très parlant et pas compliqué dans la logique     //
    // (même si ça peut être un peu traître à débugger, je parle en connaissance de cause,              //
    // les coroutines du démon), donc je vais pas rentrer dans tous les détails.                        //
    //                                                                                                  //
    // Le SEUL détail un peu tricky et important, même si je l'ai déjà mentionné dans plein d'autres    //
    // commentaire, c'est que tileNode.plant != null quand la plante grandit, et c'est à ce moment-là   //
    // qu'elle est ajoutée à la liste des plantes pour le combat. C'est donc l'ordre d'arrivée à        //
    // maturité qui définit l'ordre de priorité des plantes au combat plutôt que l'ordre de plantation. //
    //////////////////////////////////////////////////////////////////////////////////////////////////////

    public TileState tileState;
    public SeedType seedType;
    public GrowState growState;

    [SerializeField] MeshRenderer meshRenderer;

    [SerializeField] private Material grassMat;
    [SerializeField] private Material dirtMat;
    [SerializeField] private Material wetDirtMat;
    [SerializeField] private Material sunDirtMat;

    [HideInInspector] public GameObject plant;
    [HideInInspector] public GameObject plantItem;
    [HideInInspector] public PlantManager plantManager;
    [HideInInspector] public PathNode tileNode;

    public Coroutine dirtToGrass;
    public Coroutine wetToDirt;
    public Coroutine sunToDirt;
    public Coroutine seededToNone;
    public Coroutine noneToSeeded;

    public bool isGrowing = false;
    public bool isBoosted = false;
    public GameObject boostVFX;

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
        plantItem = null;
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
