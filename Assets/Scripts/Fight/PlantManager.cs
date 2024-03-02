using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////////////////////
// Bon, j'espère que t'es prêt, parce que si je pense que je t'ai commenté toute l'architecture  //
// gloable du code, les trois quarts de ce que j'ai commenté t'as pas vraiment besoin de t'en    //
// servir, mais c'est juste pour que tu comprennes comment tout s'emboitent pour bien intégrer   //
// les assets et pouvoir tester/debug plus facilement. Mais si y'a deux script sur lesquelles    //
// tu vas avoir besoin de bosser ou au moins de comprendre totalement, c'est celui-ci et         //
// UnitManager.                                                                                  //
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
// là je vais te commenter pas à pas tout le script. Je vais faire pareil pour UnitManager.      //
///////////////////////////////////////////////////////////////////////////////////////////////////

public class PlantManager : MonoBehaviour
{
    // plantNode est le node (la case dans le pathfinding) sur laquelle se trouve la plante.
    // targetNode est le node vers lequel la plante est orientée et sur laquelle elle va faire son action.
    // target va être l'objet présent sur targetNode (s'il y en a un). Ca peut être le joueur comme un ennemi.
    // targetList c'est dans le cas ou la plante est boosté par une plante booste et donc que son action s'execute
    // sur plusieurs cibles.
    public PathNode plantNode;
    public PathNode targetNode;
    private GameObject target;
    private List<GameObject> targetList = new List<GameObject>();
    private Pathfinding pathfinding;
    public float range;
    public float maxRange;

    // isActive est set à true lorsque c'est le tour de la plante. Elle va executer son action grâce à une série de coroutine.
    // A l'issue de ces coroutine, isActive redevient faux. isActive est set à true dans le playerManager dans la coroutine qui
    // gère l'action des plantes, et sert de valeur pour la boucle while. Tant que isActive est true, alors on ne peut pas sortir
    // de la boucle while et ça empêche de passer au tour de la prochaine plante tant que l'action de la plante précédante
    // ne s'est pas complètement résolue.

    // hasPlay et isPlaying servent de marqueur interne pour suivre l'avancement de la résolution de l'action de la plante.
    // Elles sont utilisées dans l'update pour savoir quand lancer les couroutines et quand réinitialiser les différentes valeurs
    // pour terminer le tour de la plante et préparer pour la setup pour son prochain tour.

    // hasReaction sert à déterminer si l'action de la plante engendre une réaction (si elle touche un ennemi, un joueur, etc.)
    // elapsedTime sert à garder un indicateur de temps pour l'exécution des différentes coroutines.
    // animTime sert à savoir la durée de l'animation de l'action de la plante. Pour l'instant c'est juste un float instantié
    // que je setup dans l'inspecteur. A termes, je pense que ça serait optimale d'avoir une référence direct à l'animation.
    // Tu peux instantier cette référence comme ça => AnimationClip anim;
    // Dans l'Awake(), il faudra alors initialiser animTime pour qu'elle corresponde à anim.length.

    // isReactionAnimLonger, c'est le concept le plus dur à appréhender. Je te l'expliquerais
    // lorsqu'il y aura besoin dans les coroutines, ça sera plus parlant une fois mis en application.
    // Pareil pour index.
    public bool isActive = false;
    public bool hasPlay = false;
    public bool isPlaying = false;
    private bool hasReaction = false;
    private float elapsedTime = 0f;
    float animTime = 0;
    public bool isReactionAnimLonger = false;
    public int index = 0;

    // isBoosted et boostFactor, de la même façon que pour le joueur dans le script PlayerController_Fight ça représente si
    // oui ou non la plante est boostée par une plante boost (la plante qui boost doit jouer avant cette plante pour que son execution
    // profite du buff. Et si oui, par combien de plantes elle est boosté, et donc de combien on va augmenter sa range.
    public bool isBoosted = false;
    public int boostFactor = 0;

    public enum Type
    {
        Attack,
        Move,
        Boost
    }

