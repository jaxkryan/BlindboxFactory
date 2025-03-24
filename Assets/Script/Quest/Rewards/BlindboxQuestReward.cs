using System;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using UnityEngine;

namespace Script.Quest {
    [Serializable]
    public class BlindboxQuestReward : QuestReward {
        [SerializeReference] public SerializedDictionary<BoxTypeName, long> Blindboxes = new();

        public override void Grant() {
            var controller = GameController.Instance.BoxController;

            foreach (var key in Blindboxes.Keys) {
                controller.TryGetAmount(key, out var amount);
                controller.TrySetAmount(key, Blindboxes[key] + amount);
            }
        }
    }
}