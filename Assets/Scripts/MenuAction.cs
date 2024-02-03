using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuAction
{
    private RadialMenuElement rme;
    private PlayerController_Fight playerController;
    private PlayerManager playerManager;
    private Pathfinding pathfinding;

    public abstract void SelectActionTarget();
    public abstract void ExecuteAction(GameObject target);
    public abstract void UndoAction();

    public class SubmenuAction : MenuAction
    {
        SubmenuAction action;

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
        MoveAction action;

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
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class PushAction : MenuAction
    {
        PushAction action;

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
            Vector2Int targetStartPos = new Vector2Int(Mathf.RoundToInt(target.transform.position.x), Mathf.RoundToInt(target.transform.position.z));
            Vector2Int targetEndPos = Vector2Int.zero;

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
                Debug.Log("yo");
                targetEndPos = targetStartPos + new Vector2Int(-1, 0);
            }
            else if (targetStartPos.x > Mathf.RoundToInt(playerController.transform.position.x) && targetStartPos.y == Mathf.RoundToInt(playerController.transform.position.z))
            {
                targetEndPos = targetStartPos + new Vector2Int(1, 0);
            }

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
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class PullAction : MenuAction
    {
        PullAction action;

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
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class PlantAction : MenuAction
    {
        PlantAction action;

        public enum Type
        {
            Default,
            Attack,
            Move,
            Boost
        }

        public Type type;

        public PlantAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
            type = Type.Default;
            switch (rme.itemIndex)
            {
                case 1:
                    type = Type.Attack;
                    break;

                case 2:
                    type = Type.Move;
                    break;

                case 3:
                    type = Type.Boost;
                    break;
            }
        }

        public override void SelectActionTarget()
        {
            
        }

        public override void ExecuteAction(GameObject target)
        {
            playerManager.actionState = PlayerManager.ActionState.Plant;
            switch (type)//Définir quel item est selectionné
            {
                case Type.Attack:
                    playerManager.plant = playerManager.plantPrefab_1;
                    break;

                case Type.Move:
                    playerManager.plant = playerManager.plantPrefab_2;
                    break;

                case Type.Boost:
                    playerManager.plant = playerManager.plantPrefab_3;
                    break;
            }

            rme.parentRM.gameObject.SetActive(false);
            rme.parentRM.upRM.gameObject.SetActive(false);
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class PlaceAction : MenuAction
    {
        PlaceAction action;

        public enum Type
        {
            Default,
            Attack,
            Move,
            Boost
        }

        public Type type;

        public PlaceAction(RadialMenuElement rme)
        {
            this.rme = rme;
            playerController = PlayerController_Fight.instance;
            playerManager = PlayerManager.instance;
            pathfinding = Pathfinding.instance;
            type = Type.Default;
            switch (rme.itemIndex)
            {
                case 1:
                    type = Type.Attack;
                    break;

                case 2:
                    type = Type.Move;
                    break;

                case 3:
                    type = Type.Boost;
                    break;
            }

            rme.parentRM.gameObject.SetActive(false);
            rme.parentRM.upRM.gameObject.SetActive(false);
        }

        public override void SelectActionTarget()
        {
            
        }

        public override void ExecuteAction(GameObject target)
        {
            playerManager.actionState = PlayerManager.ActionState.Object;
            switch (type)//Définir quel item est selectionné
            {
                case Type.Attack:
                    //playerManager.object = 
                    break;

                case Type.Move:
                    break;

                case Type.Boost:
                    break;
            }
        }

        public override void UndoAction()
        {
            // Implement undo logic
        }
    }

    public class CollectAction : MenuAction
    {
        CollectAction action;

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
        WaterAction action;

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
        SunAction action;

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
