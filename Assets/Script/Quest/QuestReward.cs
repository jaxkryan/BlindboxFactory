using System;
using MyBox;
using Script.Machine;
using UnityEngine.Serialization;

namespace Script.Quest {
    [Serializable]
    public abstract class QuestReward {
        public abstract void Grant();
    }
}