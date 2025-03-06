using System;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Resources;
using UnityEngine;

namespace Script.Quest {
    [Serializable]
    public abstract class QuestReward {
        public abstract void Grant();
    }

    [Serializable]
    public class UnlockMachineQuestReward : QuestReward {
        public override void Grant() { throw new NotImplementedException(); }
    }

    [Serializable] public class BlindboxQuestReward : QuestReward {
        public override void Grant() { throw new NotImplementedException(); }
    }

    [Serializable]
    public class ResourceQuestReward : QuestReward {
        [SerializeReference] public SerializedDictionary<Resource, int> Resources;

        public override void Grant() {
            var controller = GameController.Instance.ResourceController;

            foreach (var resource in Resources.Keys) {
                if (!controller.TryGetAmount(resource, out var amount)) continue;
                
                controller.TrySetAmount(resource, Resources[resource] + amount);
            }
        }
    }
}