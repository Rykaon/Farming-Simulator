using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildObjectConnector : MonoBehaviour
{
    public BuildObject buildObject;
    public BuildObjectConnector twinConnector;
    public BuildObjectConnector connectedObject;
    public bool isConnected = false;

    public BoxCollider col;

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Connector")
        {
            if (!other.GetComponent<BuildObjectConnector>().isConnected)
            {
                connectedObject = other.GetComponent<BuildObjectConnector>();
                connectedObject.connectedObject = this;
                connectedObject.isConnected = true;
                isConnected = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Connector")
        {
            if (isConnected && other.GetComponent<BuildObjectConnector>() == connectedObject)
            {
                connectedObject.isConnected = false;
                connectedObject.connectedObject = null;
                connectedObject = null;
                isConnected = false;
            }
        }
    }

    public bool CheckConnection(BuildObject parent, List<BuildObject> list)
    {
        bool isConnectionComplete = false;

        if (twinConnector.isConnected)
        {
            isConnectionComplete = true;
            list.Add(twinConnector.connectedObject.buildObject);

            if (twinConnector.connectedObject.buildObject == parent)
            {
                parent.group = list;

                for (int i = 0; i < list.Count; i++)
                {
                    list[i].groupHolder = parent;
                    list[i].group = list;
                    list[i].isConnected = true;
                }

                parent.CreateGroupBounds();

                return isConnectionComplete;
            }

            twinConnector.connectedObject.CheckConnection(parent, list);
        }

        return isConnectionComplete;
    }
}
