using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DelaunatorSharp;

public class BuildObject : MonoBehaviour
{
    private PlayerManager playerManager;

    private void Awake()
    {
        playerManager = PlayerManager.instance;
    }

    public void Rotate(float value)
    {
        if (value == 1)
        {
            transform.Rotate(new Vector3(0, 90f, 0));
        }
        else if (value == -1)
        {
            transform.Rotate(new Vector3(0, -90f, 0));
        }
    }

    public virtual void Place()
    {
        if (transform.tag == "Plant")
        {
            transform.gameObject.GetComponent<PlantManager>().CalculateTargetNode();
            PathNode node = PlayerManager.instance.pathfinding.GetNodeWithCoords(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            if (node != null)
            {
                if (!node.isVirtual)
                {
                    Debug.Log(node.tileManager);
                    node.tileManager.plant = transform.gameObject;
                }
            }
        }
        else if (transform.tag == "Object")
        {

        }
        Destroy(gameObject.GetComponent<BuildObjectDrag>());
        Destroy(this);
    }
}
