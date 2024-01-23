using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [CreateAssetMenu]
    public class ActionData : ScriptableObject
    {
        public int actionPoints;
        public int range;
        public int cost;
        public int damage;
        public int gold;
        public bool isLocal;

        public bool isAbility;
        public bool isWorld;
        public bool isPhysical;
        public Type type;
        public enum Type
        {
            Area,
            Linear,
            LinearPath,
        }

        public Sprite sprite;
        public string abilityName;

        public TextAsset stats;
        public TextAsset description;

        public Sprite spriteLeft;
        public Sprite spriteRight;
    }
}
