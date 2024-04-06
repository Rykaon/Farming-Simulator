using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Fight : MonoBehaviour
{
    public static PlayerController_Fight instance;

    [Header("Component References")]
    [SerializeField] private PlayerManager PC_Manager;


    private PlayerControls playerControls;
    private Pathfinding pathfinding;

    public RadialMenu rm;

    public bool isActive = true;
    public bool canUseTool = true;

    public bool LBRBisPressed = false;

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Ce script est finalement assez court. J'aimerais te dire que c'est parce que je suis       //
    // un génie et que j'ai réussi à tout faire en une centaine de ligne (haha.) mais la vérité   //
    // c'est que la grande majorité des actions du joueur en combat ne se font pas ici, mais      //
    // dans la classe abstraite MenuAction.                                                       //
    //                                                                                            //
    // Je t'explique : en gros, les actions du joueurs sont                                       //
    // bind à une option du menu radial. Chaque élément de ce menu (la logique est dans le        //
    // script RadialMenuElements) contient une instance MenuAction. Et MenuAction contient        //
    // plusieurs sous-classes qui héritent de MenuActions et qui vont gérer les différentes       //
    // actions du joueur.                                                                         //
    //                                                                                            //
    // La seule fonction ici, elle est assez big parce qu'elle permet de gérer plusieurs cas.     //
    // En gros, elle cherchent quelles sont les cases valide pour une action. Mais selon          //
    // l'action, on cherche pas les mêmes cases. Du coup voilà. C'est là que toutes les           //
    // fonctions utilitaires pour communiquer avec la grille sont utilisées de partout.           //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        playerControls = PC_Manager.playerControls;
        pathfinding = PC_Manager.pathfinding;
        playerControls.Gamepad.Enable();
        playerControls.UI.Disable();
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////
    // Alors la grosse fonction. Eeeeeuh, je vois ce qu'elle fait mais y'a énormément de conditions //
    // à vérifier, c'est super long et fastidieux à expliquer, et en vrai, fais-moi confiance       //
    // elle marche. J'ai beau réfléchir, je pense que t'auras vraiment pas besoin de la modifier    //
    // donc regarde-là, elle est belle, elle est jolie, et y'a pas grand chose de plus à savoir.    //
    //                                                                                              //
    // Son utilité est expliquée à la fin du commentaire juste au-dessus au cas-où que tu l'ais lu  //
    // vite fait en diagonale sans faire gaffe.                                                     //
    //////////////////////////////////////////////////////////////////////////////////////////////////

    public List<GameObject> GetValidTarget(bool isMove, bool isAction, int range, PathNode startNode)
    {
        List<GameObject> objectList = new List<GameObject>();
        List<PathNode> pathList = new List<PathNode>();
        
        for (int i = startNode.x - range; i < startNode.x + range; ++i)
        {
            for (int j = startNode.y - range; j < startNode.y + range; ++j)
            {
                if (pathfinding.IsCoordsInGridRange(i, j))
                {
                    PathNode node = pathfinding.GetNodeWithCoords(i, j);

                    if (node != null)
                    {
                        if (!node.isVirtual)
                        {
                            if (isMove && !isAction)
                            {
                                if (!node.isContainingUnit)
                                {
                                    pathList = pathfinding.FindAreaPathMove(startNode.x, startNode.y, i, j);

                                    if (pathList != null)
                                    {
                                        if (pathList.Count <= range)
                                        {
                                            objectList.Add(node.tile);
                                        }
                                    }
                                }
                            }
                            else if (!isMove && isAction)
                            {
                                if (i == startNode.x || j == startNode.y)
                                {
                                    if (node.isContainingUnit)
                                    {
                                        if (node.unit != null)
                                        {
                                            if (node.unit.tag == "Unit")
                                            {
                                                pathList = pathfinding.FindLinearPathAction(startNode.x, startNode.y, i, j);
                                                
                                                if (pathList != null)
                                                {
                                                    if (pathList.Count <= range)
                                                    {
                                                        objectList.Add(node.unit);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (objectList.Count > 0)
        {
            return objectList;
        }
        else
        {
            return null;
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////
    // L'update ici aussi est minuscule. En fait, on ne check qu'un seul input, le bouton Y.        //
    // Si le joueur appuie dessus, on ouvre le menu des actions de combat. L'input pour fermer      //
    // le menu et revenir ici est directement dans l'update du menu (le script RadialMenu).         //
    //                                                                                              //
    // Je check aussi si le joueur appuie sur le bouton X, mais comme le fait de pouvoir terminer   //
    // le combat en restant appuyé sur B longtemps (cf. Update() du PlayerManager), c'est pas       //
    // du GD, c'est plus une fonctionnalité de confort pour tester le proto. Du coup ça permet      //
    // de reset les stats du joueurs (et donc ses points d'actions) pour pouvoir jouer plusieurs    //
    // fois d'affilés sans passer ton tour pour plus facilement tester des cas un peu particuliers. //
    //////////////////////////////////////////////////////////////////////////////////////////////////

    private void Update()
    {
        if (isActive)
        {
            if (playerControls.Gamepad.Y.IsPressed())
            {
                rm.gameObject.SetActive(true);
            }

            if (playerControls.Gamepad.X.IsPressed())
            {
                PC_Manager.ResetStats();
            }
        }
    }
}
