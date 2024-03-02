using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////////////////////////////////////////////
// Bon, j'esp�re que t'es pr�t, parce que si je pense que je t'ai comment� toute l'architecture  //
// gloable du code, les trois quarts de ce que j'ai comment� t'as pas vraiment besoin de t'en    //
// servir, mais c'est juste pour que tu comprennes comment tout s'emboitent pour bien int�grer   //
// les assets et pouvoir tester/debug plus facilement. Mais si y'a deux script sur lesquelles    //
// tu vas avoir besoin de bosser ou au moins de comprendre totalement, c'est celui-ci et         //
// UnitManager.                                                                                  //
//                                                                                               //
// Les deux fonctionnent EXACTEMENT sur la m�me architecture, mais elle est tr�s abstraite       //
// et le truc c'est que sans les assets et les animations j'avais aucun moyen de pouvoir         //
// le tester alors attends-toi � devoir debug.                                                   //
//                                                                                               //
// Au pire du pire, si tu vois que tu y arrives vraiment, essaye de faire le maximum de trucs    //
// � c�t�, et je m'en occuperais lundi, j'ai utilis� � peu pr�s le m�me genre de m�thode         //
// sur Echoes of the Nightmare, je m'en sortirais.                                               //
//                                                                                               //
// Je vais pas faire comme les autres scripts, avec des pav�s qui expliquent la logique,         //
// l� je vais te commenter pas � pas tout le script. Je vais faire pareil pour UnitManager.      //
///////////////////////////////////////////////////////////////////////////////////////////////////

public class PlantManager : MonoBehaviour
{
    // plantNode est le node (la case dans le pathfinding) sur laquelle se trouve la plante.
    // targetNode est le node vers lequel la plante est orient�e et sur laquelle elle va faire son action.
    // target va �tre l'objet pr�sent sur targetNode (s'il y en a un). Ca peut �tre le joueur comme un ennemi.
    // targetList c'est dans le cas ou la plante est boost� par une plante booste et donc que son action s'execute
    // sur plusieurs cibles.
    public PathNode plantNode;
    public PathNode targetNode;
    private GameObject target;
    private List<GameObject> targetList = new List<GameObject>();
    private Pathfinding pathfinding;
    public float range;
    public float maxRange;

    // isActive est set � true lorsque c'est le tour de la plante. Elle va executer son action gr�ce � une s�rie de coroutine.
    // A l'issue de ces coroutine, isActive redevient faux. isActive est set � true dans le playerManager dans la coroutine qui
    // g�re l'action des plantes, et sert de valeur pour la boucle while. Tant que isActive est true, alors on ne peut pas sortir
    // de la boucle while et �a emp�che de passer au tour de la prochaine plante tant que l'action de la plante pr�c�dante
    // ne s'est pas compl�tement r�solue.

    // hasPlay et isPlaying servent de marqueur interne pour suivre l'avancement de la r�solution de l'action de la plante.
    // Elles sont utilis�es dans l'update pour savoir quand lancer les couroutines et quand r�initialiser les diff�rentes valeurs
    // pour terminer le tour de la plante et pr�parer pour la setup pour son prochain tour.

    // hasReaction sert � d�terminer si l'action de la plante engendre une r�action (si elle touche un ennemi, un joueur, etc.)
    // elapsedTime sert � garder un indicateur de temps pour l'ex�cution des diff�rentes coroutines.
    // animTime sert � savoir la dur�e de l'animation de l'action de la plante. Pour l'instant c'est juste un float instanti�
    // que je setup dans l'inspecteur. A termes, je pense que �a serait optimale d'avoir une r�f�rence direct � l'animation.
    // Tu peux instantier cette r�f�rence comme �a => AnimationClip anim;
    // Dans l'Awake(), il faudra alors initialiser animTime pour qu'elle corresponde � anim.length.

    // isReactionAnimLonger, c'est le concept le plus dur � appr�hender. Je te l'expliquerais
    // lorsqu'il y aura besoin dans les coroutines, �a sera plus parlant une fois mis en application.
    // Pareil pour index.
    public bool isActive = false;
    public bool hasPlay = false;
    public bool isPlaying = false;
    private bool hasReaction = false;
    private float elapsedTime = 0f;
    float animTime = 0;
    public bool isReactionAnimLonger = false;
    public int index = 0;

