using System;
using System.Collections.Generic;
using System.Linq;
using BuildingSystem.Models;
using Script.Controller;
using UnityEngine;

namespace Script.Quest {
    [Serializable]
    public class UnlockMachineQuestReward : QuestReward {
        [SerializeField] List<BuildableItem> _machine;

        public override void Grant() {
            var names = _machine.Select(m => m.Name);
            var controller = GameController.Instance.MachineController;
            names.ForEach(controller.UnlockMachine);
        }
    }
}