using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject
{
    [HideInInspector] public int width = 9;
    [HideInInspector] public int height = 15;
    [SerializeField][TextArea(30, 30)] private string content;
    public string Content => content;
}
