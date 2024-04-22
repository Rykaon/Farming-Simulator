using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public Image eventImage;
    public Image playerImage;
    private Color opaqueColor = Color.white;
    public Color transparentColor = new Color(1f, 1f, 1f, 0f);

    public Sprite startSprite;
    public Sprite shopSprite;
    public Sprite fightSprite;
    public Sprite randomSprite;

    public void Hover(bool value)
    {
        //Debug.Log(rectTransform.anchoredPosition + " // " + value);
    }

    public void InitializeSprite()
    {
        if (mapEvent.eventType == MapEvent.EventType.Start || mapEvent.eventType == MapEvent.EventType.End)
        {
            eventImage.sprite = startSprite;
        }
        else if (mapEvent.eventType == MapEvent.EventType.Fight)
        {
            eventImage.sprite = fightSprite;
        }
        else if (mapEvent.eventType == MapEvent.EventType.Shop)
        {
            eventImage.sprite = shopSprite;
        }
        else if (mapEvent.eventType == MapEvent.EventType.Random)
        {
            eventImage.sprite = randomSprite;
        }
    }

    public void UpdateSprite(bool isCurrent)
    {
        if (isCurrent)
        {
            playerImage.color = opaqueColor;
        }
        else
        {
            playerImage.color = transparentColor;
        }
    }

    public void Desactivate()
    {

    }

    public void Select()
    {
        if (mapEvent.eventNode.x - generator.currentNode.x == 1)
        {
            if (PlayerManager.instance.controlState == PlayerManager.ControlState.World)
            {
                if (generator.currentNode.mapEvent.isEventCheck)
                {
                    generator.DesactivatePreviousEvents(mapEvent.eventNode.x);
                    StartCoroutine(SelectBehavior());
                }
                else
                {
                    generator.SetMapInfoErrorMessage(true, false);
                }
            }
            else
            {
                generator.SetMapInfoErrorMessage(false, true);
            }
        }
    }

    public IEnumerator SelectBehavior()
    {
        yield return null;

        generator.StartEvent(mapEvent);
        VirtualMouseManager.instance.Disable(false);
    }
}
