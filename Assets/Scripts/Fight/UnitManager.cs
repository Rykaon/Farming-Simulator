using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////////////////////
// Bon, j'espère que t'es prêt, parce que si je pense que je t'ai commenté toute l'architecture  //
// gloable du code, les trois quarts de ce que j'ai commenté t'as pas vraiment besoin de t'en    //
// servir, mais c'est juste pour que tu comprennes comment tout s'emboitent pour bien intégrer   //
// les assets et pouvoir tester/debug plus facilement. Mais si y'a deux script sur lesquelles    //
// tu vas avoir besoin de bosser ou au moins de comprendre totalement, c'est celui-ci et         //
// PlantManager.                                                                                 //
//                                                                                               //
// Les deux fonctionnent EXACTEMENT sur la même architecture, mais elle est très abstraite       //
// et le truc c'est que sans les assets et les animations j'avais aucun moyen de pouvoir         //
// le tester alors attends-toi à devoir debug.                                                   //
//                                                                                               //
// Au pire du pire, si tu vois que tu y arrives vraiment, essaye de faire le maximum de trucs    //
// à côté, et je m'en occuperais lundi, j'ai utilisé à peu près le même genre de méthode         //
// sur Echoes of the Nightmare, je m'en sortirais.                                               //
//                                                                                               //
// Je vais pas faire comme les autres scripts, avec des pavés qui expliquent la logique,         //
// là je vais te commenter pas à pas tout le script. Je vais faire pareil pour PlantManager.     //
//                                                                                               //
// Gloablement vu que UnitManager contient la même logique que PlantManager, mais contient       //
// aussi en plus la logique d'IA pour que les ennemis cherchent dynamiquement leur cible,        //
// Je ne vais pas réexpliquer les fonctionnement communs, mais juste les fonctionnalités         //
// spécifiques au UnitManager. Je te laisse te référer à mes commentaires dur le script          //
// PlantManager si tu veux comprendre comment ça se passe.                                       //
///////////////////////////////////////////////////////////////////////////////////////////////////

public class UnitManager : MonoBehaviour
{
    private Pathfinding pathfinding;

    public PathNode unitNode;
    public PathNode targetNode;

    // On a juste maxActionPoints et currentActionPoints en plus par rapport au PlantManager, les noms
    // des variables parlent d'eux-mêmes.
    [SerializeField] public int range;
    [SerializeField] public int maxActionPoints;
    public int currentActionPoints;
    public bool isActive = false;
    public bool hasPlay = false;
    public bool isPlaying = false;
    private bool hasReaction = false;
    private float elapsedTime = 0f;
    public bool isReactionAnimLonger = false;
    public int index = 0;
    public Animator ennemiAnimator;
    [SerializeField] private AnimationClip unitAttackAnim;
    [SerializeField] private float animReactionTime;
    float animTime;

    public GameObject target;

    private void Awake()
    {
        pathfinding = PlayerManager.instance.pathfinding;
        animTime = unitAttackAnim.length;
    }

    private void Start()
    {
        // On setup la position actuelle de l'ennemi et on lui set ses points d'actions.
        unitNode = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        currentActionPoints = maxActionPoints;
        target = null;
    }

    // Rien de nouveau par rapport à PlantManager.
    private IEnumerator ExecuteAction()
    {
        isPlaying = true;
        elapsedTime = 0f;
        hasReaction = true;

        ennemiAnimator.SetBool("Attack", true);
        BarkManager.instance.GetObjectFromPool(target, BarkManager.instance.EnnemiAttaque);
        BarkManager.instance.GetObjectFromPool(PlayerManager.instance.gameObject, BarkManager.instance.JoueurPerdUnePlante);

        bool isReactionSet = false;
        bool hasReactionSet = false;
        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= animReactionTime)
            {
                ennemiAnimator.SetBool("Attack", false);
                isReactionSet = true;
            }

