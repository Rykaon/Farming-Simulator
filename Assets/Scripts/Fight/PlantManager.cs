using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public PathNode plantNode;
    public PathNode targetNode;
    private GameObject target;
    private List<GameObject> targetList = new List<GameObject>();
    private Pathfinding pathfinding;
    public float range;
    public float maxRange;

    public bool isActive = false;
    public bool hasPlay = false;
    public bool isPlaying = false;
    private bool hasReaction = false;
    private float elapsedTime = 0f;
    float animTime = 0;
    public bool isReactionAnimLonger = false;

    public int index = 0;

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
        plantNode = pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    }

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

    private IEnumerator ExecuteAction()
    {
        isPlaying = true;
        elapsedTime = 0f;
        animTime = 0;

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

                animTime = 1;
                break;

            case Type.Move:
                if (plantNode.isContainingUnit)
                {
                    target = plantNode.unit;
                }

                animTime = 1;
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

                animTime = 1;
                break;
        }

        if (target != null)
        {
            hasReaction = true;
        }

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
            hasPlay = true;
        }
    }

    public IEnumerator ResolveReaction()
    {
        float elapsedTime = 0f;
        float animTime = 0;

        switch (type)
        {
            case Type.Attack:
                animTime = 1;

                target.GetComponent<UnitManager>().unitNode.isContainingUnit = false;
                target.GetComponent<UnitManager>().unitNode.unit = null;
                target.GetComponent<UnitManager>().unitNode.isWalkable = true;
                PlayerManager.instance.unitList.Remove(target);
                Destroy(target);

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

        if (animTime > (this.animTime - this.elapsedTime))
        {
            isReactionAnimLonger = true;
        }

        while (elapsedTime < animTime)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

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
