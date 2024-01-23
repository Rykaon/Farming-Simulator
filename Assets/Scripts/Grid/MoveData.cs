using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts
{
    [CreateAssetMenu]
    public class MoveData : ScriptableObject
    {
        public int movePoints;
        public int range;
        public int cost;
        public int gold;

        public bool isAbility;
        public Type type;
        public enum Type
        {
            Area,
            Linear
        }

        public Sprite sprite;
        public string abilityName;

        public TextAsset stats;
        public TextAsset description;

        public Sprite spriteLeft;
        public Sprite spriteRight;
    }
}
