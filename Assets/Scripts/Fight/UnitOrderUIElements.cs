using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitOrderUIElements : MonoBehaviour
{
    [HideInInspector] public UnitOrderUI parent; 
    [HideInInspector] public RectTransform rt;
    [HideInInspector] public Transform entity;

    public Image portrait;
    public bool selected = false;
    public int assignedIndex = 0;
    public int entityIndex = 0;
    private CanvasGroup cg;

    void Awake()
    {
        rt = gameObject.GetComponent<RectTransform>();

        if (gameObject.GetComponent<CanvasGroup>() == null)
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }
        else
        {
            cg = gameObject.GetComponent<CanvasGroup>();
        }


        if (rt == null)
        {
            Debug.LogError("Radial Menu: Rect Transform for radial element " + gameObject.name + " could not be found. Please ensure this is an object parented to a canvas.");
        }
    }

    public void SetEntity(int index)
    {
        entityIndex = index;
        entity = parent.PC_Manager.entitiesList[index].transform;

        switch (entity.tag)
        {
            case "Player":
                portrait.sprite = parent.playerPortrait;
                break;

            case "Plant":
                switch (entity.GetComponent<PlantManager>().type)
                {
                    case PlantManager.Type.Attack:
                        portrait.sprite = parent.plantAttackPortrait;
                        break;

                    case PlantManager.Type.Move:
                        portrait.sprite = parent.plantMovePortrait;
                        break;

                    case PlantManager.Type.Boost:
                        portrait.sprite = parent.plantBoostPortrait;
                        break;
                }
                break;

            case "Unit":
                portrait.sprite = parent.unitPortrait;
                break;
        }
    }
}
