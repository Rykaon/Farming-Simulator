using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;
using UnityEngine.SceneManagement;

public class UnitGridSystem : MonoBehaviour
{
    [SerializeField] public Team team;              //OK
    public State state;                             //OK
    public State previousState;                     //OK
    public Turn turn;                               //OK

    public GridSystem gridSystem;                   //OK
    private Pathfinding pathfinding;              //OK
    public UnitMovePathfinding movePathfinding;     //OK
    public int movePoints;                          //OK
    public int actionPoints;                        //OK
    public int remainingMovePoints;                 //OK
    public int remainingActionPoints;               //OK

    public GameObject[] implants;                   //OK

    public GameObject ui;                           //OK
    public UISystem uiSystem;
    public PlayerController_MAD playerController;

    public bool isOverheat = false;
    public bool isSpiderWeb = false;
    public bool hasSpiderWebToBeRemove = false;
    public GameObject spiderWeb;
    public GameObject energeticParticlePrefab;
    public List<GameObject> energeticLinkedUnits;
    public List<GameObject> energeticParticles;

    public int movePointsToMove = 0;

    public enum Team
    {
        Blue,
        Orange
    }

    public enum State
    {
        WorldMode,
        NexusInfo,
        OtherInfo,
        SelfInfo,
        Repair,
        MoveUI,
        ActionUI,
        MoveWorld,
        ActionWorld,
        HitUI,
        PopUp
    }

    public enum Turn
    {
        Playing,
        Waiting
    }

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        string sceneName = SceneManager.GetActiveScene().name;
        string name = "";
        for (int i = 0; i < 5; ++i)
        {
            name = name + sceneName[i];
        }

