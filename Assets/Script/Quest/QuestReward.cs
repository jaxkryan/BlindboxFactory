using System;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Machine;
using Script.Resources;
using UnityEngine;

namespace Script.Quest {
    [Serializable]
    public abstract class QuestReward {
        public abstract void Grant();
    }

    [Serializable]
    public class UnlockMachineQuestReward : QuestReward {
        [SerializeField] MachineBase _machine;
        public override void Grant() {  }
    }

    [Serializable] public class BlindboxQuestReward : QuestReward {
        public override void Grant() { }
    }

    [Serializable]
    public class ResourceQuestReward : QuestReward {
        [SerializeReference] public SerializedDictionary<Resource, int> Resources = new();

        public override void Grant() {
            var controller = GameController.Instance.ResourceController;

            foreach (var resource in Resources.Keys) {
                if (!controller.TryGetAmount(resource, out var amount)) continue;
                
                controller.TrySetAmount(resource, Resources[resource] + amount);
            }
        }
    }
}