using UnityEngine;

namespace Script.Controller.DailyMission {
    public abstract class MissionReward : ScriptableObject, IMissionReward {
        public Sprite RewardSprite { get => _rewardSprite; }
        [SerializeField] Sprite _rewardSprite;
        public abstract void OnComplete();
    }
}