    // isBoosted et boostFactor, de la m�me fa�on que pour le joueur dans le script PlayerController_Fight �a repr�sente si
    // oui ou non la plante est boost�e par une plante boost (la plante qui boost doit jouer avant cette plante pour que son execution
    // profite du buff. Et si oui, par combien de plantes elle est boost�, et donc de combien on va augmenter sa range.
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
        // on r�f�rence la position de la plante.
        plantNode = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

    // La seule chose dont tu n'as pas besoin de t'occuper, c'est cette fonction. Elle est appel�e par le script BuildObject
    // qui est attach� sur tous les objets que le joueur peut placer dans le vaisseau. Au moment o� il choisit sa position
    // d�finitive, on update la position de la plante pour qu'elle corresponde � la case choisie par le joueur.
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

    // La premi�re d�s deux coroutines. Celle-ci g�re l'execution de l'action de la plante, tandis que l'autre g�re la r�action de
    // la target si il y en a une.
    private IEnumerator ExecuteAction()
    {
        // Son action vient de commencer donc isPlaying devient true et �vite que l'update lance en boucle la coroutine.
        // elapsedTime doit �tre set � 0, mais animTime n'est pas �gale � 1, j'ai mis �a parce que je devais lui instantier une valeur
        // pour tester. Bien s�r, animTime reste �gale � AnimationClip anim.lenght
        isPlaying = true;
        elapsedTime = 0f;
        animTime = 1f;

        // Selon le type de la plante, on applique une m�thode diff�rente pour calculer les target (on veut par exemple que les plantes
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

        // Si � l'issue de ce calcule on a trouv� une target, alors l'action engendrera une r�action sur la target (en gros, comprends
        // que la target aussi va jouer une animation pour montrer qu'elle a �t� touch�e par une attaque par exemple).
        if (target != null)
        {
            hasReaction = true;
        }

        // On instantie des bool�ns qui vont permettre de garder une trace de l'avancement de la r�action de la target.
        bool isReactionSet = false;
        bool hasReactionSet = false;

        // On rentre dans une boucle while dont on ne sort pas tant que l'animation de la plante n'est pas fini. A chaque it�ration
        // on update elapsedTime avec Time.DeltaTime. A la fin de la boucle while on attends la fin de la frame pour recommencer la boucle
        // afin qu'elapsedTime garde une valeur r�elle du temps �coul�.
        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;

            // Ici j'ai mis des valeurs au hasard car je n'ai pas les animations. Ce qu'il faut v�rifier, c'est si elapsedTime est �gale
            // au temps �coul� n�cessaire pour que visuellement l'animation touche la target. En gros si la plante attaque, au bout de combien
            // de temps d'animation la plante met r�ellement le coup sur la target. Je suppose que dans une version ultra propre on ferait �a
            // des colliders pour connaitre si la target � �t� touch� selon le mesh de la target.
            // Mais bon, on va dire que la version � peu pr�s (oui c'est bine moi qui dit �a) suffit pour que visuellement �a passe. Une fois que
            // tu auras les animations, tu pourras les jouer dans l'inspecteur depuis les assets pour connaitre ce temps et lui d�finir.
            // Je suppose que tu devras ajouter une variable float au script pour set cette valeur dans l'inspecteur du script.
            // Une fois ce temps atteint, isReactionSet passe � true;
            if (elapsedTime >= animTime / 2)
            {
                isReactionSet = true;
            }

            // On utilise les deux bool�ns pour �tre s�r de ne passer qu'une seule fois dans la condition, et on appelle la fonction
            // Resolve() qui elle va lancer la coroutine de r�action de la target.
            if (isReactionSet && !hasReactionSet)
            {
                hasReactionSet = true;
                Resolve();
            }
            yield return new WaitForEndOfFrame();
        }

