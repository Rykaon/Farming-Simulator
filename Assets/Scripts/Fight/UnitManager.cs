using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private Pathfinding pathfinding;

    public PathNode unitNode;
    public PathNode targetNode;

    [SerializeField] public int range;
    [SerializeField] public int maxActionPoints;
    public int currentActionPoints;
    public bool isActive = false;
    public bool hasPlay = false;
    public bool isPlaying = false;
    private bool hasReaction = false;
    private float elapsedTime = 0f;
    float animTime = 0;
    public bool isReactionAnimLonger = false;
    public int index = 0;

    public GameObject target;

    private void Awake()
    {
        pathfinding = PlayerManager.instance.pathfinding;
    }

    private void Start()
    {
        unitNode = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        currentActionPoints = maxActionPoints;
        target = null;
    }

    private IEnumerator ExecuteAction()
    {
        isPlaying = true;
        elapsedTime = 0f;
        animTime = 1;
        hasReaction = true;

        bool isReactionSet = false;
        bool hasReactionSet = false;
        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= animTime / 2)
            {
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

    public IEnumerator ResolveReaction()
    {
        float elapsedTime = 0f;
        float animTime = 1;

        if (animTime > (this.animTime - this.elapsedTime))
        {
            isReactionAnimLonger = true;
        }

        PlayerManager.instance.plantList.Remove(target);
        Destroy(target);
        targetNode.isSeeded = false;
        targetNode.isPlant = false;
        targetNode.plant = null;

        for (int i = 0; i < PlayerManager.instance.plantList.Count; i++)
        {
            PlayerManager.instance.plantList[i].GetComponent<PlantManager>().index = PlayerManager.instance.plantList.Count - 1;
        }

        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
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
                Debug.Log(pathList.Count);
                //Debug.Log(pathList[pathList.Count - 1]);
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
                Debug.Log("unitCount = " + index + " // plantList.Count = " + i + " // currentRange = " + currentRange);
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

        Debug.Log("Loose Condition");
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
