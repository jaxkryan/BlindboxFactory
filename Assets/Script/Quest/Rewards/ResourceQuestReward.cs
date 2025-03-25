using System;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Resources;
using UnityEngine;

namespace Script.Quest {
    [Serializable]
    public class ResourceQuestReward : QuestReward {
        [SerializeReference] public SerializedDictionary<Resource, int> Resources = new();

        public override void Grant() {
            var controller = GameController.Instance.ResourceController;

            foreach (var resource in Resources.Keys) {
                controller.TryGetAmount(resource, out var amount);
                controller.TrySetAmount(resource, Resources[resource] + amount);
            }
        }
    }
}