        // Le fameux bool�en isReactionAnimLonger sert � savoir si l'animation de r�action de la target est plus longue que le restant
        // de l'animation de l'action de la plante depuis qu'elle a touch� la target.
        // Si oui, et donc que isReactionAnimLonger est true, �a veut dire qu'� ce moment pr�cis, la target n'a pas encore fini son animation
        // de r�action. A ce moment-l� on ne fait rien et les bool�ens isPlaying et hasPlay seront set � la fin de la coroutine de r�action.
        // Dans le cas contraire, l'animation de r�action est d�j� finie et l'on peut set isPlaying � false et hasPlay � true.
        // Une fois fait �a veut dire que le tour de la plante sera termin� et que l'on commencera le tour de la prochaine unit�.
        // Du coup �a permet de s'assurer que tous les feedbacks et animations se soient jou�s enti�rement pour plus de lisibilit� des actions
        // � l'�cran pour le joueur.
        if (!isReactionAnimLonger)
        {
            isPlaying = false;
            hasPlay = true;
        }
    }

    // La deuxi�me coroutine qui va g�rer l'animation de r�action de la target si il y en a une.
    public IEnumerator ResolveReaction()
    {
        // Ici, elapsedTime correspond au temps �coul� � partir du d�but de l'animation de r�action de la target. On vient de rentrer dans
        // la coroutine donc elle vaut 0f. Attention � ne pas confondre avec this.elapsedTime qui correspond elle au temps �coul� depuis le
        // d�but de l'animation de la plante. A ce moment pr�cis, this.elapsedTime doit correspondre au temps n�c�ssaire � la plante pour
        // toucher la target avec son animation. On applique la m�me logique avec animeTime qui correspond au temps total de l'animation de 
        // r�action de la target tandis que this.animTime correspond au temps total de l'animation de la plante.
        // Il faudra trouver un moyen pour que les unit�s (plantes et ennemis) puissent avoir acc�s au diff�rentes animations de r�action, surement
        // en gardant directement les r�f�rences directement dans ces deux scripts PlantManager et UnitManager.
        float elapsedTime = 0f;
        float animTime = 0;

        // On calcule donc si l'animation de r�action est plus longue ou non que le restant de l'animation de la plante.
        if (animTime > (this.animTime - this.elapsedTime))
        {
            isReactionAnimLonger = true;
        }

        // La m�me logique, avec la boucle while on attends que l'animation de r�action se finissent.
        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // Enfin, on effectue sur les targets les modifications n�cessaires suite � l'action de la plante. Si c'est une plante Attack
        // on d�truit l'ennemi, si c'est une plante Move on le d�place, etc.
        switch (type)
        {
            case Type.Attack:
                animTime = 1;

                target.GetComponent<UnitManager>().unitNode.isContainingUnit = false;
                target.GetComponent<UnitManager>().unitNode.unit = null;
                target.GetComponent<UnitManager>().unitNode.isWalkable = true;
                PlayerManager.instance.unitList.Remove(target);
                Destroy(target);

                // Ici on se sert enfin de la variable index d�finit juste en dessous de isReactionAnimLonger dans le script.
                // En gros, dans le cas o� c'est une plante Attack et qu'elle a d�truit un ennemi, on le Destroy et on update
                // la liste des ennemis du PlayerManager pour que leur index reste coh�rent et que �a ne pose pas de probl�me
                // pour la suite des �v�nements. En fait, dans le script PlantManager, on ne modifie jamais la veleur de la variable
                // index du PlantManager, mais on modifie la valeur index du script UnitManager. Et inversement, dans le script UnitManager
                // on ne modifie jamais la variable index du script UnitManager, mais on modifie celle du script PlantManager.
                // Enfin bref, t'inqui�tes �a marche, c'est test� et approuv� normalement.
                for (int i = 0; i < PlayerManager.instance.unitList.Count; i++)
                {
                    PlayerManager.instance.unitList[i].GetComponent<UnitManager>().index = PlayerManager.instance.unitList.Count - 1;
                }

                Debug.Log("D�gats");
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
                        // Dans ce else et le prochain, on est dans deux cas o� on a voulu d�placer un ennemi sur une case contenant soit
                        // une autre unit�, soit en dehors des cases du vaisseau (contre un mur en gros).
                        // Comme d�j� mentionn� dans mes commentaires dans le script MenuAction (pour les actions Push et Pull), si l'id�e
                        // de compter les collisions commes un d�g�t comme dans Into The Breach, tu peut impl�menter la logique ici en reprenant
                        // la logique de la plante Attack juste au-dessus.
                        Debug.Log("D�gats Collision");
                    }
                }
                else
                {
                    Debug.Log("D�gats Collision");
                }

                Debug.Log("D�placement");
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
        // on set les bool�en isPlaying et hasPlay afin de finir le tour de la plante et passer � celui de la prochaine unit�.
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

    // Rien de sorcier, selon les valeurs des diff�rents bool�ens de contr�le on lance les coroutines ou on setup les variables
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
