using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapUIElement : MonoBehaviour
{
    public enum State
    {
        Default,
        Hover,
        Select,
        Desactivate
    }

    public State state;
    public MapEvent mapEvent;
    public MapGenerator generator;
    public RectTransform rectTransform;

    public void Hover(bool value)
    {
        //Debug.Log(rectTransform.anchoredPosition + " // " + value);
    }

    public void Desactivate()
    {

    }

    public void Select()
    {
        if (mapEvent.eventNode.x - generator.currentNode.x != 1)
        {
            return;
        }
        else
        {
            generator.DesactivatePreviousEvents(mapEvent.eventNode.x);
            StartCoroutine(SelectBehavior());
        }
    }

    public IEnumerator SelectBehavior()
    {
        yield return null;

        generator.StartEvent(mapEvent);
        VirtualMouseManager.instance.Disable(false);
    }
}
