using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ImplantSystem : MonoBehaviour
{
    GridSystem gridSystem;
    private Pathfinding pathfinding;
    public int lifePoints;

    public GameObject floatingText;

    private void Awake()
    {
        gridSystem = Camera.main.GetComponent<GridSystem>();
    }

    void Start()
    {
        pathfinding = Pathfinding.instance;
        lifePoints = 100;
    }

    public IEnumerator InstantiateFloatingText(int damage)
    {
        float ratio = 0.35f;
        float x = Random.Range(transform.position.x - ratio, transform.position.x + ratio);
        float z = Random.Range(transform.position.z - ratio, transform.position.z + ratio);
        GameObject newFloatingText = Instantiate(floatingText, new Vector3(x, 1f, z), Quaternion.identity);
        newFloatingText.transform.LookAt(new Vector3(Camera.main.transform.GetChild(0).position.x, newFloatingText.transform.position.y, Camera.main.transform.GetChild(0).position.z));
        newFloatingText.transform.rotation = Quaternion.Euler(newFloatingText.transform.eulerAngles.x, newFloatingText.transform.eulerAngles.y - 180, newFloatingText.transform.eulerAngles.z);
        newFloatingText.GetComponent<TextMeshPro>().text = "-" + damage.ToString();
        newFloatingText.transform.SetParent(transform);
        yield return new WaitForSecondsRealtime(2f);
        Destroy(newFloatingText);
    }

    public void InflictDamage(int damage)
    {
        if (lifePoints > 0)
        {
            lifePoints -= damage;
            StartCoroutine(InstantiateFloatingText(damage));

            if (lifePoints <= 0)
            {
                lifePoints = 0;

                if (transform.gameObject == transform.parent.GetChild(3).gameObject)
                {
                    int index = -1;
                    int energeticIndex = -1;
                    bool isDead = false;
                    switch (transform.parent.GetComponent<UnitGridSystem>().team)
                    {
                        case UnitGridSystem.Team.Orange:
                            foreach (GameObject unit in gridSystem.unitListPlayerOne)
                            {
                                if (unit.GetComponent<UnitGridSystem>().energeticLinkedUnits.Count > 0)
                                {
                                    if (unit == transform.parent.gameObject)
                                    {
                                        for (int i = 0; i < unit.GetComponent<UnitGridSystem>().energeticLinkedUnits.Count; ++i)
                                        {
                                            unit.GetComponent<UnitGridSystem>().energeticLinkedUnits.RemoveAt(i);
                                            Destroy(unit.GetComponent<UnitGridSystem>().energeticParticles[i]);
                                        }
                                    }
                                    else
                                    {
                                        foreach (GameObject energeticUnit in unit.GetComponent<UnitGridSystem>().energeticLinkedUnits)
                                        {
                                            if (energeticUnit == transform.parent.gameObject)
                                            {
                                                energeticIndex = gridSystem.GetIndexOfObjectInList(energeticUnit, unit.GetComponent<UnitGridSystem>().energeticLinkedUnits);
                                                unit.GetComponent<UnitGridSystem>().energeticLinkedUnits.RemoveAt(energeticIndex);
                                                Destroy(unit.GetComponent<UnitGridSystem>().energeticParticles[energeticIndex]);
                                            }
                                        }
                                    }
                                }
                            }

                            index = gridSystem.GetIndexOfObjectInList(transform.parent.gameObject, gridSystem.unitListPlayerOne);
                            gridSystem.unitListPlayerOne.RemoveAt(index);

                            if (gridSystem.unitListPlayerOne.Count <= 0)
                            {
                                isDead = true;
                            }

                            if (isDead)
                            {
                                Debug.Log("VICTOIRE DES BLEUS");
                            }
                            break;

                        case UnitGridSystem.Team.Blue:
                            foreach (GameObject unit in gridSystem.unitListPlayerTwo)
                            {
                                if (unit.GetComponent<UnitGridSystem>().energeticLinkedUnits.Count > 0)
                                {
                                    if (unit == transform.parent.gameObject)
                                    {
                                        for (int i = 0; i < unit.GetComponent<UnitGridSystem>().energeticLinkedUnits.Count; ++i)
                                        {
                                            unit.GetComponent<UnitGridSystem>().energeticLinkedUnits.RemoveAt(i);
                                            Destroy(unit.GetComponent<UnitGridSystem>().energeticParticles[i]);
                                        }
                                    }
                                    else
                                    {
                                        foreach (GameObject energeticUnit in unit.GetComponent<UnitGridSystem>().energeticLinkedUnits)
                                        {
                                            if (energeticUnit == transform.parent.gameObject)
                                            {
                                                energeticIndex = gridSystem.GetIndexOfObjectInList(energeticUnit, unit.GetComponent<UnitGridSystem>().energeticLinkedUnits);
                                                unit.GetComponent<UnitGridSystem>().energeticLinkedUnits.RemoveAt(energeticIndex);
                                                Destroy(unit.GetComponent<UnitGridSystem>().energeticParticles[energeticIndex]);
                                            }
                                        }
                                    }
                                }
                            }

                            index = gridSystem.GetIndexOfObjectInList(transform.parent.gameObject, gridSystem.unitListPlayerTwo);
                            gridSystem.unitListPlayerTwo.RemoveAt(index);

                            if (gridSystem.unitListPlayerTwo.Count <= 0)
                            {
                                isDead = true;
                            }

                            if (isDead)
                            {
                                Debug.Log("VICTOIRE DES ORANGES");
                            }
                            break;
                    }

                    pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x), (int)Mathf.Round(transform.parent.position.z)).SetIsContainingUnit(false, null);
                    Destroy(transform.parent.gameObject);
                    Debug.Log(transform.parent.gameObject.name + " is DESTROOOOOY !");
                }
                else
                {
                    DisableImplant();
                }
            }
        }
    }

    public void DisableImplant()
    {
        bool hasImplantBeenInstantiated = false;

        if (hasImplantBeenInstantiated)
        {
            hasImplantBeenInstantiated = false;
            //break;
        }

        for (int i = 1; i < pathfinding.GetGrid().GetHeight(); ++i)
        {
            if (hasImplantBeenInstantiated)
            {
                hasImplantBeenInstantiated = false;
                break;
            }

            PathNode node = pathfinding.GetGrid().GetGridObject((int)Mathf.Round(transform.parent.position.x), (int)Mathf.Round(transform.parent.position.z));

            for (int x = -1; x < 2; ++x)
            {
                if (hasImplantBeenInstantiated)
                {
                    hasImplantBeenInstantiated = false;
                    break;
                }

                for (int y = -1; y < 2; ++x)
                {
                    if (hasImplantBeenInstantiated)
                    {
                        hasImplantBeenInstantiated = false;
                        break;
                    }

                    PathNode otherNode = null;
                    int newY = 0;
                    if (y == -1)
                    {
                        newY = 0;
                    }
                    else if (y == 0)
                    {
                        newY = -1;
                    }
                    else if (y == 1)
                    {
                        newY = 1;
                    }

                    switch (transform.parent.GetComponent<UnitGridSystem>().team)
                    {
                        case UnitGridSystem.Team.Orange:
                            otherNode = pathfinding.GetGrid().GetGridObject(node.x - x, node.y - newY);
                            break;

                        case UnitGridSystem.Team.Blue:
                            otherNode = pathfinding.GetGrid().GetGridObject(node.x + x, node.y - newY);
                            break;
                    }

                    if (otherNode.unit != transform.parent.gameObject)
                    {
                        /*if (!otherNode.isContainingImplant && !otherNode.isContainingUnit)
                        {
                            GameObject clone = Instantiate(transform.gameObject, new Vector3(otherNode.x, 0.6f, otherNode.y), Quaternion.identity);
                            clone.transform.localScale = new Vector3(1, 1, 1);
                            clone.transform.rotation = Quaternion.Euler(90, 0, 0);
                            clone.GetComponent<SpriteRenderer>().sortingOrder = 7;
                            clone.GetComponent<ImplantSystem>().lifePoints = 70;
                            pathfinding.GetGrid().GetGridObject(otherNode.x, otherNode.y).SetIsContainingImplant(true, clone);
                            hasImplantBeenInstantiated = true;
                            if (transform.TryGetComponent<MoveAbility>(out MoveAbility moveAbility))
                            {
                                moveAbility.DisableAbility();
                            }

                            if (transform.TryGetComponent<ActionAbility>(out ActionAbility actionAbility))
                            {
                                actionAbility.DisableAbility();
                            }
                            return;
                        }
                        else
                        {
                            Debug.Log("NO PLACE FOUND TO INSTANCE IMPLANT");
                        }*/
                    }
                }
            }
        }

        

        Vector3 position = new Vector3(-1, -1, -1);
    }

    public void ReplaceImplant(GameObject newImplant)
    {
        /*if (transform.TryGetComponent<MoveAbility>(out MoveAbility moveAbility))
        {
            if (newImplant.TryGetComponent<MoveAbility>(out MoveAbility newMoveAbility))
            {
                moveAbility.ChangeAbility(newMoveAbility.moveAbility);
            }
            else
            {
                Destroy(this);
            }
        }
        else
        {
            if (newImplant.TryGetComponent<MoveAbility>(out MoveAbility newMoveAbility))
            {
                transform.gameObject.AddComponent<MoveAbility>();
                transform.gameObject.GetComponent<MoveAbility>().ChangeAbility(newMoveAbility.moveAbility);
            }
        }

        if (transform.TryGetComponent<ActionAbility>(out ActionAbility actionAbility))
        {
            if (newImplant.TryGetComponent<ActionAbility>(out ActionAbility newActionAbility))
            {
                actionAbility.ChangeAbility(newActionAbility.actionAbility);
            }
            else
            {
                Destroy(this);
            }
        }
        else
        {
            if (newImplant.TryGetComponent<ActionAbility>(out ActionAbility newActionAbility))
            {
                transform.gameObject.AddComponent<ActionAbility>();
                transform.gameObject.GetComponent<ActionAbility>().ChangeAbility(newActionAbility.actionAbility);
            }
        }

        lifePoints = newImplant.GetComponent<ImplantSystem>().lifePoints;
        pathfinding.GetNodeWithCoords((int)Mathf.Round(newImplant.transform.position.x), (int)Mathf.Round(newImplant.transform.position.z)).SetIsContainingImplant(false, null);
        Destroy(newImplant);*/
    }

    public Vector3 GetNodeToInstance()
    {
        List<PathNode> units = new List<PathNode>();
        Vector3 position = new Vector3(-1, -1, -1);

        for (int i = 0; i <= pathfinding.GetGrid().GetWidth(); ++i)
        {
            for (int j = 0; j <= pathfinding.GetGrid().GetHeight(); ++j)
            {
                PathNode thisNode = null;

                if (pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x) + i, (int)Mathf.Round(transform.parent.position.x) + j) != null)
                {
                    thisNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x) + i, (int)Mathf.Round(transform.parent.position.x) + j);
                    if (thisNode.isContainingUnit)
                    {
                        if (thisNode.unit != transform.parent.gameObject)
                        {
                            units = AddNodeToList(thisNode, units);
                        }
                    }
                }

                if (pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x) - i, (int)Mathf.Round(transform.parent.position.x) + j) != null)
                {
                    thisNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x) - i, (int)Mathf.Round(transform.parent.position.x) + j);
                    if (thisNode.isContainingUnit)
                    {
                        if (thisNode.unit != transform.parent.gameObject)
                        {
                            units = AddNodeToList(thisNode, units);
                        }
                    }
                }

                if (pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x) + i, (int)Mathf.Round(transform.parent.position.x) - j) != null)
                {
                    thisNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x) + i, (int)Mathf.Round(transform.parent.position.x) - j);
                    if (thisNode.isContainingUnit)
                    {
                        if (thisNode.unit != transform.parent.gameObject)
                        {
                            units = AddNodeToList(thisNode, units);
                        }
                    }
                }

                if (pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x) - i, (int)Mathf.Round(transform.parent.position.x) - j) != null)
                {
                    thisNode = pathfinding.GetNodeWithCoords((int)Mathf.Round(transform.parent.position.x) - i, (int)Mathf.Round(transform.parent.position.x) - j);
                    if (thisNode.isContainingUnit)
                    {
                        if (thisNode.unit != transform.parent.gameObject)
                        {
                            units = AddNodeToList(thisNode, units);
                        }
                    }
                }
            }
        }

        if (units.Count > 0)
        {
            Debug.Log("units.Count > 0 // else");
            for (int x = 0; x < units.Count; ++x)
            {
                Debug.Log(units[x].unit);
                List<PathNode> pathNode = null;
                pathNode = pathfinding.FindAreaPathMove((int)Mathf.Round(transform.parent.position.x), (int)Mathf.Round(transform.parent.position.z), units[x].x, units[x].y);

                if (pathNode != null)
                {
                    for (int y = 0; y < pathNode.Count; ++y)
                    {
                        Debug.Log(pathNode[y]);
                        /*if (!pathNode[pathNode.Count - 1 - y].isContainingImplant)
                        {
                            position = new Vector3(pathNode[pathNode.Count - 1 - y].x, 0.6f, pathNode[pathNode.Count - 1 - y].y);
                            return position;
                        }*/
                    }
                }
            }
        }

        return position;
    }

    public List<PathNode> AddNodeToList(PathNode thisNode, List<PathNode> units)
    {
        List<PathNode> newUnits = null;

        if (thisNode.unit != transform.parent.gameObject)
        {
            Debug.Log(thisNode.unit);
            if (units == null || units.Count == 0)
            {
                newUnits = new List<PathNode>();
                newUnits.Add(thisNode);
                Debug.Log(units);
            }
            else
            {
                Debug.Log(units.Count);
                int index = -1;
                //newUnits = units;

                for (int x = 0; x <= units.Count-1; ++x)
                {
                    Debug.Log(x);
                    PathNode node = units[x];
                    if (node != thisNode)
                    {
                        float thisNodeDistance = Vector3.Distance(new Vector3(thisNode.x, 0, thisNode.y), new Vector3((int)Mathf.Round(transform.parent.position.x), 0, (int)Mathf.Round(transform.parent.position.z)));
                        float nodeDistance = Vector3.Distance(new Vector3(node.x, 0, node.y), new Vector3((int)Mathf.Round(transform.parent.position.x), 0, (int)Mathf.Round(transform.parent.position.z)));
                        if (node.unit.GetComponent<UnitGridSystem>().team != thisNode.unit.GetComponent<UnitGridSystem>().team)
                        {
                            Debug.Log("team != team");
                            if (thisNodeDistance <= nodeDistance)
                            {
                                //units.Insert(x, thisNode);
                                index = x;
                                Debug.Log("(thisNodeDistance <= nodeDistance) == true");
                            }

                            if (x == units.Count - 1)
                            {
                                if (index == -1)
                                {
                                    for (int y = 0; y < units.Count; ++y)
                                    {
                                        if (units[y].unit.GetComponent<UnitGridSystem>().team == thisNode.unit.GetComponent<UnitGridSystem>().team)
                                        {
                                            newUnits.Insert(y, thisNode);
                                            Debug.Log("index = 0");
                                            break;
                                        }
                                    }

                                    if (index == -1)
                                    {
                                        newUnits.Add(thisNode);
                                        Debug.Log("index = 0");
                                    }
                                }
                                else
                                {
                                    newUnits.Insert(x, thisNode);
                                    Debug.Log("index = 0");
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("team = team");
                            if (thisNodeDistance >= nodeDistance)
                            {
                                Debug.Log("(thisNodeDistance >= nodeDistance) == true");
                                index = x;
                            }

                            if (x == units.Count - 1)
                            {
                                if (index == -1)
                                {
                                    for (int y = 0; y < units.Count; ++y)
                                    {
                                        if (units[y].unit.GetComponent<UnitGridSystem>().team != thisNode.unit.GetComponent<UnitGridSystem>().team)
                                        {
                                            index = y;
                                            Debug.Log("index = y");
                                        }
                                    }

                                    if (index == -1)
                                    {
                                        newUnits.Insert(0, thisNode);
                                        Debug.Log("index = 0");
                                    }
                                    else
                                    {
                                        if (index + 1 <= units.Count - 1)
                                        {
                                            newUnits.Insert(index + 1, thisNode);
                                            Debug.Log("index = 0");
                                        }
                                        else
                                        {
                                            newUnits.Add(thisNode);
                                            Debug.Log("index = 0");
                                        }
                                    }
                                }
                                else
                                {
                                    newUnits.Insert(x, thisNode);
                                    Debug.Log("index = 0");
                                }
                            }
                        }
                    }
                }
            }
        }

        Debug.Log(newUnits);
        return newUnits;
    }
}