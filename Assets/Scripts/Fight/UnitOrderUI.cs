using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class UnitOrderUI : MonoBehaviour
{
    [SerializeField] public PlayerManager PC_Manager;

    [SerializeField] public Sprite playerPortrait;
    [SerializeField] public Sprite plantAttackPortrait;
    [SerializeField] public Sprite plantMovePortrait;
    [SerializeField] public Sprite plantBoostPortrait;
    [SerializeField] public Sprite unitPortrait;
    [HideInInspector] public RectTransform rt;

    public RectTransform selectionFollowerContainer;
    public List<UnitOrderUIElements> elements = new List<UnitOrderUIElements>();
    public UnitOrderUIElements currentElements;

    public int index = 0;
    private int previousActiveIndex = 0;

    void Awake()
    {
        rt = GetComponent<RectTransform>();

        if (rt == null)
        {
            Debug.LogError("Radial Menu: Rect Transform for radial menu " + gameObject.name + " could not be found. Please ensure this is an object parented to a canvas.");
        }

        if (selectionFollowerContainer == null)
        {
            Debug.LogError("Radial Menu: Selection follower container is unassigned on " + gameObject.name + ", which has the selection follower enabled.");
        }

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i] == null)
            {
                Debug.LogError("Radial Menu: element " + i.ToString() + " in the radial menu " + gameObject.name + " is null!");
                continue;
            }

            elements[i].assignedIndex = i;
            elements[i].parent = this;
        }
        currentElements.parent = this;
    }

    public void SetNextEntity()
    {
        Utilities.RotateList(PC_Manager.entitiesList);
        UpdateEntities();
    }

    public void UpdateEntities()
    {
        if (PC_Manager.entitiesList != null)
        {
            currentElements.SetEntity(0);

            for (int i = 0; i < elements.Count; i++)
            {
                if (i < PC_Manager.entitiesList.Count)
                {

                    elements[i].SetEntity(i);
                }
            }
        }
    }

    public void RunThroughEntities(int value)
    {
        int listCount = PC_Manager.entitiesList.Count;

        int currentIndex = (elements[0].entityIndex + listCount) % listCount;
        int newIndex = (currentIndex + value + listCount) % listCount;

        newIndex = (newIndex + listCount) % listCount;

        for (int i = 0; i < elements.Count; ++i)
        {
            elements[i].SetEntity(newIndex);

            newIndex = (newIndex + 1) % listCount;
        }
    }
}
