using DG.Tweening;
using JetBrains.Annotations;
using Palmmedia.ReportGenerator.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static Cinemachine.DocumentationSortingAttribute;
using static Map.MapEvent;
using static PlayerManager;

namespace Map
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] PlayerManager manager;
        public int mapWidth, mapHeight;
        public int minEvents, maxEvents;
        public int minUnits, maxUnits;
        public int nbrSeedRewardPerUnits, nbrGoldRewardPerUnits;
        public float baseTimeBetweenEvents, additionalTimeBetweenEvents;
        public AnimationCurve startAnimCurve;
        public AnimationCurve endAnimCurve;

        [Header("Beahviour References")]
        [HideInInspector] public bool onMap = false;
        [HideInInspector] public float travelElapsedTime;
        [HideInInspector] public float travelTime;
        [SerializeField] private float travelArrivalTime;
        [SerializeField] private Transform spawnPoint, dispawnPoint, stopPoint;
        [SerializeField] private GameObject worldToSpawn;
        [SerializeField] private List<GameObject> fightToSpawn;
        [SerializeField] private List<GameObject> shopToSpawn;
        [SerializeField] private List<GameObject> randomToSpawn;
        [SerializeField] private List<GameObject> endToSpawn;

        [Header("Grid References")]
        private GridMap<PathNode> nodeGrid;
        public List<PathNode> eventNodes;
        public PathNode currentNode;

        [Header("MapUI References")]
        private int poolSize;
        private float widthUnit, heightUnit;
        [SerializeField] private float widthPadding, heightPadding;
        [SerializeField] private GameObject mapParent;
        [SerializeField] private TextMeshProUGUI mapElementInfo;
        [SerializeField] private GameObject mapInfoEventNotCheck;
        [SerializeField] private GameObject mapInfoDestinationNotSet;
        [SerializeField] private GameObject mapEventPrefab;
        [SerializeField] private GameObject mapEventLinkPrefab;
        [SerializeField] private GameObject poolParent;
        private List<MapUIElement> pool = new List<MapUIElement>();
        private List<MapUIElement> objectsInUse = new List<MapUIElement>();
        private List<RectTransform> mapEventLinks = new List<RectTransform>();

        private void Awake()
        {
            poolSize = (mapWidth - 3) * maxEvents + 3;
            widthUnit = (1920 - (widthPadding * 2)) / (mapWidth - 1);
            heightUnit = (1080 - (heightPadding * 2)) / (mapHeight - 1);

            GameObject temp;

            for (int i = 0; i < poolSize; i++)
            {
                temp = Instantiate(mapEventPrefab, poolParent.transform);
                temp.SetActive(false);
                pool.Add(temp.GetComponent<MapUIElement>());
            }

            GenerateMap();
        }

        public void GenerateMap()
        {
            nodeGrid = new GridMap<PathNode>(mapWidth, mapHeight, 1f, Vector3.zero);

            int minEvents = this.minEvents;
            int maxEvents = this.maxEvents;
            if (mapHeight < maxEvents)
            {
                maxEvents = mapHeight;
            }

            List<PathNode> eventNodes = new List<PathNode>();

            for (int i = 0; i < nodeGrid.GetWidth(); ++i)
            {
                List<PathNode> nodeList = new List<PathNode>();

                for (int j = 0; j < nodeGrid.GetHeight(); ++j)
                {
                    nodeGrid.SetGridObject(i, j, new PathNode(nodeGrid, i, j, false));

                    if (i > 0 && i < nodeGrid.GetWidth() - 2)
                    {
                        nodeList.Add(nodeGrid.GetGridObject(i, j));
                    }
                    else
                    {
                        if (j == (nodeGrid.GetHeight() - 1) / 2)
                        {
                            PathNode node = nodeGrid.GetGridObject(i, j);
                            eventNodes.Add(node);
                            node.isEvent = true;

                            if (i == 0)
                            {
                                node.mapEvent = new MapEvent(this, node, MapEvent.EventType.Start);
                            }
                            else if (i == nodeGrid.GetWidth() - 2)
                            {
                                node.mapEvent = new MapEvent(this, node, MapEvent.EventType.Boss);
                            }
                            else
                            {
                                node.mapEvent = new MapEvent(this, node, MapEvent.EventType.End);
                            }
                        }
                    }
                }

                if (nodeList.Count > 0)
                {
                    int nbrEvents = UnityEngine.Random.Range(minEvents, maxEvents + 1);
                    List<int> usedIndex = new List<int>();
                    int randomIndex;

                    for (int j = 0; j < nbrEvents; ++j)
                    {
                        randomIndex = UnityEngine.Random.Range(0, nodeList.Count);
                        while (usedIndex.Contains(randomIndex))
                        {
                            randomIndex = UnityEngine.Random.Range(0, nodeList.Count);
                        }
                        usedIndex.Add(randomIndex);

                        PathNode node = nodeGrid.GetGridObject(i, randomIndex);
                        eventNodes.Add(node);
                        node.isEvent = true;
                        int randomEventType = UnityEngine.Random.Range(1, 101);

                        if (randomEventType > 0 && randomEventType < 61)
                        {
                            node.mapEvent = new MapEvent(this, node, MapEvent.EventType.Fight);
                        }
                        else if (randomEventType > 60 && randomEventType < 71)
                        {
                            node.mapEvent = new MapEvent(this, node, MapEvent.EventType.Shop);
                        }
                        else
                        {
                            node.mapEvent = new MapEvent(this, node, MapEvent.EventType.Random);
                        }
                    }
                }
            }

            this.eventNodes = eventNodes;
            RootEvents();
        }

        public void RootEvents()
        {
            for (int i = 0; i < eventNodes.Count; i++)
            {
                int axisPos = eventNodes[i].x;
                int nextAxisPos = -1;

                if (axisPos < mapWidth - 1)
                {
                    nextAxisPos = axisPos + 1;
                    List<PathNode> nextNodes = new List<PathNode>();
                    for (int j = i + 1; j < eventNodes.Count; ++j)
                    {
                        if (eventNodes[j].x == nextAxisPos)
                        {
                            nextNodes.Add(eventNodes[j]);
                            int diff = Mathf.Abs(eventNodes[i].y - eventNodes[j].y);
                            eventNodes[i].toNodes.Add(eventNodes[j]);
                            eventNodes[i].toNodesTime.Add(baseTimeBetweenEvents + (diff * additionalTimeBetweenEvents));
                            eventNodes[j].fromNodes.Add(eventNodes[j]);
                        }
                        else if (eventNodes[j].x > nextAxisPos)
                        {
                            break;
                        }
                    }
                }
            }

            currentNode = eventNodes[0];
            currentNode.mapEvent.isEventCheck = true;

            GenerateUIMap();
        }

        private void GenerateUIMap()
        {
            for (int i = 0; i < eventNodes.Count; ++i)
            {
                MapUIElement mapUIElement = GetObjectFromPool(eventNodes[i].mapEvent);

                for (int j = 0; j < eventNodes[i].toNodes.Count; ++j)
                {
                    RectTransform temp = (RectTransform)Instantiate(mapEventLinkPrefab, mapParent.transform).transform;
                    mapEventLinks.Add(temp);
                    temp.SetAsFirstSibling();
                    float nextPosY = ((heightPadding + eventNodes[i].toNodes[j].y * heightUnit) - mapUIElement.rectTransform.anchoredPosition.y) / 2;
                    temp.anchoredPosition = new Vector3(mapUIElement.rectTransform.anchoredPosition.x + (widthUnit / 2), mapUIElement.rectTransform.anchoredPosition.y + nextPosY, 0f);
                    int diff = eventNodes[i].y - eventNodes[i].toNodes[j].y;
                    int absDiff = Mathf.Abs(diff);
                    temp.sizeDelta = new Vector2(Mathf.Sqrt((widthUnit * widthUnit) + ((absDiff * heightUnit) * (absDiff * heightUnit))), 10f);
                    temp.rotation = Quaternion.Euler(0, 0, -Mathf.Atan((diff * heightUnit) / widthUnit) * (180 / Mathf.PI));
                }
            }
        }

        public void ShowHideUIMap(bool value)
        {
            mapParent.SetActive(value);
            onMap = value;
        }

        public void SetMapInfoErrorMessage(bool eventNotCheck, bool destinationNotSet)
        {
            GameObject info = null;

            if (eventNotCheck && !destinationNotSet)
            {
                info = mapInfoEventNotCheck;
            }
            else if (!eventNotCheck && destinationNotSet)
            {
                info = mapInfoDestinationNotSet;
            }
            else
            {
                Debug.Log("Parameters not set");
                return;
            }

            StartCoroutine(SetMapInfoErrorMessage(info));
        }

        private IEnumerator SetMapInfoErrorMessage(GameObject mapInfo)
        {
            mapInfo.SetActive(true);
            yield return new WaitForSecondsRealtime(3);
            mapInfo.SetActive(false);
        }

        public void DesactivatePreviousEvents(int value)
        {
            for (int i = 0; i < eventNodes.Count; ++i)
            {
                if (eventNodes[i].x == value - 1)
                {
                    objectsInUse[i].Desactivate();
                }
                else if (eventNodes[i].x == value)
                {
                    break;
                }
            }
        }

        public MapUIElement GetObjectFromPool(MapEvent mapEvent)
        {
            if (pool.Count > 0)
            {
                MapUIElement pulledObject = pool[0];
                pool.RemoveAt(0);
                objectsInUse.Add(pulledObject);
                pulledObject.mapEvent = mapEvent;
                pulledObject.generator = this;
                pulledObject.gameObject.SetActive(true);
                pulledObject.transform.SetParent(mapParent.transform, true);
                pulledObject.rectTransform.anchoredPosition = new Vector3(widthPadding + (mapEvent.eventNode.x * widthUnit), heightPadding + (mapEvent.eventNode.y * heightUnit), 0f);

                return pulledObject;
            }

            return null;
        }

        public void ReturnObjectToPool(MapUIElement objectToReturn)
        {
            objectToReturn.transform.position = transform.position;
            objectToReturn.transform.rotation = transform.rotation;
            objectToReturn.transform.parent = poolParent.transform;

            // Ici on reset tout autre paramètre modifié par le cycle de vie de l'objet
            objectToReturn.gameObject.SetActive(false);
            objectsInUse.Remove(objectToReturn);
            pool.Add(objectToReturn);
        }

        public void SetUIMapEventInfo(MapEvent mapEvent)
        {
            if (mapEvent == null)
            {
                mapElementInfo.text = "";
                return;
            }

            string eventType = "";
            string rewardType = "";
            string time = "";

            switch (mapEvent.rewardType)
            {
                case MapEvent.RewardType.Gold:
                    rewardType = " Pièces d'or";
                    break;

                case MapEvent.RewardType.Attack:
                    rewardType = " Plantes Attaque";
                    break;

                case MapEvent.RewardType.Move:
                    rewardType = " Plantes Move";
                    break;

                case MapEvent.RewardType.Boost:
                    rewardType = " Plantes Boost";
                    break;
            }

            int diff = mapEvent.eventNode.x - currentNode.x;
            if (diff > 0)
            {
                if (diff == 1)
                {
                    int index = Utilities.FindIndexInList(mapEvent.eventNode, currentNode.toNodes);
                    time = currentNode.toNodesTime[index].ToString() + " secondes";
                    Debug.Log("yo");
                }
                else
                { 
                    float minTime = 0;
                    float maxTime = 0;
                    int minIndex = -1;
                    int maxIndex = -1;
                    PathNode minNode = currentNode;
                    PathNode maxNode = currentNode;

                    for (int i = 1; i < diff; ++i)
                    {
                        float thisMinTime = 1000;
                        float thisMaxTime = 0;

                        for (int j = 0; j < minNode.toNodes.Count; ++j)
                        {
                            if (minNode.toNodesTime[j] < thisMinTime)
                            {
                                thisMinTime = minNode.toNodesTime[j];
                                minIndex = j;
                            }
                        }

                        for (int j = 0; j < maxNode.toNodes.Count; ++j)
                        {
                            if (maxNode.toNodesTime[j] > thisMaxTime)
                            {
                                thisMaxTime = maxNode.toNodesTime[j];
                                maxIndex = j;
                            }
                        }

                        minTime += thisMinTime;
                        maxTime += thisMaxTime;
                        minNode = minNode.toNodes[minIndex];
                        maxNode = maxNode.toNodes[maxIndex];
                    }

                    minIndex = Utilities.FindIndexInList(mapEvent.eventNode, minNode.toNodes);
                    maxIndex = Utilities.FindIndexInList(mapEvent.eventNode, maxNode.toNodes);
                    minTime += minNode.toNodesTime[minIndex];
                    maxTime += minNode.toNodesTime[maxIndex];

                    if (minTime == maxTime)
                    {
                        time = minTime.ToString() + " secondes";
                    }
                    else
                    {
                        time = "Entre " + minTime.ToString() + " et " + maxTime.ToString() + " secondes";
                    }
                }
            }
            else
            {
                time = "/";
            }

            switch (mapEvent.eventType)
            {
                case MapEvent.EventType.Start:
                    eventType = "Début de livraison";
                    mapElementInfo.text = "Type = " + eventType;
                    break;

                case MapEvent.EventType.Fight:
                    eventType = "Combat";
                    mapElementInfo.text = "Type = " + eventType + " // Ennemis = " + mapEvent.nbrUnits.ToString() + " // Récompense = " + mapEvent.nbrReward.ToString() + rewardType + " // Durée du voyage = " + time;
                    break;

                case MapEvent.EventType.Boss:
                    eventType = "Combat de boss";
                    mapElementInfo.text = "Type = " + eventType + " // Ennemis = " + mapEvent.nbrUnits.ToString() + " // Récompense = " + mapEvent.nbrReward.ToString() + rewardType + " // Durée du voyage = " + time;
                    break;

                case MapEvent.EventType.Shop:
                    eventType = "Marchand";
                    mapElementInfo.text = "Type = " + eventType + " // Durée du voyage = " + time;
                    break;

                case MapEvent.EventType.Random:
                    eventType = "Mystère";
                    mapElementInfo.text = "Type = " + eventType + " // Récompense = Inconnue" + " // Durée du voyage = " + time;
                    break;

                case MapEvent.EventType.End:
                    eventType = "Point de livraison";
                    mapElementInfo.text = "Type = " + eventType + " // Commande = " + mapEvent.nbrReward.ToString() + rewardType + " // Durée du voyage = " + time;
                    break;
            }
        }

        public void StartEvent(MapEvent mapEvent)
        {
            StartCoroutine(StartEventBehavior(mapEvent));
        }

        public IEnumerator StartEventBehavior(MapEvent mapEvent)
        {
            ShowHideUIMap(false);
            float elapsedTime = 0f;
            worldToSpawn.transform.DOMove(dispawnPoint.position, travelArrivalTime).SetEase(startAnimCurve);

            while (elapsedTime < travelArrivalTime)
            {
                elapsedTime += Time.deltaTime;
            }

            Destroy(worldToSpawn);

            // SetUp la phase Farm en attendant l'event
            manager.ChangeState(ControlState.Farm);

            manager.athFarm.SetActive(true);
            manager.athFight.SetActive(false);

            // La Coroutine pour la phase Farm
            int thisMapEventIndex = Utilities.FindIndexInList(mapEvent.eventNode, currentNode.toNodes);
            travelElapsedTime = 0f;
            travelTime = currentNode.toNodesTime[thisMapEventIndex];
            while (travelElapsedTime < travelTime)
            {
                //Debug.Log("CURRENT TIME = " + travelElapsedTime + " // END TIME " + travelTime);
                if (!onMap)
                {
                    travelElapsedTime += Time.deltaTime;
                }
                
                yield return new WaitForEndOfFrame();
            }

            currentNode = mapEvent.eventNode;
            List<GameObject> worldToSpawnList = new List<GameObject>();

            if (mapEvent.eventType == MapEvent.EventType.Boss || mapEvent.eventType == MapEvent.EventType.Fight)
            {
                worldToSpawnList = fightToSpawn;
            }
            else if (mapEvent.eventType == MapEvent.EventType.Shop)
            {
                worldToSpawnList = shopToSpawn;
            }
            else if (mapEvent.eventType == MapEvent.EventType.Random)
            {
                worldToSpawnList = randomToSpawn;
            }
            else if (mapEvent.eventType == MapEvent.EventType.End)
            {
                worldToSpawnList = endToSpawn;
            }

            int index = UnityEngine.Random.Range(0, worldToSpawnList.Count);
            worldToSpawn = Instantiate(worldToSpawnList[index], spawnPoint.position, Quaternion.identity);

            elapsedTime = 0f;
            worldToSpawn.transform.DOMove(stopPoint.position, travelArrivalTime).SetEase(endAnimCurve);

            while (elapsedTime < travelArrivalTime)
            {
                elapsedTime += Time.deltaTime;
            }

            // SetUp la phase de l'event selectionné
            if (mapEvent.eventType == MapEvent.EventType.Boss || mapEvent.eventType == MapEvent.EventType.Fight)
            {
                if (manager.PC_farm.GetCurrentNode() != null)
                {
                    if (!manager.PC_farm.GetCurrentNode().isVirtual)
                    {
                        manager.playerNode = manager.PC_farm.GetCurrentNode();
                    }
                    else
                    {
                        manager.playerNode = manager.pathfinding.GetNodeWithCoords(0, 0);
                    }
                }
                else
                {
                    manager.playerNode = manager.pathfinding.GetNodeWithCoords(0, 0);
                }

                manager.playerNode.isContainingUnit = true;
                manager.playerNode.unit = gameObject;
                manager.playerNode.isWalkable = false;

                manager.ResetStats();

                if (mapEvent.eventType == MapEvent.EventType.Boss)
                {
                    manager.SetUnits(mapEvent.nbrUnits, true);
                }
                else
                {
                    manager.SetUnits(mapEvent.nbrUnits, false);
                }

                manager.turn = Turn.Player;
                manager.ChangeState(ControlState.Fight);

                manager.rigidBody.velocity = Vector3.zero;
                transform.position = new Vector3(manager.playerNode.x, transform.position.y, manager.playerNode.y);

                manager.athFight.SetActive(true);
                manager.athFarm.SetActive(false);
                manager.fightOrderUI.gameObject.SetActive(true);
                manager.fightOrderUI.UpdateEntities();
            }
            else if (mapEvent.eventType == MapEvent.EventType.Shop)
            {
                manager.ChangeState(ControlState.World);
                mapEvent.isEventCheck = true;
            }
            else if (mapEvent.eventType == MapEvent.EventType.Random)
            {
                manager.ChangeState(ControlState.World);
            }
            else if (mapEvent.eventType == MapEvent.EventType.End)
            {
                manager.ChangeState(ControlState.World);

                manager.PC_farm.CollectAll();
                int nbrOfGold = 0;
                for (int i = 0; i < manager.inventory.plantsList.Count; ++i)
                {
                    nbrOfGold += Utilities.GetNumberOfItemByPrefab(manager.inventory.inventory, manager.inventory.plantsList[i].Prefab);
                }
                nbrOfGold *= manager.inventory.plantsList[0].sellPrice;
                nbrOfGold += manager.inventory.nbArgent;

                if (nbrOfGold < mapEvent.nbrReward)
                {
                    // La run est loose
                }
            }
        }

        public void TakeReward()
        {
            GameObject item = null;

            switch (currentNode.mapEvent.rewardType)
            {
                case MapEvent.RewardType.Gold:
                    if (currentNode.mapEvent.isBonus)
                    {
                        manager.inventory.nbArgent += currentNode.mapEvent.nbrReward;
                    }
                    else
                    {
                        manager.inventory.nbArgent -= currentNode.mapEvent.nbrReward;
                        if (manager.inventory.nbArgent < 0)
                        {
                            manager.inventory.nbArgent = 0;
                        }
                    }
                    break;

                case MapEvent.RewardType.Attack:
                    item = manager.inventory.plantsList[0].Prefab;
                    break;

                case MapEvent.RewardType.Move:
                    item = manager.inventory.plantsList[1].Prefab;
                    break;

                case MapEvent.RewardType.Boost:
                    item = manager.inventory.plantsList[2].Prefab;
                    break;
            }

            if (item != null)
            {
                if (currentNode.mapEvent.isBonus)
                {
                    for (int i = 0; i < currentNode.mapEvent.nbrReward; ++i)
                    {
                        Utilities.AddItemByPrefab(manager.inventory.inventory, item);
                    }
                }
                else
                {
                    for (int i = 0; i < currentNode.mapEvent.nbrReward; ++i)
                    {
                        Utilities.RemoveItemByPrefab(manager.inventory.inventory, item);
                    }
                }
            }

            manager.UpdateUIInventory();
        }
    }

    [Serializable]
    public class MapEvent
    {
        private MapGenerator generator;

        public enum EventType
        {
            Start,
            End,
            Boss,
            Fight,
            Shop,
            Random
        }

        public enum RewardType
        {
            Gold,
            Attack,
            Move,
            Boost
        }

        // General values
        public EventType eventType;
        public RewardType rewardType;
        public int nbrReward;
        public string rewardName;
        public PathNode eventNode;
        public bool isEventCheck;

        // Fight values
        public int nbrUnits;

        // Random values
        public bool isBonus;

        public MapEvent(MapGenerator generator, PathNode node, EventType type)
        {
            this.generator = generator;
            eventNode = node;
            eventType = type;
            isBonus = true;
            isEventCheck = false;

            if (eventType == EventType.End)
            {
                rewardType = RewardType.Gold;
                nbrReward = (generator.mapWidth - 3) * (generator.minUnits * 15);
            }
            else
            {
                int randReward = UnityEngine.Random.Range(1, 101);
                if (randReward > 0 && randReward < 26)
                {
                    rewardType = RewardType.Gold;
                    rewardName = "Gold";
                }
                else if (randReward > 25 && randReward < 51)
                {
                    rewardType = RewardType.Attack;
                    rewardName = "Attack";
                }
                else if (randReward > 50 && randReward < 76)
                {
                    rewardType = RewardType.Move;
                    rewardName = "Move";
                }
                else
                {
                    rewardType = RewardType.Boost;
                    rewardName = "Boost";
                }
            }

            switch(type)
            {
                case EventType.Boss:
                    nbrUnits = generator.maxUnits;

                    if (rewardType == RewardType.Gold)
                    {
                        nbrReward = generator.maxUnits * generator.nbrGoldRewardPerUnits;
                    }
                    else
                    {
                        nbrReward = generator.maxUnits * generator.nbrSeedRewardPerUnits;
                    }
                    break;

                case EventType.Fight:
                    nbrUnits = UnityEngine.Random.Range(generator.minUnits, generator.maxUnits + 1);

                    if (rewardType == RewardType.Gold)
                    {
                        nbrReward = nbrUnits * generator.nbrGoldRewardPerUnits;
                    }
                    else
                    {
                        nbrReward = nbrUnits * generator.nbrSeedRewardPerUnits;
                    }
                    break;

                case EventType.Shop:
                    break;

                case EventType.Random:
                    int isBonus = UnityEngine.Random.Range(1, 101);

                    if (isBonus > 0 && isBonus < 71)
                    {
                        this.isBonus = true;
                    }
                    else
                    {
                        this.isBonus = false;
                    }

                    if (rewardType == RewardType.Gold)
                    {
                        nbrReward = generator.nbrGoldRewardPerUnits;
                    }
                    else
                    {
                        nbrReward = generator.nbrSeedRewardPerUnits;
                    }
                    break;
            }
        }
    }
}

