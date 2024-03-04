using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using Ink.Runtime;

public class UnitMovePathfinding : MonoBehaviour, IUnitMove
{
    /////////////////////////////////////////////////////////////////////////////////////////////////
    // Ce script ne fait pas grand chose en soi mais la logique peut être un peu difficile         //
    // à capter au premier abord (même moi je dois me reconcentrer dessus quand j'y ai pas         //
    // touché depuis un moment).                                                                   //
    //                                                                                             //
    // Toutes les unités qui doivent pouvoir bouger sur la grille pendant les combats doivent      //
    // avoir impérativement deux scripts attachés à eux. Ce script là et le script UnitMove.       //
    // Ces deux scripts héritent de la même interface IUnitMove pour implémenter les mêmes         //
    // fonctions (la même logique que les scriptableObject PlantItem, SeedItem et ObjectItem       //
    // qui héritent tous de la même interface).                                                    //
    //                                                                                             //
    // Les deux scripts implémentent donc la même fonction SetVelocity avec les mêmes paramètres.  //
    // Normalement tu n'as besoin de communiquer qu'avec celui-là en appelant la fonction          //
    // SetVelocity, la fonction équivalente dans UnitMove est appelée automatiquement.             //
    //                                                                                             //
    // SetVelocity prends comme paramètre un Vector3 qui correspond à la case sur laquelle on      //
    // veut le déplacer sur la grille. La liste nodeList va retourner tous les nodes (les cases)   //
    // qui vont constituer le chemin de sa position actuelle jusqu'à la position souhaitée.        //
    // Si elle n'est pas nulle, on calcule la distance entre la position actuelle et la première   // 
    // case du chemin. Tant que la position actuelle n'est pas la première case, on donne          //
    // automatiquement à UnitMove la position de cette case. L'Update() de UnitMove va             //
    // se charger de translate l'unité jusqu'à cette case.                                         //
    //                                                                                             //
    // Une fois atteinte, on passe à la prochaine case du chemin, jusqu'à ce que l'unité arrive à  //
    // la position souhaitée à la base. Et c'est tout. C'est beaucoup de dialogue avec le          //
    // pathfinding, mais t'occupes pas de ça.                                                      //
    //                                                                                             //
    // Ce qui va peut-être t'intéresser, c'est la variable moveSpeed définit dans le script        //
    // UnitMove que tu peux modifier directement depuis l'inspecteur qui va modifier la vitesse    //
    // de translation et l'update de la rotation de l'objet pour qu'il s'oriente dans la direction //
    // de la case pour avoir un mouvement naturel.                                                 //
    //                                                                                             //
    // J'imagine que tu vas potentiellement vouloir modifier le comportement de la rotation        //
    // dans le cas ou on repousse/attire un ennemi, normalement je pense que tu sauras le faire    //
    // sans trop de problèmes.                                                                     //
    //                                                                                             //
    // Juste un rappel, si tu veux modifier les paramètres des fonctions SetVelocity pour          //
    // changer ça, les deux fonctions (dans UnitMovePathfinding et UnitMove) DOIVENT               //
    // avoir les mêmes paramètres. Et l'interface IUnitMove doit aussi implémenter cette fonction  //
    // avec les mêmes paramètres. L'interface IUnitMove est dans son propre script                 //
    // (Assets > Scripts > Grid > IUnitMove).                                                      //
    /////////////////////////////////////////////////////////////////////////////////////////////////
    
    Pathfinding pathfinding;
    [SerializeField] private PathNode pathNode;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private UnitMove unitMove;
    [SerializeField] private float reachedPathPositionDistance;
    private List<PathNode> nodeList;
    private int pathIndex = -1;

    private void Start()
    {
        pathfinding = PlayerManager.instance.pathfinding;
    }

    public void SetVelocity(Vector3 targetPos)
    {
        if (transform.gameObject.tag == "Player")
        {
            pathNode = playerManager.playerNode;
        }
        else
        {
            pathNode = transform.GetComponent<UnitManager>().unitNode;
        }

        nodeList = pathfinding.FindAreaPathMove(pathNode.x, pathNode.y, (int)Mathf.Round(targetPos.x), (int)Mathf.Round(targetPos.z));

        if (nodeList == null)
        {
            pathIndex = -1;
            return;
        }
        else
        {
            PathNode startNode = pathNode;
            startNode.isContainingUnit = false;
            startNode.unit = null;
            startNode.isWalkable = true;
            PathNode endNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(targetPos.x), (int)Mathf.Round(targetPos.z));
            endNode.isContainingUnit = true;
            endNode.unit = transform.gameObject;
            endNode.isWalkable = false;

            if (transform.tag == "Player")
            {
                playerManager.playerNode = endNode;
            }
            else
            {
                transform.GetComponent<UnitManager>().unitNode = endNode;
            }

            Debug.Log("NODES = " + nodeList.Count);

            for (int i = 0; i < nodeList.Count - 1; i++)
            {
                if (transform.tag == "Player")
                {
                    playerManager.currentDistanceMoved++;

                    if (playerManager.currentDistanceMoved == 3 || playerManager.currentDistanceMoved == 6 || playerManager.currentDistanceMoved == 9)
                    {
                        Debug.Log("kgjkseg");
                        playerManager.currentActionsPoints--;
                    }
                }
            }

            pathIndex = 0;
        }

        
    }

    private void Update()
    {
        if (pathIndex != -1)
        {
            PathNode nextNode = nodeList[pathIndex];
            Vector3 moveVelocity = new Vector3((nextNode.x - transform.position.x), 0, (nextNode.y - transform.position.z)).normalized;
            unitMove.SetVelocity(moveVelocity);
            Vector3 unitHeight = new Vector3(transform.position.x, 0, transform.position.z);

            if (Vector3.Distance(unitHeight, new Vector3(nextNode.x, 0, nextNode.y)) <= reachedPathPositionDistance)
            {
                ++pathIndex;
                if (pathIndex >= nodeList.Count)
                {
                    pathIndex = -1;
                }
            }
        }
        else
        {
            unitMove.SetVelocity(Vector3.zero);
        }
    }
}
