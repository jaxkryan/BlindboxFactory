using System;
using System.Linq;
using System.Text;
using Script.Alert;
using Script.Controller;
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

        private Commission _commission;
        private StringBuilder _stringBuilder = new StringBuilder();
        private float _nextUpdateTime;
        private double _remainingSeconds;
        private const float UPDATE_INTERVAL = 1f; // Update every second

        public Commission Commission
        {
            get => _commission;
            set
            {
                _commission = value;
                if (_commission != null)
                {
                    // Calculate remaining time when commission is set
                    _remainingSeconds = (_commission.ExpireDate - DateTime.Now).TotalSeconds;
                    UpdateCommissionData();
                    _nextUpdateTime = Time.time;
                }
            }
        }

        private void Update()
        {
            if (_commission == null) return;
        }

        public void UpdateCommissionData()
        {
            var items = _commission.Items;

            _stringBuilder.Clear();
            _stringBuilder.AppendJoin(", ", items.Keys);
            _name.text = _stringBuilder.ToString();

            var box = GameController.Instance.BoxController;
            bool isComplete = true;
            
            _stringBuilder.Clear();
            foreach (var item in items)
            {
                _stringBuilder.Append($"{item.Value}x {item.Key}, ");
                
                if (box.TryGetAmount(item.Key, out var amount)) {
                    if (amount < item.Value) isComplete = false;
                }
                else Debug.LogError($"Cannot get amount {item.Key}");
            }

            if (isComplete) _accept.image.sprite = AlertManager.Instance.Green;
            if (_stringBuilder.Length > 0) _stringBuilder.Length -= 2;
            _description.text = _stringBuilder.ToString();

            _reward.text = _commission.Price > 0
                ? $"{_commission.Price} Gold"
                : "No reward";

            int hours = (int)(_remainingSeconds / 3600);
            int minutes = (int)((_remainingSeconds % 3600) / 60);
            int seconds = (int)(_remainingSeconds % 60);
            _expired.text = $"{hours:D2}h {minutes:D2}m {seconds:D2}s";
        }
    }
}