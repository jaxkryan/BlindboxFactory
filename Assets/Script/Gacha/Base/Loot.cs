using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Gacha.Base {
    public class Loot : ScriptableObject, ILoot {
        public Grade Grade { get => grade; protected set => grade = value; }
        [SerializeField] private Grade grade;
    }
}