        if (name == "Arena")
        {
            Init();
        }
    }

    public void Init()
    {
        gridSystem = Camera.main.GetComponent<GridSystem>();
        pathfinding = Pathfinding.instance;
        movePathfinding = transform.GetComponent<UnitMovePathfinding>();
        movePoints = 3;
        actionPoints = 3;

        StartCoroutine(InitializeRemainingPoints());

        turn = Turn.Waiting;
    }

    IEnumerator InitializeRemainingPoints()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        remainingMovePoints = movePoints;
        remainingActionPoints = actionPoints;
    }

    public IEnumerator ChangeStateWithDelay(State state)
    {
        yield return new WaitForSecondsRealtime(0.15f);
        ChangeState(state);
    }

    public void ChangeState(State state)
    {
        previousState = this.state;
        this.state = state;

        Debug.Log(state);

        uiSystem.currentMode.SetActive(false);

        if (state == UnitGridSystem.State.WorldMode)
        {
            uiSystem.currentMode = uiSystem.worldMode;
            //playerController.ChangeCursor(playerController.cursorAim);
        }
        else if (state == UnitGridSystem.State.NexusInfo)
        {
            uiSystem.currentMode = uiSystem.nexusMode;
            playerController.ChangeCursor(playerController.cursorArrow);
        }
        else if (state == UnitGridSystem.State.OtherInfo || state == UnitGridSystem.State.PopUp)
        {
            uiSystem.currentMode = uiSystem.otherMode;
            playerController.ChangeCursor(playerController.cursorArrow);
        }
        else if (state == UnitGridSystem.State.SelfInfo || state == UnitGridSystem.State.Repair || state == UnitGridSystem.State.HitUI)
        {
            uiSystem.currentMode = uiSystem.selfMode;
            playerController.ChangeCursor(playerController.cursorArrow);
        }
        else if (state == UnitGridSystem.State.ActionUI || state == UnitGridSystem.State.MoveUI)
        {
            uiSystem.currentMode = uiSystem.abilityUIMode;
            playerController.ChangeCursor(playerController.cursorArrow);
        }
        else if (state == UnitGridSystem.State.ActionWorld || state == UnitGridSystem.State.MoveWorld)
        {
            uiSystem.currentMode = uiSystem.abilityWorldMode;
            playerController.ChangeCursor(playerController.cursorAim);
        }

        uiSystem.currentMode.SetActive(true);

        for (int i = 0; i < uiSystem.currentMode.transform.childCount; ++i)
        {
            uiSystem.currentMode.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }
    }

    public IEnumerator Teleport(Vector3 targetPos)
    {
        Vector3 startingScale = transform.localScale;
        Debug.Log(transform.localScale);
        float scaleFactor = 0.01f;
        for (; ; )
        {
            transform.localScale = transform.localScale - new Vector3(scaleFactor, scaleFactor, scaleFactor);
            yield return new WaitForSeconds(0.000000001f);

            if (transform.localScale.x <= Vector3.zero.x)
            {
                transform.localScale = Vector3.zero;
                break;
            }
        }

        pathfinding.GetNodeWithCoords((int)MathF.Round(transform.position.x), (int)Mathf.Round(transform.position.z)).SetIsContainingUnit(false, null);
        transform.position = targetPos;
        pathfinding.GetNodeWithCoords((int)MathF.Round(transform.position.x), (int)Mathf.Round(transform.position.z)).SetIsContainingUnit(true, transform.gameObject);

        for (; ; )
        {
            transform.localScale = transform.localScale + new Vector3(scaleFactor, scaleFactor, scaleFactor);
            yield return new WaitForSeconds(0.0001f);

            if (transform.localScale.x >= startingScale.x)
            {
                transform.localScale = startingScale;
                break;
            }
        }

        gridSystem.SetEnergeticTower(team);
        gridSystem.ResetTileOutline();
    }

    public void HoverMoveAbility()
    {
        gridSystem.ResetTileOutline();
        /*switch (moveData.type)
        {
            case MoveData.Type.Area:
                GetMaxAreaRangeMovePath(moveData);
                break;

            case MoveData.Type.Linear:
                GetMaxLinearRangeMovePath(moveData);
                break;
        }*/
    }

    public bool DoMoveAbility(/*MoveData moveData*/)
    {
        bool isValidPosition = false;
        /*if (moveData.cost <= remainingMovePoints)
        {
            if (playerController.GetMouseWorldPosition() != new Vector3(-1, -1, -1))
            {
                pathfinding.GetGrid().GetXY(playerController.GetMouseWorldPosition(), out int x, out int y);
                if (pathfinding.GetNodeWithCoords(x, y).isWalkable && pathfinding.GetNodeWithCoords(x, y).isValidMovePosition)
                {
                    if (moveData.abilityName != "Téléporteur")
                    {
                        Vector3 targetPosition = new Vector3(x, 0, y);
                        movePathfinding.SetVelocity(targetPosition, moveData, () => { });
                    }
                    else
                    {
                        Vector3 targetPosition = new Vector3(x, transform.position.y, y);
                        StartCoroutine(Teleport(targetPosition));
                    }
                    
                    remainingMovePoints -= moveData.cost;
                    isValidPosition = true;
                }
            }
        }*/
        return isValidPosition;
    }

    public void GetMaxAreaRangeMovePath(/*MoveData moveData*/)
    {
        pathfinding.GetGrid().GetXY(GetUnitPosition(), out int unitX, out int unitY);

        /*if (moveData.cost <= remainingMovePoints)
        {
            for (int i = unitX - moveData.range; i <= unitX + moveData.range; ++i)
            {
                for (int j = unitY - moveData.range; j <= unitY + moveData.range; ++j)
                {
                    if (pathfinding.GetNodeWithCoords(i, j) != null)
                    {
                        if (pathfinding.GetNodeWithCoords(i, j).isWalkable && !pathfinding.GetNodeWithCoords(i, j).isContainingUnit && pathfinding.GetNodeWithCoords(i, j) != pathfinding.GetNodeWithCoords(unitX, unitY))
                        {
                            List<PathNode> pathNodeList = pathfinding.FindAreaPathMove((int)GetUnitPosition().x, (int)GetUnitPosition().z, i, j);
                            if (pathNodeList != null)
                            {
                                movePointsToMove = 0;

                                for (int k = 0; k < pathNodeList.Count; ++k)
                                {
                                    if (moveData.abilityName != "Téléporteur")
                                    {
                                        movePointsToMove += 2;
                                    }
                                    else
                                    {
                                        movePointsToMove += 1;
                                    }
                                }

                                if (movePointsToMove <= moveData.range)
                                {
                                    foreach (PathNode pathNode in pathNodeList)
                                    {
                                        if (pathNode != pathfinding.GetNodeWithCoords(unitX, unitY))
                                        {
                                            pathNode.SetIsValidMovePosition(true);
                                            switch (team)
                                            {
                                                case Team.Orange:
                                                    for (int k = 0; k < gridSystem.tileObject[pathNode.x, pathNode.y].transform.childCount; ++k)
                                                    {
                                                        gridSystem.tileObject[pathNode.x, pathNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                                    }
                                                    break;

                                                case Team.Blue:
                                                    for (int k = 0; k < gridSystem.tileObject[pathNode.x, pathNode.y].transform.childCount; ++k)
                                                    {
                                                        gridSystem.tileObject[pathNode.x, pathNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }*/
    }

    public void GetMaxLinearRangeMovePath(/*MoveData moveData*/)
    {
        pathfinding.GetGrid().GetXY(GetUnitPosition(), out int unitX, out int unitY);

        /*if (moveData.cost <= remainingMovePoints)
        {
            for (int i = unitX - moveData.range; i <= unitX + moveData.range; ++i)
            {
                for (int j = unitY - moveData.range; j <= unitY + moveData.range; ++j)
                {
                    if (i == unitX || j == unitY)
                    {
                        if (pathfinding.GetNodeWithCoords(i, j) != null)
                        {
                            if (pathfinding.GetNodeWithCoords(i, j).isWalkable && !pathfinding.GetNodeWithCoords(i, j).isContainingUnit && pathfinding.GetNodeWithCoords(i, j) != pathfinding.GetNodeWithCoords(unitX, unitY))
                            {
                                List<PathNode> pathNodeList = pathfinding.FindLinearPathMove((int)GetUnitPosition().x, (int)GetUnitPosition().z, i, j);
                                if (pathNodeList != null)
                                {
                                    movePointsToMove = 0;

                                    for (int k = 0; k < pathNodeList.Count; ++k)
                                    {
                                        if (isActiveAndEnabled)
                                        {
                                            movePointsToMove += 2;
                                        }
                                        else
                                        {
                                            movePointsToMove += 1;
                                        }
                                    }

                                    if (movePointsToMove <= moveData.range)
                                    {
                                        foreach (PathNode pathNode in pathNodeList)
                                        {
                                            if (pathNode != pathfinding.GetNodeWithCoords(unitX, unitY))
                                            {
                                                pathNode.SetIsValidMovePosition(true);
                                                switch (team)
                                                {
                                                    case Team.Orange:
                                                        for (int k = 0; k < gridSystem.tileObject[pathNode.x, pathNode.y].transform.childCount; ++k)
                                                        {
                                                            gridSystem.tileObject[pathNode.x, pathNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                                        }
                                                        break;

                                                    case Team.Blue:
                                                        for (int k = 0; k < gridSystem.tileObject[pathNode.x, pathNode.y].transform.childCount; ++k)
                                                        {
                                                            gridSystem.tileObject[pathNode.x, pathNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                                        }
                                                        break;
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
        }*/
    }

    public void HoverActionAbility(/*ActionData actionData*/)
    {
        gridSystem.ResetTileOutline();
        /*switch (actionData.type)
        {
            case ActionData.Type.Area:
                GetMaxAreaRangeActionPath(actionData);
                break;

            case ActionData.Type.Linear:
                GetMaxLinearRangeActionPath(actionData);
                break;

            case ActionData.Type.LinearPath:
                GetMaxLinearStopRangeActionPath(actionData);
                break;
        }*/
    }

    public bool DoActionAbility(/*ActionData actionData*/)
    {
        bool isValidPosition = false;
        /*if (!uiSystem.selectedActionAbility.GetComponent<ActionAbility>().hasBeenUsed)
        {
            if (actionData.isWorld)
            {
                if (actionData.cost <= remainingActionPoints)
                {
                    if (playerController.GetMouseWorldPosition() != new Vector3(-1, -1, -1))
                    {
                        pathfinding.GetGrid().GetXY(playerController.GetMouseWorldPosition(), out int x, out int y);
                        if (pathfinding.GetNodeWithCoords(x, y).isWalkable && pathfinding.GetNodeWithCoords(x, y).isValidMovePosition)
                        {
                            if (pathfinding.GetNodeWithCoords(x, y).isContainingUnit)
                            {
                                GameObject unit = pathfinding.GetNodeWithCoords(x, y).unit;
                                if (actionData.isLocal)
                                {
                                    uiSystem.SetModeToHitUI(true, true, pathfinding.GetNodeWithCoords(x, y).unit, false, null);
                                    gridSystem.ResetTileOutline();
                                    StartCoroutine(ChangeStateWithDelay(State.HitUI));
                                }
                                else
                                {
                                    for (int i = 0; i < implants.Length; ++i)
                                    {
                                        if (pathfinding.GetNodeWithCoords(x, y).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().lifePoints <= actionData.damage)
                                        {
                                            //pathfinding.GetNode(x, y).unit.GetComponent<UnitGridSystem>().implants[3].GetComponent<ImplantSystem>().InflictDamage(actionData.damage);
                                            break;
                                        }
                                        else
                                        {
                                            //pathfinding.GetNode(x, y).unit.GetComponent<UnitGridSystem>().implants[i].GetComponent<ImplantSystem>().InflictDamage(actionData.damage);
                                        }
                                    }
                                    gridSystem.ResetTileOutline();
                                    uiSystem.selectedActionAbility.ApplyAbility(pathfinding.GetNodeWithCoords(x, y).unit, -1, null, pathfinding.GetNodeWithCoords(x, y));
                                    remainingActionPoints -= actionData.cost;
                                }
                                Debug.Log("ACTION DONE ON " + unit.name);
                            }
                            else
                            {
                                uiSystem.selectedActionAbility.ApplyAbility(null, -1, null, pathfinding.GetNodeWithCoords(x, y));
                                gridSystem.ResetTileOutline();
                                remainingActionPoints -= actionData.cost;
                                Debug.Log("ACTION DONE BUT MISSED");
                            }
                            isValidPosition = true;
                        }
                        
                    }
                }
            }
            else if (actionData.cost <= remainingActionPoints)
            {
                uiSystem.selectedActionAbility.ApplyAbility(null, -1, null, null);
                gridSystem.ResetTileOutline();
                remainingActionPoints -= actionData.cost;
                isValidPosition = true;
            }
        }*/
        return isValidPosition;
    }

    public void GetMaxAreaRangeActionPath(/*ActionData actionData*/)
    {
        pathfinding.GetGrid().GetXY(GetUnitPosition(), out int unitX, out int unitY);

        /*if (actionData.cost <= remainingActionPoints)
        {
            for (int i = unitX - actionData.range; i <= unitX + actionData.range; ++i)
            {
                for (int j = unitY - actionData.range; j <= unitY + actionData.range; ++j)
                {
                    if (pathfinding.GetNodeWithCoords(i, j) != null)
                    {
                        if (pathfinding.GetNodeWithCoords(i, j).isWalkable && pathfinding.GetNodeWithCoords(i, j) != pathfinding.GetNodeWithCoords(unitX, unitY))
                        {
                            List<PathNode> pathNodeList = pathfinding.FindAreaPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, i, j);
                            if (pathNodeList != null)
                            {
                                if (pathNodeList.Count <= actionData.range)
                                {
                                    foreach (PathNode pathNode in pathNodeList)
                                    {
                                        if (pathNode != pathfinding.GetNodeWithCoords(unitX, unitY))
                                        {
                                            pathNode.SetIsValidMovePosition(true);
                                            switch (team)
                                            {
                                                case Team.Orange:
                                                    for (int k = 0; k < gridSystem.tileObject[pathNode.x, pathNode.y].transform.childCount; ++k)
                                                    {
                                                        gridSystem.tileObject[pathNode.x, pathNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                                    }
                                                    break;

                                                case Team.Blue:
                                                    for (int k = 0; k < gridSystem.tileObject[pathNode.x, pathNode.y].transform.childCount; ++k)
                                                    {
                                                        gridSystem.tileObject[pathNode.x, pathNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (!pathfinding.GetNodeWithCoords(i, j).isWalkable)
                        {
                            List<PathNode> pathNodeList = pathfinding.FindAreaPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, i, j);
                            if (pathNodeList != null)
                            {
                                if (pathNodeList.Count <= actionData.range)
                                {
                                    foreach (PathNode pathNode in pathNodeList)
                                    {
                                        pathNode.SetIsValidMovePosition(true);
                                        switch (team)
                                        {
                                            case Team.Orange:
                                                for (int k = 0; k < gridSystem.tileObject[i, j].transform.childCount; ++k)
                                                {
                                                    gridSystem.tileObject[i, j].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                                }
                                                break;

                                            case Team.Blue:
                                                for (int k = 0; k < gridSystem.tileObject[i, j].transform.childCount; ++k)
                                                {
                                                    gridSystem.tileObject[i, j].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }*/
    }

    public void GetMaxLinearRangeActionPath(/*ActionData actionData*/)
    {
        pathfinding.GetGrid().GetXY(GetUnitPosition(), out int unitX, out int unitY);
        /*if (actionData.cost <= remainingActionPoints)
        {
            for (int i = unitX - actionData.range; i <= unitX + actionData.range; ++i)
            {
                for (int j = unitY - actionData.range; j <= unitY + actionData.range; ++j)
                {
                    if (i == unitX || j == unitY)
                    {
                        if (pathfinding.GetNodeWithCoords(i, j) != null)
                        {
                            if (pathfinding.GetNodeWithCoords(i, j).isWalkable && pathfinding.GetNodeWithCoords(i, j) != pathfinding.GetNodeWithCoords(unitX, unitY))
                            {
                                List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, i, j);

                                if (pathNodeList != null)
                                {
                                    if (pathNodeList.Count <= actionData.range)
                                    {
                                        foreach (PathNode pathNode in pathNodeList)
                                        {
                                            pathNode.SetIsValidMovePosition(true);
                                            switch (team)
                                            {
                                                case Team.Orange:
                                                    for (int k = 0; k < gridSystem.tileObject[pathNode.x, pathNode.y].transform.childCount; ++k)
                                                    {
                                                        gridSystem.tileObject[pathNode.x, pathNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                                    }
                                                    break;

                                                case Team.Blue:
                                                    for (int k = 0; k < gridSystem.tileObject[pathNode.x, pathNode.y].transform.childCount; ++k)
                                                    {
                                                        gridSystem.tileObject[pathNode.x, pathNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (!pathfinding.GetNodeWithCoords(i, j).isWalkable)
                            {
                                break;
                            }
                            else if (!pathfinding.GetNodeWithCoords(i, j).isWalkable)
                            {
                                List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, i, j);
                                if (pathNodeList != null)
                                {
                                    if (pathNodeList.Count <= actionData.range)
                                    {
                                        foreach (PathNode pathNode in pathNodeList)
                                        {
                                            pathNode.SetIsValidMovePosition(true);
                                        }

                                        switch (team)
                                        {
                                            case Team.Orange:
                                                for (int k = 0; k < gridSystem.tileObject[i, j].transform.childCount; ++k)
                                                {
                                                    gridSystem.tileObject[i, j].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                                }
                                                break;

                                            case Team.Blue:
                                                for (int k = 0; k < gridSystem.tileObject[i, j].transform.childCount; ++k)
                                                {
                                                    gridSystem.tileObject[i, j].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }*/
    }

    public void GetMaxLinearStopRangeActionPath(/*ActionData actionData*/)
    {
        PathNode unitNode = pathfinding.GetNodeWithCoords((int)MathF.Round(transform.position.x), (int)MathF.Round(transform.position.z));
        /*if (actionData.cost <= remainingActionPoints)
        {
            for (int i = 1; i <= actionData.range; ++i)
            {
                PathNode targetNode = pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y + i);
                if (targetNode != null)
                {
                    if (targetNode.isWalkable)
                    {
                        List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, targetNode.x, targetNode.y);

                        if (pathNodeList != null)
                        {
                            if (pathNodeList.Count <= actionData.range)
                            {
                                foreach (PathNode pathNode in pathNodeList)
                                {
                                    pathNode.SetIsValidMovePosition(true);
                                    switch (team)
                                    {
                                        case Team.Orange:
                                            for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                            {
                                                gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                            }
                                            break;

                                        case Team.Blue:
                                            for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                            {
                                                gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (targetNode.isContainingUnit)
                        {
                            break;
                        }
                    }
                    else if (!targetNode.isWalkable)
                    {
                        break;
                    }
                    else if (!targetNode.isWalkable)
                    {
                        List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, targetNode.x, targetNode.y);
                        if (pathNodeList != null)
                        {
                            if (pathNodeList.Count <= actionData.range)
                            {
                                foreach (PathNode pathNode in pathNodeList)
                                {
                                    pathNode.SetIsValidMovePosition(true);
                                }

                                switch (team)
                                {
                                    case Team.Orange:
                                        for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                        {
                                            gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                        }
                                        break;

                                    case Team.Blue:
                                        for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                        {
                                            gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                        }
                                        break;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            for (int i = 1; i <= actionData.range; ++i)
            {
                PathNode targetNode = pathfinding.GetNodeWithCoords(unitNode.x, unitNode.y - i);
                if (targetNode != null)
                {
                    if (targetNode.isWalkable)
                    {
                        List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, targetNode.x, targetNode.y);

                        if (pathNodeList != null)
                        {
                            if (pathNodeList.Count <= actionData.range)
                            {
                                foreach (PathNode pathNode in pathNodeList)
                                {
                                    pathNode.SetIsValidMovePosition(true);
                                    switch (team)
                                    {
                                        case Team.Orange:
                                            for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                            {
                                                gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                            }
                                            break;

                                        case Team.Blue:
                                            for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                            {
                                                gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (targetNode.isContainingUnit)
                        {
                            break;
                        }
                    }
                    else if (!targetNode.isWalkable)
                    {
                        break;
                    }
                    else if (!targetNode.isWalkable)
                    {
                        List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, targetNode.x, targetNode.y);
                        if (pathNodeList != null)
                        {
                            if (pathNodeList.Count <= actionData.range)
                            {
                                foreach (PathNode pathNode in pathNodeList)
                                {
                                    pathNode.SetIsValidMovePosition(true);
                                }

                                switch (team)
                                {
                                    case Team.Orange:
                                        for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                        {
                                            gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                        }
                                        break;

                                    case Team.Blue:
                                        for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                        {
                                            gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                        }
                                        break;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            for (int i = 1; i <= actionData.range; ++i)
            {
                PathNode targetNode = pathfinding.GetNodeWithCoords(unitNode.x + i, unitNode.y);
                if (targetNode != null)
                {
                    if (targetNode.isWalkable)
                    {
                        List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, targetNode.x, targetNode.y);

                        if (pathNodeList != null)
                        {
                            if (pathNodeList.Count <= actionData.range)
                            {
                                foreach (PathNode pathNode in pathNodeList)
                                {
                                    pathNode.SetIsValidMovePosition(true);
                                    switch (team)
                                    {
                                        case Team.Orange:
                                            for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                            {
                                                gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                            }
                                            break;

                                        case Team.Blue:
                                            for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                            {
                                                gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (targetNode.isContainingUnit)
                        {
                            break;
                        }
                    }
                    else if (!targetNode.isWalkable)
                    {
                        break;
                    }
                    else if (!targetNode.isWalkable)
                    {
                        List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, targetNode.x, targetNode.y);
                        if (pathNodeList != null)
                        {
                            if (pathNodeList.Count <= actionData.range)
                            {
                                foreach (PathNode pathNode in pathNodeList)
                                {
                                    pathNode.SetIsValidMovePosition(true);
                                }

                                switch (team)
                                {
                                    case Team.Orange:
                                        for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                        {
                                            gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                        }
                                        break;

                                    case Team.Blue:
                                        for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                        {
                                            gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                        }
                                        break;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            for (int i = 1; i <= actionData.range; ++i)
            {
                PathNode targetNode = pathfinding.GetNodeWithCoords(unitNode.x - i, unitNode.y);
                if (targetNode != null)
                {
                    if (targetNode.isWalkable)
                    {
                        List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, targetNode.x, targetNode.y);

                        if (pathNodeList != null)
                        {
                            if (pathNodeList.Count <= actionData.range)
                            {
                                foreach (PathNode pathNode in pathNodeList)
                                {
                                    pathNode.SetIsValidMovePosition(true);
                                    switch (team)
                                    {
                                        case Team.Orange:
                                            for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                            {
                                                gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                            }
                                            break;

                                        case Team.Blue:
                                            for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                            {
                                                gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (targetNode.isContainingUnit)
                        {
                            break;
                        }
                    }
                    else if (!targetNode.isWalkable)
                    {
                        break;
                    }
                    else if (!targetNode.isWalkable)
                    {
                        List<PathNode> pathNodeList = pathfinding.FindLinearPathAction((int)GetUnitPosition().x, (int)GetUnitPosition().z, targetNode.x, targetNode.y);
                        if (pathNodeList != null)
                        {
                            if (pathNodeList.Count <= actionData.range)
                            {
                                foreach (PathNode pathNode in pathNodeList)
                                {
                                    pathNode.SetIsValidMovePosition(true);
                                }

                                switch (team)
                                {
                                    case Team.Orange:
                                        for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                        {
                                            gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightOrange;
                                        }
                                        break;

                                    case Team.Blue:
                                        for (int k = 0; k < gridSystem.tileObject[targetNode.x, targetNode.y].transform.childCount; ++k)
                                        {
                                            gridSystem.tileObject[targetNode.x, targetNode.y].transform.GetChild(k).GetComponent<Outline>().OutlineColor = playerController.lightBlue;
                                        }
                                        break;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }*/
    }

    public void SetEnergeticTower()
    {
        PathNode node = pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.position.x), (int)Mathf.Round(transform.position.z));
        
        /*if (implants[1].GetComponent<ActionAbility>().actionAbility != null)
        {
            if (implants[1].GetComponent<ActionAbility>().actionAbility.abilityName == "Tour Énergétique")
            {
                if (energeticLinkedUnits != null)
                {
                    for (int i = 0; i < energeticLinkedUnits.Count; ++i)
                    {
                        energeticLinkedUnits[i].GetComponent<UnitGridSystem>().movePoints -= energeticTower.damage;
                        energeticLinkedUnits[i].GetComponent<UnitGridSystem>().actionPoints -= energeticTower.damage;
                        energeticLinkedUnits[i].GetComponent<UnitGridSystem>().remainingMovePoints -= energeticTower.damage;
                        energeticLinkedUnits[i].GetComponent<UnitGridSystem>().remainingActionPoints -= energeticTower.damage;

                        if (energeticLinkedUnits[i].GetComponent<UnitGridSystem>().remainingMovePoints < 0)
                        {
                            energeticLinkedUnits[i].GetComponent<UnitGridSystem>().remainingMovePoints = 0;
                        }

                        if (energeticLinkedUnits[i].GetComponent<UnitGridSystem>().remainingActionPoints < 0)
                        {
                            energeticLinkedUnits[i].GetComponent<UnitGridSystem>().remainingActionPoints = 0;
                        }

                        Destroy(energeticParticles[i]);
                    }

                    energeticLinkedUnits = null;
                }

                energeticLinkedUnits = new List<GameObject>();
                energeticParticles = new List<GameObject>();

                for (int i = -1; i < 2; ++i)
                {
                    for (int j = -1; j < 2; ++j)
                    {
                        if (pathfinding.GetNodeWithCoords(node.x + i, node.y + j) != null)
                        {
                            PathNode otherNode = pathfinding.GetNodeWithCoords(node.x + i, node.y + j);
                            if (otherNode.x == node.x || otherNode.y == node.y)
                            {
                                if (otherNode.isContainingUnit)
                                {
                                    if (otherNode.unit != transform.gameObject && otherNode.unit.GetComponent<UnitGridSystem>().team == team)
                                    {
                                        energeticLinkedUnits.Add(otherNode.unit);

                                        if (otherNode.x == node.x)
                                        {
                                            if (otherNode.y > node.y)
                                            {
                                                Vector3 euler = new Vector3(0, 0, 0);
                                                energeticParticles.Add(Instantiate(energeticParticlePrefab, new Vector3(node.x, 0, node.y), Quaternion.Euler(euler)));
                                            }
                                            else if (otherNode.y < node.y)
                                            {
                                                Vector3 euler = new Vector3(0, 180, 0);
                                                energeticParticles.Add(Instantiate(energeticParticlePrefab, new Vector3(node.x, 0, node.y), Quaternion.Euler(euler)));
                                            }
                                        }
                                        else if (otherNode.y == node.y)
                                        {
                                            if (otherNode.x > node.x)
                                            {
                                                Vector3 euler = new Vector3(0, 90, 0);
                                                energeticParticles.Add(Instantiate(energeticParticlePrefab, new Vector3(node.x, 0, node.y), Quaternion.Euler(euler)));
                                            }
                                            else if (otherNode.x < node.x)
                                            {
                                                Vector3 euler = new Vector3(0, 270, 0);
                                                energeticParticles.Add(Instantiate(energeticParticlePrefab, new Vector3(node.x, 0, node.y), Quaternion.Euler(euler)));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (energeticLinkedUnits.Count > 0)
                {
                    for (int i = 0; i < energeticLinkedUnits.Count; ++i)
                    {
                        energeticLinkedUnits[i].GetComponent<UnitGridSystem>().movePoints += energeticTower.damage;
                        energeticLinkedUnits[i].GetComponent<UnitGridSystem>().actionPoints += energeticTower.damage;
                        energeticLinkedUnits[i].GetComponent<UnitGridSystem>().remainingMovePoints += energeticTower.damage;
                        energeticLinkedUnits[i].GetComponent<UnitGridSystem>().remainingActionPoints += energeticTower.damage;
                    }
                }
            }
        }*/
    }

    private Vector3 GetUnitPosition()
    {
        Vector3 unitPosition = new Vector3((int)Mathf.Round(transform.position.x), (int)Mathf.Round(transform.position.y), (int)Mathf.Round(transform.position.z));
        return unitPosition;
    }

    public void GetListOfAbilities()
    {
        /*moveDataList = new List<MoveData>();
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).TryGetComponent<MoveAbility>(out MoveAbility ability))
            {
                if (ability.moveAbility != null && ability.moveAbility.isAbility)
                {
                    moveDataList.Add(ability.moveAbility);
                    moveAbilityList.Add(ability);
                }
            }
        }

        actionDataList = new List<ActionData>();
        actionAbilityList = new List<ActionAbility>();
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (transform.GetChild(i).TryGetComponent<ActionAbility>(out ActionAbility ability))
            {
                if (ability.actionAbility != null && ability.actionAbility.isAbility)
                {
                    actionDataList.Add(ability.actionAbility);
                    actionAbilityList.Add(ability);
                }
            }
        }*/
    }
}