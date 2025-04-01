using System.Linq;
using Script.Controller;
using Script.Controller.Commission;
using Script.Quest;
using Script.Resources;
using TMPro;
using UnityEngine;

namespace Script.UI.Mission {
    public class CommissionItemUI : MonoBehaviour {
        [SerializeField] TextMeshProUGUI _name;
        [SerializeField] TextMeshProUGUI _description;
        [SerializeField] TextMeshProUGUI _progress;
        [SerializeField] TextMeshProUGUI _reward;

        public Commission Commission { get; set; }

        public void UpdateCommissionData() {
            if (Commission is null) {
                _name.text = "";
                _description.text = "";
                _progress.text = "";
                _reward.text = "";
                Debug.LogWarning($"No commission assigned!");
                return;
            }

            var boxController = GameController.Instance.BoxController;
            _name.text = string.Join(", ", Commission.Items.Select(c => c.Key));
            _description.text = string.Join(", ", Commission.Items.Select(c => $"{c.Value} {c.Key}"));
            _progress.text = string.Join("\n", 
                Commission.Items
                    .Select(c => $"*{c.Key} {(boxController.TryGetAmount(c.Key, out var amount) ? amount : 0)}/{c.Value}"));

            if (Commission.Reward is ResourceQuestReward resourceReward) {
                _reward.text = resourceReward.Resources.Where(r => r.Key == Resource.Gold).Select(r => r.Value).Sum().ToString();
            }
            else _reward.text = "";
        }
    }
}