            if (isReactionSet && !hasReactionSet)
            {
                hasReactionSet = true;
                Resolve();
            }
            yield return new WaitForEndOfFrame();
        }

        if (!isReactionAnimLonger)
        {
            isPlaying = false;

            if (currentActionPoints == 0)
            {
                hasPlay = true;
            }
            else
            {
                hasPlay = false;
            }
        }
    }

    // Toujours rien de nouveau par rapport à PlantManager.
    public IEnumerator ResolveReaction()
    {
        float elapsedTime = 0f;
        float animTime = 0f;

        if (animTime > (this.animTime - this.elapsedTime))
        {
            isReactionAnimLonger = true;
        }

        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        PlayerManager.instance.plantList.Remove(target);
        PlayerManager.instance.entitiesList.Remove(target);
        Destroy(target);
        targetNode.isSeeded = false;
        targetNode.isPlant = false;
        targetNode.plant = null;
        targetNode.tileManager.ChangeTileState(TileManager.TileState.Dirt);
        target = null;
        targetNode = null;

        for (int i = 0; i < PlayerManager.instance.plantList.Count; i++)
        {
            PlayerManager.instance.plantList[i].GetComponent<PlantManager>().index = PlayerManager.instance.plantList.Count - 1;
        }

        if (isReactionAnimLonger)
        {
            isPlaying = false;

            if (currentActionPoints == 0)
            {
                hasPlay = true;
            }
            else
            {
                hasPlay = false;
            }
        }
    }

    public void Resolve()
    {
        if (hasReaction)
        {
            StartCoroutine(ResolveReaction());
        }
    }

    // A partir d'ici on rentre dans les fonctionnalités spécifiques à l'ennemi.
    // En vrai j'y pense, mais tout fonctionne dans ces fonctions et t'as pas besoin d'y toucher.
    // Le seul truc que je vois de bizarre c'est cette coroutine pour le déplacement des ennemis.
    // En fait l'idée de la coroutine c'est d'être sûr que l'unité a bien fini son déplacement avant que la fonction qui check
    // la prochaine action de l'ennemi se lance. J'appelle cette coroutine dans la fonction CheckAction() juste en dessous.
    // J'utilise les fonctions du script UnitMovePathfinding pour le faire se déplacer (si t'as pas encore vu ce script,
    // va voir les commentaires que j'y ai mis pour comprendre sa logique). Il faudrait faire en sorte que la coroutine s'arrête
    // lorsque l'ennemi à atteint sa position, donc soit jouer avec le temps de la coroutine soit faire autrement avec un booléen
    // UnitMovePathfinding qui commeunique avec ce script. Bref je te laisse voir.
    // Pour le reste, c'est bon.
    // Si tu lis ça, c'était la dernière ligne de commentaire que je pensais nécéssaire de te faire, le reste fonctionne.
    // Sur ce, bon courage, j'espère que ça ira.

    public IEnumerator MoveAction()
    {
        float elapsedTime = 0f;
        float animTime = 1;

        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        isPlaying = false;

        if (currentActionPoints == 0)
        {
            hasPlay = true;
        }
        else
        {
            hasPlay = false;
        }
    }

    public void CheckAction()
    {
        isPlaying = true;

        if (target != null && targetNode != null)
        {
            currentActionPoints--;
            
            if (targetNode == unitNode)
            {
                StartCoroutine(ExecuteAction());
                return;
            }
            else
            {
                if (targetNode.isWalkable)
                {
                    List<PathNode> pathList = new List<PathNode>();
                    pathList = pathfinding.FindAreaPathMove(unitNode.x, unitNode.y, targetNode.x, targetNode.y);

                    if (pathList.Count > 0)
                    {
                        transform.GetComponent<UnitMovePathfinding>().SetVelocity(new Vector3(pathList[range].x, 0, pathList[range].y));
                        StartCoroutine(MoveAction());
                    }
                    else
                    {
                        Debug.Log("Erreur Path Not Found Between Unit and Target");
                    }
                }
                else
                {
                    FindTarget();
                    isPlaying = false;
                    hasPlay = false;
                }
            }
        }
        else
        {
            FindTarget();
            isPlaying = false;
            hasPlay = false;
        }
    }

    public void FindTarget()
    {
        GameObject target = null;
        List<GameObject> targetList = new List<GameObject>();
        List<PathNode> pathList = new List<PathNode>();
        PlayerManager playerManager = PlayerManager.instance;
        int currentRange = 0;
        bool hasFindOne = false;

        for (int i = 0; i < playerManager.plantList.Count; ++i)
        {   
            if (!playerManager.plantList[i].GetComponent<PlantManager>().plantNode.isContainingUnit)
            {
                pathList = pathfinding.FindAreaPathMove(unitNode.x, unitNode.y, playerManager.plantList[i].GetComponent<PlantManager>().plantNode.x, playerManager.plantList[i].GetComponent<PlantManager>().plantNode.y);
                if (!hasFindOne)
                {
                    currentRange = pathList.Count;
                    hasFindOne = true;
                    targetList.Add(playerManager.plantList[i]);
                }
                else
                {
                    if (pathList.Count < currentRange)
                    {
                        targetList = new List<GameObject>();
                        targetList.Add(playerManager.plantList[i]);
                        currentRange = pathList.Count;
                    }
                    else if (pathList.Count == currentRange)
                    {
                        targetList.Add(playerManager.plantList[i]);
                    }
                }
            }
        }

        if (targetList.Count > 0)
        {
            if (targetList.Count == 1)
            {
                target = targetList[0];
                this.target = target;
                targetNode = target.GetComponent<PlantManager>().plantNode;
            }
            else
            {
                List<GameObject> shuffledList = ShuffleList(targetList);
                target = shuffledList[0];
                this.target = target;
                targetNode = target.GetComponent<PlantManager>().plantNode;
            }
            return;
        }
        else
        {
            this.target = null;
            targetNode = null;
        }

        isActive = false;
    }

    private List<GameObject> ShuffleList(List<GameObject> list)
    {
        List<GameObject> shuffledList = list.OrderBy(x => Random.value).ToList();

        return shuffledList;
    }

    private void Update()
    {
        if (isActive && !hasPlay && !isPlaying && currentActionPoints != 0)
        {
            CheckAction();
        }
        else if (isActive && hasPlay && !isPlaying && currentActionPoints == 0)
        {
            isActive = false;
            hasPlay = false;
            isPlaying = false;
            currentActionPoints = maxActionPoints;
        }
    }
}
