using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuAction
{
    private RadialMenuElement rme;
    private PlayerController_Fight playerController;
    private PlayerManager playerManager;
    private Pathfinding pathfinding;

    ////////////////////////////////////////////////////////////////////////////////////////////////
    // Voici la classe MenuAction. Elle ne contient que trois fonctions. Dans les faits je n'en   //
    // utilise que deux, UndoAction ne fait jamais rien, je sais pas si y'aura moyen/besoin de    //
    // l'utiliser, dans le cas où on voudrait annuler une action et revenir en arrière. Dans le   //
    // doute, j'ai laissé l'implémentation, mais t'en occupes pas.                                //
    //                                                                                            //
    // Pour l'instant les trois fonctions ne font rien, c'est juste une implémentation générale   //
    // pour que toutes les sous-classes ci-dessous puissent en hériter.                           //
    ////////////////////////////////////////////////////////////////////////////////////////////////

    public abstract void SelectActionTarget();
    public abstract void ExecuteAction(GameObject target);
    public abstract void UndoAction();

    public class SubmenuAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // On commence direct avec un cas particulier. Dans le cas où le joueur est dans le menu Farm //
        // et qu'il selectionne l'outil Plante ou Objet, on execute rien pour l'instant. A la place   //
        // on ouvre un sous-menu pour qu'il puisse choisir quel Plante ou quel Objet précisément      //
        // il veut instantier. Bref, pas besoin d'y toucher.                                          //
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public SubmenuAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
        }

        public override void SelectActionTarget()
        {
            
        }

        public override void ExecuteAction(GameObject target)
        {
            rme.subRM.isSubRM = true;
            rme.parentRM.EnableDisable(false);
            rme.subRM.gameObject.SetActive(true);
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class MoveAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // C'est l'action pour que le joueur se déplace sur la grille pendant les combats.            //
        //                                                                                            //
        // La fonction SelectActionTarget() (implémantée dans PlayerController_Fight) retourne une    //
        // liste de GameObject (les tiles des cases valides).                                         //
        // Si elle n'est pas nulle, on SetActive le script VirtualMouseManager qui permet aux joueur  //
        // de selectionner avec une souris virtuelle (controlée par le joystick) pour selectionner la //
        // tile qu'il désire. Le foncionnement de ce script ne me paraît pas vraiment important,      //
        // je doute que tu l'utilise (et accessoirement il est déjà 5h du matin) alors je ne l'ai pas //
        // commenté.                                                                                  //
        //                                                                                            //
        // Dans la fonction ExecuteAction() (appelée justement dans ce script VirtualMouseManager     //
        // lorsque le joueur selectionne une case), on fait bouger le joueur en utilisant le script   //
        // MovePathfindind. Ce script-là par contre, tu vas sûrement devoir le comprendre parce qu'il //
        // permet à toutes les unités de bouger, donc autant le joueur que les ennemis. Je te l'ai    //
        // commenté aussi.                                                                            //
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public MoveAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
        }

        public override void SelectActionTarget()
        {
            List<GameObject> list = new List<GameObject>();
            int range = (playerManager.currentActionsPoints * playerManager.moveRange) - playerManager.currentDistanceMoved;
            list = playerController.GetValidTarget(true, false, range, playerManager.playerNode);
            
            if (list != null)
            {
                rme.parentRM.gameObject.SetActive(false);
                VirtualMouseManager.instance.Enable(VirtualMouseManager.TypeToSelect.Tile, list, rme.action);
            }
        }

        public override void ExecuteAction(GameObject target)
        {
            Vector3 targetPos = new Vector3(Mathf.RoundToInt(target.transform.position.x), 0, Mathf.RoundToInt(target.transform.position.z));
            playerManager.movePathfinding.SetVelocity(targetPos);

            playerManager.UpdateFightUI();
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class PushAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // C'est l'action pour que le joueur repousse un ennemi.                                      //
        //                                                                                            //
        // Exactement la même logique que pour MoveAction, dans la fonction SelectActionTarget        //
        // j'utilise la même fonction implémantée dans PlayerControllerScript pour avoir une liste    //
        // de target possible. Ensuite on utilise le script VirtualMouseManager pour qu'il puisse     //
        // choisir sa cible. Pas besoin d'y toucher ça marche très bien.                              //
        //                                                                                            //
        // La fonction ExecuteAction est légèrement plus complexe, et je pense que t'auras besoin de  //
        // la modifier, donc je t'ai mis directement les commentaires dedans                          //
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public PushAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
        }

        public override void SelectActionTarget()
        {
            List<GameObject> list = new List<GameObject>();
            list = playerController.GetValidTarget(false, true, playerManager.actionRange, playerManager.playerNode);

            if (list != null)
            {
                rme.parentRM.gameObject.SetActive(false);
                VirtualMouseManager.instance.Enable(VirtualMouseManager.TypeToSelect.Unit, list, rme.action);
            }
        }

        public override void ExecuteAction(GameObject target)
        {
            // TargetStartPos est la position en X,Y dans la grille de l'ennemi.
            // TargetEndPos est la position en X,Y sur laquelle on veut déplacer l'ennemi.
            // On l'instantie comme Vector2.zero afin d'éviter d'utiliser une variable non assignée ce qui donne une erreur.
            Vector2Int targetStartPos = new Vector2Int(Mathf.RoundToInt(target.transform.position.x), Mathf.RoundToInt(target.transform.position.z));
            Vector2Int targetEndPos = Vector2Int.zero;

            // On compare la position de départ de l'ennemi avec celle du joueur pour en déduire sa position d'arrivée.
            if (targetStartPos.x == Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y < Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(0, -1);
            }
            else if (targetStartPos.x == Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y > Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(0, 1);
            }
            else if (targetStartPos.x < Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y == Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(-1, 0);
            }
            else if (targetStartPos.x > Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y == Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(1, 0);
            }

            // Le node qui correspond à la position d'arriver. On vérifie si elle est bien sur la grille, et si ce n'est pas une case virtuelle
            // (une case qui ferait partie de la grille mais pas de la surface jouable du vaisseau).
            // On vérifie si elle est walkable (qu'elle ne soit pas déjà occupée par une autre unité, joueur ou ennemi).
            // Si oui, on bouge l'unité en utilisant la même méthode déplacement sur la grille que le joueur
            // (cf. commentaires dans le script UnitMovePathfinding)
            // Le booléen isValidPosition est set à vrai, sinon il reste faux.
            // On vérifie, la valeur du booléen. S'il est vrai, l'opération s'est déroulée sans encombre.
            // S'il est toujours faux, c'est que l'opération n'était pas possible : soit une autre unité le bloquait
            // soit il est bloqué par les limites de la surface jouable (contre un mur).
            // Dans ce cas-là, il est possible de lui infliger un dégât si l'idée te plait (comme les collisions dans Into The Breach).
            // Pour l'instant ce n'est pas implémenté, mais c'est facilement faisable en 3/4 lignes.
            PathNode node = pathfinding.GetNodeWithCoords(targetEndPos.x, targetEndPos.y);
            Debug.Log(node.x + ", " + node.y);
            bool isValidPosition = false;

            if (node != null)
            {
                if (!node.isVirtual)
                {
                    if (!node.isContainingUnit)
                    {
                        if (node.isWalkable)
                        {
                            target.GetComponent<UnitMovePathfinding>().SetVelocity(new Vector3(node.x, 0, node.y));
                            isValidPosition = true;
                        }
                    }
                }
            }
            Debug.Log(node.isContainingUnit +", " + node.unit + ", " + node.isWalkable);
            if (!isValidPosition)
            {
                Debug.Log("Degats");
            }

            playerManager.UpdateFightUI();
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class PullAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // C'est l'action pour que le joueur attire un ennemi.                                        //
        //                                                                                            //
        // Basiquement, c'est quasi EXACTEMENT la même logique que pour PushAction juste au-dessus    //
        // alors réfères-toi aux commentaires de PushAction.                                          //
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public PullAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
        }

        public override void SelectActionTarget()
        {
            List<GameObject> list = new List<GameObject>();
            list = playerController.GetValidTarget(false, true, playerManager.actionRange, playerManager.playerNode);

            if (list != null)
            {
                rme.parentRM.gameObject.SetActive(false);
                VirtualMouseManager.instance.Enable(VirtualMouseManager.TypeToSelect.Unit, list, rme.action);
            }
        }

        public override void ExecuteAction(GameObject target)
        {
            Vector2Int targetStartPos = new Vector2Int(Mathf.RoundToInt(target.transform.position.x), Mathf.RoundToInt(target.transform.position.z));
            Vector2Int targetEndPos = Vector2Int.zero;

            if (targetStartPos.x == Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y < Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(0, 1);
            }
            else if (targetStartPos.x == Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y > Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(0, -1);
            }
            else if (targetStartPos.x < Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y == Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(1, 0);
            }
            else if (targetStartPos.x > Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y == Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(-1, 0);
            }

            PathNode node = pathfinding.GetNodeWithCoords(targetEndPos.x, targetEndPos.y);
            bool isValidPosition = false;

            if (node != null)
            {
                if (!node.isVirtual)
                {
                    if (!node.isContainingUnit)
                    {
                        if (node.isWalkable)
                        {
                            target.GetComponent<UnitMovePathfinding>().SetVelocity(new Vector3(node.x, 0, node.y));
                            isValidPosition = true;
                        }
                    }
                }
            }

            if (!isValidPosition)
            {
                Debug.Log("Degats");
            }

            playerManager.UpdateFightUI();
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class PlantAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // C'est l'action qui permet de selectionner l'outil Plante dans le mode Farm lorsque le      //
        // joueur a fait son choix dans le sous-menu sur quelle plante il aimerait instantier.        //
        // Comme pour toutes les actions du mode Farm, la fonction SelectActionTarget n'est plus      //
        // utilisée. Seule la fonction ExecuteAction est utilisée.                                    //
        //                                                                                            //
        // Le type de plante est par l'index du bouton UI dans le sous-menu radial.                   //
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public PlantAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
        }

        public override void SelectActionTarget()
        {
            
        }

        public override void ExecuteAction(GameObject target)
        {
            playerManager.actionState = PlayerManager.ActionState.Plant;
            playerManager.objectPrefab = playerManager.inventory.plantsList[rme.itemIndex - 1].Prefab;

            rme.parentRM.gameObject.SetActive(false);
            rme.parentRM.upRM.gameObject.SetActive(false);
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class ObjectAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // EXACTEMENT la même logique que pour les plantes, mais pour les objets.                     //
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public ObjectAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;

            rme.parentRM.gameObject.SetActive(false);
            rme.parentRM.upRM.gameObject.SetActive(false);
        }

        public override void SelectActionTarget()
        {
            
        }

        public override void ExecuteAction(GameObject target)
        {
            playerManager.actionState = PlayerManager.ActionState.Object;
            //playerManager.objectPrefab = playerManager.inventory.objectsList[rme.itemIndex - 1].Prefab;
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class CollectAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // Rien de particulier à part set le bon ActionState au PlayerManager.                        //
        ////////////////////////////////////////////////////////////////////////////////////////////////
        
        public CollectAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
        }

        public override void SelectActionTarget()
        {
            
        }

        public override void ExecuteAction(GameObject target)
        {
            playerManager.actionState = PlayerManager.ActionState.Collect;

            rme.parentRM.gameObject.SetActive(false);
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class WaterAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // Rien de particulier à part set le bon ActionState au PlayerManager.                        //
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public WaterAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
        }

        public override void SelectActionTarget()
        {
            
        }

        public override void ExecuteAction(GameObject target)
        {
            playerManager.actionState = PlayerManager.ActionState.Water;

            rme.parentRM.gameObject.SetActive(false);
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class SunAction : MenuAction
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // Rien de particulier à part set le bon ActionState au PlayerManager.                        //
        ////////////////////////////////////////////////////////////////////////////////////////////////

        public SunAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
        }

        public override void SelectActionTarget()
        {
            
        }

        public override void ExecuteAction(GameObject target)
        {
            playerManager.actionState = PlayerManager.ActionState.Sun;

            rme.parentRM.gameObject.SetActive(false);
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }
}
