using System;
using System.Linq;
using JetBrains.Annotations;
using Script.Controller.Commission;
using Script.Quest;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Script.UI.Mission {
    public class QuestItemUI : MonoBehaviour {
        [SerializeField] TextMeshProUGUI _name;
        [SerializeField] TextMeshProUGUI _description;
        [SerializeField] TextMeshProUGUI _progress;
        [SerializeField] Image _reward;
        [CanBeNull]
        [Header("Reward sprites")]
        [SerializeField] Sprite _commissionRewardSprite;
        [CanBeNull] [SerializeField] Sprite _blindboxRewardSprite;
        [CanBeNull] [SerializeField] Sprite _newLandRewardSprite;
        [CanBeNull] [SerializeField] Sprite _resourceRewardSprite;
        [CanBeNull] [SerializeField] Sprite _unlockMachineRewardSprite;
        [SerializeField] Sprite _noRewardSprite;
        
        public Quest.Quest Quest { get; set; }

        public void SetQuestData() {
            if (Quest == null) {
                _name.text = "";
                _description.text = "";
                _progress.text = "";
                _reward.sprite = _noRewardSprite;
                Debug.LogWarning($"No quest assigned!");
                return;
            }
            
            Quest.Evaluate();
            bool isCompleted = Quest.State == QuestState.Complete; 
            
            _name.text = Quest.Name;
            _description.text = Quest.Description;
            _progress.text = isCompleted ? "" : string.Join("\n", Quest.Objectives.Select(p => p.Progress(Quest)));
            _reward.sprite = GetSprite();
            Quest.onQuestStateChanged += OnQuesStateChanged;
            Debug.LogWarning(string.Join("\n", Quest.Objectives.Select(p => p.Progress(Quest))));
            
            Sprite GetSprite() {
                if (!Quest.Rewards.Any()) {
                    Debug.LogWarning($"No rewards assigned!");
                    return _noRewardSprite;
                }

                try {
                    return Quest.Rewards.First() switch {
                        CommissionReward commissionReward => _commissionRewardSprite?.texture != null
                            ? _commissionRewardSprite
                            : _noRewardSprite,
                        BlindboxQuestReward blindboxQuestReward => _blindboxRewardSprite?.texture
                            ? _blindboxRewardSprite
                            : _noRewardSprite,
                        NewLandQuestReward newLandQuestReward => _newLandRewardSprite?.texture
                            ? _newLandRewardSprite
                            : _noRewardSprite,
                        ResourceQuestReward resourceQuestReward => _resourceRewardSprite?.texture
                            ? _resourceRewardSprite
                            : _noRewardSprite,
                        UnlockMachineQuestReward unlockMachineQuestReward => _unlockMachineRewardSprite?.texture
                            ? _unlockMachineRewardSprite
                            : _noRewardSprite,
                        _ => _noRewardSprite
                    };
                }
                catch {
                    return _noRewardSprite;
                }
            }
        }

        private void OnDestroy() {
            if (Quest != null) {
                Quest.onQuestStateChanged -= OnQuesStateChanged;
            }
        }

        private void OnQuesStateChanged(Quest.Quest quest, QuestState state) {
            if (quest == Quest) SetQuestData();
        }
    }
}