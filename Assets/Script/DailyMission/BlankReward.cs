using UnityEngine;

namespace Script.Controller.DailyMission {
    public class BlankReward : IMissionReward {
        public Sprite RewardSprite {
            get => null;
        }

        public void OnComplete() { Debug.LogWarning("Daily reward is not set!"); }
    }
}