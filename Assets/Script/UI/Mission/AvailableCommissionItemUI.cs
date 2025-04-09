using System;
using System.Linq;
using Script.Controller.Commission;
using Script.Quest;
using Script.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.UI.Mission
{
    public class AvailableCommissionItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _description;
        [SerializeField] private TextMeshProUGUI _reward;
        [SerializeField] private TextMeshProUGUI _expired;
        [SerializeField] private Button _accept;

        public Commission Commission { get; set; }

        private void Update()
        {
            if (Commission == null) return;
            UpdateTimerDisplay();
        }

        public void UpdateCommissionData()
        {
            if (Commission == null) return;

            _name.text = string.Join(", ", Commission.Items.Keys);
            _description.text = string.Join(", ", Commission.Items.Select(i => $"{i.Value}x {i.Key}"));

            if (Commission.Reward is ResourceQuestReward reward)
            {
                _reward.text = reward.Resources.TryGetValue(Resource.Gold, out var goldAmount) ? $"{goldAmount} Gold" : "No reward";
            }

            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            TimeSpan remaining = Commission.ExpireDate - DateTime.Now;

            if (remaining <= TimeSpan.Zero)
            {
                _expired.text = "Expired";
                _accept.interactable = false;
            }
            else
            {
                _expired.text = $"{(int)remaining.TotalMinutes}m {remaining.Seconds:D2}s left";
                _accept.interactable = true;
            }
        }
    }
}
