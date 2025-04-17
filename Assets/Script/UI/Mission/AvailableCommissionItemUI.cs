using System;
using System.Linq;
using System.Text;
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

            // Update timer display only when the interval has passed
            if (Time.time >= _nextUpdateTime)
            {
                // Decrease remaining time based on elapsed game time
                _remainingSeconds -= (Time.time - _nextUpdateTime);
                UpdateTimerDisplay();
                _nextUpdateTime = Time.time + UPDATE_INTERVAL;
            }
        }

        public void UpdateCommissionData()
        {
            var items = _commission.Items;

            _stringBuilder.Clear();
            _stringBuilder.AppendJoin(", ", items.Keys);
            _name.text = _stringBuilder.ToString();

            _stringBuilder.Clear();
            foreach (var item in items)
            {
                _stringBuilder.Append($"{item.Value}x {item.Key}, ");
            }
            if (_stringBuilder.Length > 0) _stringBuilder.Length -= 2;
            _description.text = _stringBuilder.ToString();

            _reward.text = _commission.Reward is ResourceQuestReward reward &&
                          reward.Resources.TryGetValue(Resource.Gold, out var goldAmount)
                ? $"{goldAmount} Gold"
                : "No reward";

            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            bool isExpired = _remainingSeconds <= 0;
            if (isExpired)
            {
                _expired.text = "Expired";
                _accept.interactable = false;
            }
            else
            {
                int minutes = (int)(_remainingSeconds / 60);
                int seconds = (int)(_remainingSeconds % 60);
                _expired.text = $"{minutes:D2}m {seconds:D2}s";
                _accept.interactable = true;
            }
        }

        public void OnAcceptButtonClicked()
        {
            if (_commission != null)
            {
                GameController.Instance.CommissionController.TryAddCommission(_commission);
                Debug.Log($"Commission accepted: {_commission}");
                _accept.interactable = false;
            }
        }

        private void Start()
        {
            if (_accept != null)
            {
                _accept.onClick.AddListener(OnAcceptButtonClicked);
            }
        }
    }
}