    public Type type;

    private void Awake()
    {
        pathfinding = PlayerManager.instance.pathfinding;
    }

    private void Start()
    {
        // Au start, une fois que toutes les initialisation et les Awake() des script importants sont faits,
        // on référence la position de la plante.
        plantNode = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

    // La seule chose dont tu n'as pas besoin de t'occuper, c'est cette fonction. Elle est appelée par le script BuildObject
    // qui est attaché sur tous les objets que le joueur peut placer dans le vaisseau. Au moment où il choisit sa position
    // définitive, on update la position de la plante pour qu'elle corresponde à la case choisie par le joueur.
    // On calcule ensuite quelle est la case que la plante va target avec son action selon sa rotation.
    public void CalculateTargetNode()
    {
        plantNode = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        PathNode node = null;
        switch (Mathf.RoundToInt(transform.eulerAngles.y))
        {
            case 0:
                node = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z) - 1);
                break;

            case 90:
                node = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x) - 1, Mathf.RoundToInt(transform.position.z));
                break;

            case 180:
                node = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z) +1);
                break;

            case 270:
                node = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x) + 1, Mathf.RoundToInt(transform.position.z));
                break;
        }
        targetNode = node;
    }

    // La première dès deux coroutines. Celle-ci gère l'execution de l'action de la plante, tandis que l'autre gère la réaction de
    // la target si il y en a une.
    private IEnumerator ExecuteAction()
    {
        // Son action vient de commencer donc isPlaying devient true et évite que l'update lance en boucle la coroutine.
        // elapsedTime doit être set à 0, mais animTime n'est pas égale à 1, j'ai mis ça parce que je devais lui instantier une valeur
        // pour tester. Bien sûr, animTime reste égale à AnimationClip anim.lenght
        isPlaying = true;
        elapsedTime = 0f;
        animTime = 1f;

        // Selon le type de la plante, on applique une méthode différente pour calculer les target (on veut par exemple que les plantes
        // boost puissent cible le joueur, mais pas les plantes attack).
        switch (type)
        {
            case Type.Attack:
                if (targetNode != null)
                {
                    if (targetNode.isContainingUnit)
                    {
                        if (targetNode.unit.tag == "Unit")
                        {
                            target = targetNode.unit;
                            Debug.Log("TAREGT = " + targetNode);
                        }
                    }
                }
                break;

            case Type.Move:
                if (plantNode.isContainingUnit)
                {
                    target = plantNode.unit;
                }
                break;

            case Type.Boost:
                if (targetNode != null)
                {
                    if (targetNode.isContainingUnit)
                    {
                        if (targetNode.unit.tag == "Player")
                        {
                            target = targetNode.unit;
                            targetList.Add(target);
                        }
                    }
                    else if (targetNode.isPlant)
                    {
                        target = targetNode.plant;
                        targetList.Add(target);
                    }

                }
                break;
        }

        // Si à l'issue de ce calcule on a trouvé une target, alors l'action engendrera une réaction sur la target (en gros, comprends
        // que la target aussi va jouer une animation pour montrer qu'elle a été touchée par une attaque par exemple).
        if (target != null)
        {
            hasReaction = true;
        }

        // On instantie des booléns qui vont permettre de garder une trace de l'avancement de la réaction de la target.
        bool isReactionSet = false;
        bool hasReactionSet = false;

        // On rentre dans une boucle while dont on ne sort pas tant que l'animation de la plante n'est pas fini. A chaque itération
        // on update elapsedTime avec Time.DeltaTime. A la fin de la boucle while on attends la fin de la frame pour recommencer la boucle
        // afin qu'elapsedTime garde une valeur réelle du temps écoulé.
        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;

            // Ici j'ai mis des valeurs au hasard car je n'ai pas les animations. Ce qu'il faut vérifier, c'est si elapsedTime est égale
            // au temps écoulé nécessaire pour que visuellement l'animation touche la target. En gros si la plante attaque, au bout de combien
            // de temps d'animation la plante met réellement le coup sur la target. Je suppose que dans une version ultra propre on ferait ça
            // des colliders pour connaitre si la target à été touché selon le mesh de la target.
            // Mais bon, on va dire que la version à peu près (oui c'est bine moi qui dit ça) suffit pour que visuellement ça passe. Une fois que
            // tu auras les animations, tu pourras les jouer dans l'inspecteur depuis les assets pour connaitre ce temps et lui définir.
            // Je suppose que tu devras ajouter une variable float au script pour set cette valeur dans l'inspecteur du script.
            // Une fois ce temps atteint, isReactionSet passe à true;
            if (elapsedTime >= animTime / 2)
            {
                isReactionSet = true;
            }

            // On utilise les deux booléns pour être sûr de ne passer qu'une seule fois dans la condition, et on appelle la fonction
            // Resolve() qui elle va lancer la coroutine de réaction de la target.
            if (isReactionSet && !hasReactionSet)
            {
                hasReactionSet = true;
                Resolve();
            }
            yield return new WaitForEndOfFrame();
        }

        // Le fameux booléen isReactionAnimLonger sert à savoir si l'animation de réaction de la target est plus longue que le restant
        // de l'animation de l'action de la plante depuis qu'elle a touché la target.
        // Si oui, et donc que isReactionAnimLonger est true, ça veut dire qu'à ce moment précis, la target n'a pas encore fini son animation
        // de réaction. A ce moment-là on ne fait rien et les booléens isPlaying et hasPlay seront set à la fin de la coroutine de réaction.
        // Dans le cas contraire, l'animation de réaction est déjà finie et l'on peut set isPlaying à false et hasPlay à true.
        // Une fois fait ça veut dire que le tour de la plante sera terminé et que l'on commencera le tour de la prochaine unité.
        // Du coup ça permet de s'assurer que tous les feedbacks et animations se soient joués entièrement pour plus de lisibilité des actions
        // à l'écran pour le joueur.
        if (!isReactionAnimLonger)
        {
            isPlaying = false;
            hasPlay = true;
        }
    }

    // La deuxième coroutine qui va gérer l'animation de réaction de la target si il y en a une.
    public IEnumerator ResolveReaction()
    {
        // Ici, elapsedTime correspond au temps écoulé à partir du début de l'animation de réaction de la target. On vient de rentrer dans
        // la coroutine donc elle vaut 0f. Attention à ne pas confondre avec this.elapsedTime qui correspond elle au temps écoulé depuis le
        // début de l'animation de la plante. A ce moment précis, this.elapsedTime doit correspondre au temps nécéssaire à la plante pour
        // toucher la target avec son animation. On applique la même logique avec animeTime qui correspond au temps total de l'animation de 
        // réaction de la target tandis que this.animTime correspond au temps total de l'animation de la plante.
        // Il faudra trouver un moyen pour que les unités (plantes et ennemis) puissent avoir accès au différentes animations de réaction, surement
        // en gardant directement les références directement dans ces deux scripts PlantManager et UnitManager.
        float elapsedTime = 0f;
        float animTime = 0;

        // On calcule donc si l'animation de réaction est plus longue ou non que le restant de l'animation de la plante.
        if (animTime > (this.animTime - this.elapsedTime))
        {
            isReactionAnimLonger = true;
        }

        // La même logique, avec la boucle while on attends que l'animation de réaction se finissent.
        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // Enfin, on effectue sur les targets les modifications nécessaires suite à l'action de la plante. Si c'est une plante Attack
        // on détruit l'ennemi, si c'est une plante Move on le déplace, etc.
        switch (type)
        {
            case Type.Attack:
                animTime = 1;

                target.GetComponent<UnitManager>().unitNode.isContainingUnit = false;
                target.GetComponent<UnitManager>().unitNode.unit = null;
                target.GetComponent<UnitManager>().unitNode.isWalkable = true;
                PlayerManager.instance.unitList.Remove(target);
                Destroy(target);

                // Ici on se sert enfin de la variable index définit juste en dessous de isReactionAnimLonger dans le script.
                // En gros, dans le cas où c'est une plante Attack et qu'elle a détruit un ennemi, on le Destroy et on update
                // la liste des ennemis du PlayerManager pour que leur index reste cohérent et que ça ne pose pas de problème
                // pour la suite des évènements. En fait, dans le script PlantManager, on ne modifie jamais la veleur de la variable
                // index du PlantManager, mais on modifie la valeur index du script UnitManager. Et inversement, dans le script UnitManager
                // on ne modifie jamais la variable index du script UnitManager, mais on modifie celle du script PlantManager.
                // Enfin bref, t'inquiètes ça marche, c'est testé et approuvé normalement.
                for (int i = 0; i < PlayerManager.instance.unitList.Count; i++)
                {
                    PlayerManager.instance.unitList[i].GetComponent<UnitManager>().index = PlayerManager.instance.unitList.Count - 1;
                }

                Debug.Log("Dégats");
                break;

            case Type.Move:
                animTime = 1;

                if (targetNode != null)
                {
                    if (!targetNode.isContainingUnit && targetNode.isWalkable)
                    {
                        target.GetComponent<UnitMovePathfinding>().SetVelocity(new Vector3(targetNode.x, 0, targetNode.y));
                    }
                    else
                    {
                        // Dans ce else et le prochain, on est dans deux cas où on a voulu déplacer un ennemi sur une case contenant soit
                        // une autre unité, soit en dehors des cases du vaisseau (contre un mur en gros).
                        // Comme déjà mentionné dans mes commentaires dans le script MenuAction (pour les actions Push et Pull), si l'idée
                        // de compter les collisions commes un dégât comme dans Into The Breach, tu peut implémenter la logique ici en reprenant
                        // la logique de la plante Attack juste au-dessus.
                        Debug.Log("Dégats Collision");
                    }
                }
                else
                {
                    Debug.Log("Dégats Collision");
                }

                Debug.Log("Déplacement");
                break;

            case Type.Boost:
                animTime = 1;

                for (int i = 0; i < targetList.Count; ++i)
                {
                    if (targetList[i].tag == "Plant")
                    {
                        PlantManager plantManager = targetList[i].GetComponent<PlantManager>();
                        plantManager.isBoosted = true;
                        plantManager.boostFactor++;
                        plantManager.range = boostFactor;
                    }
                    else if (targetList[i].tag == "Player")
                    {
                        PlayerManager.instance.isBoosted = true;
                        PlayerManager.instance.boostFactor++;
                        PlayerManager.instance.moveRange++;
                        PlayerManager.instance.actionRange++;
                    }
                }

                Debug.Log("Boost");
                break;
        }

        // Si effectivement l'animation de reaction de la target se finis APRES l'animation de la plante, alors seuelement maintenant
        // on set les booléen isPlaying et hasPlay afin de finir le tour de la plante et passer à celui de la prochaine unité.
        if (isReactionAnimLonger)
        {
            isPlaying = false;
            hasPlay = true;
        }    
    }

    public void Resolve()
    {
        if (hasReaction)
        {
            StartCoroutine(ResolveReaction());
        }
    }

    // Rien de sorcier, selon les valeurs des différents booléens de contrôle on lance les coroutines ou on setup les variables
    // pour correctement terminer le tour de la plante et la setup pour son prochain tour.
    private void Update()
    {
        if (isActive && !hasPlay && !isPlaying)
        {
            if (isBoosted)
            {
                range = maxRange * boostFactor;
            }
            else
            {
                range = maxRange;
            }

            StartCoroutine(ExecuteAction());
        }
        else if (isActive && hasPlay && !isPlaying)
        {
            isBoosted = false;
            hasPlay = false;
            isPlaying = false;
            boostFactor = 0;
            isActive = false;
            isReactionAnimLonger = false;
            target = null;
            targetList = new List<GameObject>();
        }
    }
}
