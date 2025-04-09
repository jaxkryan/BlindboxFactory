using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Controller;
using Script.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Script.UI.Mission {
    public class MissionHubUI : MonoBehaviour {
        [SerializeField] [Range(1, 3)] private int _defaultPanelIndex = 1;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _background;
        [SerializeField] private GameObject _contentHolder;
        [SerializeField] private DailyMissionPanel _dailyMissionPanel;
        [SerializeField] private QuestPanel _questPanel;
        [SerializeField] private CommissionPanel _commissionPanel;
        [SerializeField] private AvailableCommissionPanel _availableCommissionPanel;


        public void Open() {
            switch (_defaultPanelIndex) {
                case 1:
                    OpenCommissionPanel();
                    break;
                case 2:
                    OpenDailyMissionPanel();
                    break;
                case 3:
                    OpenQuestPanel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OpenDailyMissionPanel() {
            Setup(_dailyMissionPanel);

            var controller = GameController.Instance.DailyMissionController;
            var list = controller.DailyMissions.Select(m => {
                //Instantiate mission from prefab and populate content
                var mission = Instantiate(_dailyMissionPanel.ItemPrefab.gameObject, _contentHolder.transform);
                if (mission.TryGetComponent<DailyItemUI>(out var item)) {
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
            
            var controller = GameController.Instance.CommissionController;
            // var list = controller.Commissions.Select(c => {
            //     var commission = Instantiate(_commissionPanel.ItemPrefab.gameObject, _contentHolder.transform);
            //     if (commission.TryGetComponent<CommissionItemUI>(out var item)) {
            //         //Here
            //     }
            //     else {
            //         Destroy(commission);
            //         Debug.LogWarning(
            //             $"{_commissionPanel.ItemPrefab.gameObject.name} is missing component {nameof(CommissionItemUI)}");
            //     }
            //
            //     return null;
            // }).Where(c => c != null).ToList();
        }

        public void OpenAvailableCommissionsPanel()
        {
            Setup(_availableCommissionPanel);

            var controller = GameController.Instance.CommissionController;
            foreach (var available in controller.CreateCommissions())
            {
                var go = Instantiate(_availableCommissionPanel.ItemPrefab.gameObject, _contentHolder.transform);
                var ui = go.GetComponent<AvailableCommissionItemUI>();
                ui.Commission = available;
                ui.UpdateCommissionData();

                // Optionally bind the accept button behavior
                ui.GetComponentInChildren<Button>().onClick.AddListener(() =>
                {
                    controller.TryAddCommission(available);
                    OpenCommissionPanel(); 
                });
            }
        }

        public void OpenQuestPanel() {
            Debug.LogWarning("Open quest panel");
            Setup(_questPanel);

            var controller = GameController.Instance.QuestController;
            Debug.LogWarning("Instantiating quests");
            controller.Quests.ForEach((q => q.Evaluate()));
            Debug.LogWarning($"List of quests: {string.Join(", ", controller.Quests.Select(q => $"{q.Name}: {q.State}"))}");
            Debug.LogWarning($"List of active quests: {string.Join(", ", controller.Quests.Where(q => q.State == QuestState.InProgress).Select(q => $"{q.Name}: {q.State}"))}");
            var list = controller.Quests.Where(q => q.State == QuestState.InProgress).Select(q => {
                //Instantiate mission from prefab and populate content
                var quest = Instantiate(_questPanel.ItemPrefab.gameObject, _contentHolder.transform);
                if (quest.TryGetComponent<QuestItemUI>(out var item)) {
                    item.Quest = q;
                    item.UpdateQuestData();
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