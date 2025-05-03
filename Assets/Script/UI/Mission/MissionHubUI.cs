using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Controller;
using Script.Controller.Commission;
using Script.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Script.UI.Mission {
    public class MissionHubUI : MonoBehaviour {
        [SerializeField] [Range(1, 4)] private int _defaultPanelIndex = 1;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _background;
        [SerializeField] private GameObject _contentHolder;
        [SerializeField] private DailyMissionPanel _dailyMissionPanel;
        [SerializeField] private QuestPanel _questPanel;
        [SerializeField] private CommissionPanel _commissionPanel;
        [SerializeField] private AvailableCommissionPanel _availableCommissionPanel;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private bool _openCommissionPanelWhenSelectNewCommission = true;

        public string ActivePanelName { get; private set; } = string.Empty;
        
        public void Open() {
            switch (_defaultPanelIndex) {
                case 1:
                    OpenAvailableCommissionsPanel();
                    break;
                case 2:
                    OpenCommissionPanel();
                    break;
                case 3:
                    OpenDailyMissionPanel();
                    break;
                case 4:
                    OpenQuestPanel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDisable() {
            ActivePanelName = string.Empty;
        }

        public void OpenDailyMissionPanel() {
            Setup(_dailyMissionPanel);

            ActivePanelName = nameof(DailyMissionPanel);
            var controller = GameController.Instance.DailyMissionController;
            var list = controller.DailyMissions.Select(m => {
                //Instantiate mission from prefab and populate content
                var mission = Instantiate(_dailyMissionPanel.ItemPrefab.gameObject, _contentHolder.transform);
                if (mission.TryGetComponent<DailyItemUI>(out var item)) {
                    // Debug.LogWarning(m.State);
                    item.DailyMission = m;
                    item.UpdateQuestData();
                    return item;
                }
                else {
                    Destroy(mission);
                    Debug.LogWarning(
                        $"{_dailyMissionPanel.ItemPrefab.gameObject.name} is missing component {nameof(DailyItemUI)}");
                }

                return null;
            }).Where(m => m != null).ToList();
        }

        public void OpenCommissionPanel() {
            Setup(_commissionPanel);

            ActivePanelName = nameof(CommissionPanel);
            var controller = GameController.Instance.CommissionController;
            controller.UpdateCommissions();
            
            var list = controller.Commissions.Select(c => {
                var commission = Instantiate(_commissionPanel.ItemPrefab.gameObject, _contentHolder.transform);
                if (commission.TryGetComponent<CommissionItemUI>(out var item)) {
                    item.Commission = c;
                    item.UpdateCommissionData();
                    return commission;
                }
                else {
                    Destroy(commission);
                    Debug.LogWarning(
                        $"{_commissionPanel.ItemPrefab.gameObject.name} is missing component {nameof(CommissionItemUI)}");
                }
            
                return null;
            }).Where(c => c != null).ToList();
        }

        private HashSet<Commission> _availableCommissions = new();
        DateTime _lastAvailableCommissionUpdate = DateTime.MinValue;
        private float _availableCommissionRefreshHours = -1;
        
        public void OpenAvailableCommissionsPanel()
        {
            Setup(_availableCommissionPanel);
            var controller = GameController.Instance.CommissionController;

            ActivePanelName = nameof(AvailableCommissionPanel);
            if (_availableCommissionRefreshHours < 0) _availableCommissionRefreshHours = controller.AvailableCommissionRefreshHours;

            if (!_availableCommissions.Any() || _lastAvailableCommissionUpdate <
                DateTime.Now.AddHours(-_availableCommissionRefreshHours))
            {
                _availableCommissions = controller.CreateCommissions();
                _lastAvailableCommissionUpdate = DateTime.Now;
            }

            foreach (var available in _availableCommissions)
            {
                var go = Instantiate(_availableCommissionPanel.ItemPrefab.gameObject, _contentHolder.transform);
                var ui = go.GetComponent<AvailableCommissionItemUI>();
                ui.Commission = available;
                ui.UpdateCommissionData();

                // Optionally bind the accept button behavior
                ui.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    controller.TryAddCommission(available);
                    _availableCommissions.Remove(available);
                    if (_openCommissionPanelWhenSelectNewCommission) OpenCommissionPanel(); 
                });
            }
        }

        public void OpenQuestPanel() {
            Setup(_questPanel);

            ActivePanelName = nameof(QuestPanel);
            var controller = GameController.Instance.QuestController;
            controller.Quests.ForEach((q => q.Evaluate()));
            var list = controller.Quests.Where(q => q.State == QuestState.InProgress).Select(q => {
                //Instantiate mission from prefab and populate content
                var quest = Instantiate(_questPanel.ItemPrefab.gameObject, _contentHolder.transform);
                if (quest.TryGetComponent<QuestItemUI>(out var item)) {
                    item.Quest = q;
                    item.SetQuestData();
                    return item;
                }
                else {
                    Destroy(quest);
                    Debug.LogWarning(
                        $"{_dailyMissionPanel.ItemPrefab.gameObject.name} is missing component {nameof(QuestItemUI)}");
                }

                return null;
            }).Where(m => m != null).ToList();
        }

        private void Setup(MissionPanel panel = null) {
            gameObject.SetActive(true);
            if (TryGetComponent<DotweenAnimation>(out var animation)) {
                animation.AnimateIn();
            } 
            ClearContent();

            if (panel != null) { 
                //Change panel name
                _name.text = panel.PanelName;
                //Change panel background
                if (panel.BackgroundImage != null) {
                    Debug.LogWarning(panel.BackgroundImage);
                    _background.sprite = panel.BackgroundImage;
                }
                
            }
        }
        private void ClearContent(){
            var children = new Stack<GameObject>();
            for (int i = 0; i < _contentHolder.transform.childCount; i++) {
                children.Push(_contentHolder.transform.GetChild(i).gameObject);
            }

            while (children.Count > 0) {
                var child = children.Pop();
                Destroy(child);
            }
        }

        TimeSpan _availableCommissionRemainingTime = new TimeSpan(0);
        private void Update() {
            switch (ActivePanelName) {
                case nameof(AvailableCommissionPanel):
                    var availableCommissionRemainingTime = _lastAvailableCommissionUpdate.AddHours(_availableCommissionRefreshHours) - DateTime.Now;
                    if (availableCommissionRemainingTime.TotalSeconds <= 0.0001) OpenAvailableCommissionsPanel();
                    if ((int)availableCommissionRemainingTime.TotalSeconds != (int)_availableCommissionRemainingTime.TotalSeconds || _timerText.text == String.Empty) {
                        _timerText.text = $@"Refresh after: {availableCommissionRemainingTime:hh\:mm\:ss}";
                        _availableCommissionRemainingTime = availableCommissionRemainingTime;
                    }
                    break;
                default:
                    if (_timerText.text != String.Empty) _timerText.text = string.Empty;
                    break;
            }
        }
    }

    [Serializable]
    public abstract class MissionPanel {
        [SerializeField] public string PanelName;
        [SerializeField] [CanBeNull] public Sprite BackgroundImage;
    }

    [Serializable]
    public class DailyMissionPanel : MissionPanel {
        [SerializeField] public DailyItemUI ItemPrefab;
    }

    [Serializable]
    public class QuestPanel : MissionPanel {
        [SerializeField] public QuestItemUI ItemPrefab;
    }

    [Serializable]
    public class CommissionPanel : MissionPanel {
        [FormerlySerializedAs("ItemUI")] [SerializeField] public CommissionItemUI ItemPrefab;       
    }

    [Serializable]
    public class AvailableCommissionPanel : MissionPanel
    {
        [SerializeField] public AvailableCommissionItemUI ItemPrefab;
    }
}