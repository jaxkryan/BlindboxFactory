using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.UI.Mission {
    public class MissionHubUI : MonoBehaviour {
        [SerializeField] [Range(1, 3)] private int _defaultPanelIndex = 1;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private Image _background;
        [SerializeField] private GameObject _contentHolder;
        [SerializeField] private DailyMissionPanel _dailyMissionPanel;


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
            Setup();
            
            //Change panel name
            _name.text = _dailyMissionPanel.PanelName;
            //Change panel background
            _background.sprite = _dailyMissionPanel.BackgroundImage;

            var controller = GameController.Instance.DailyMissionController;
            var list = controller.DailyMissions.Select(m => {
                //Instantiate mission from prefab and populate content
                var mission = Instantiate(_dailyMissionPanel.ItemPrefab.gameObject, _contentHolder.transform);
                if (mission.TryGetComponent<DailyItemUI>(out var item)) {
                    item.Quest = m;
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
            Setup();
        }

        public void OpenQuestPanel() {
            Setup();
        }

        private void Setup() {
            gameObject.SetActive(true);
            ClearContent();
        }
        private void ClearContent(){
            var children = new Stack<GameObject>();
            for (int i = 0; i < transform.childCount; i++) {
                children.Push(transform.GetChild(i).gameObject);
            }

            while (children.Count > 0) {
                var child = children.Pop();
                Destroy(child);
            }
        }
    }

    [Serializable]
    public class MissionPanel {
        [SerializeField] public string PanelName;
        [SerializeField] public Sprite BackgroundImage;
    }

    [Serializable]
    public class DailyMissionPanel : MissionPanel {
        [SerializeField] public DailyItemUI ItemPrefab;
    }
}