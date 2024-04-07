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
    // TileState g�re l'�tat du sol de la case pour update son mat�rial en fonction de si le sol        //
    // est arros�, ensolleill�, plein d'herbe si la case est vierge ou de terre si une plante � �t�     //
    // plant�e.                                                                                         //
    //                                                                                                  //
    // SeedType g�re si oui ou non une plante � �t� plant�e sur la case.                                //
    //                                                                                                  //
    // GrowState g�re l'�tat de maturit� de la plante (si elle a �t� arros�e/ensoleill�e ou pas).       //
    // Il me semble que j'avais commenc� en mettant 3 stades de maturit�s, mais que dans les faits      //
    // pour le proto je n'utilise que GrowState.Low et GrowState.High parce qu'on est pas s�r           //
    // niveau GD. Si t'es s�r de toi et que t'as envie de r�gler �a, je te fais confiance pour          //
    // essayer d'impl�menter ta propre id�e.                                                            //
    //                                                                                                  //
    // Tous les changements d'�tats se font par des fonctions publiques appel�es par le script          //
    // PlayerController_Farm (cf. ExecuteAction dans ce script) et qui appellent ensuitent une          //
    // coroutine priv�e pour faire le changement de fa�on fluide (pour le grossissement des plantes)    //
    // ou g�rer le temps �coul� (pour les changements de Material de la case pour faire s�cher          //
    // terre apr�s avoir arros� la case ou repousser l'herbe apr�s avoir r�colter une plante).          //
    //                                                                                                  //
    // Honn�tement, je trouve le script tr�s lisible, tr�s parlant et pas compliqu� dans la logique     //
    // (m�me si �a peut �tre un peu tra�tre � d�bugger, je parle en connaissance de cause,              //
    // les coroutines du d�mon), donc je vais pas rentrer dans tous les d�tails.                        //
    //                                                                                                  //
    // Le SEUL d�tail un peu tricky et important, m�me si je l'ai d�j� mentionn� dans plein d'autres    //
    // commentaire, c'est que tileNode.plant != null quand la plante grandit, et c'est � ce moment-l�   //
    // qu'elle est ajout�e � la liste des plantes pour le combat. C'est donc l'ordre d'arriv�e �        //
    // maturit� qui d�finit l'ordre de priorit� des plantes au combat plut�t que l'ordre de plantation. //
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
