using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScaler : MonoBehaviour
{
    void Start()
    {
        transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Screen.width);
        transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
    }
}
