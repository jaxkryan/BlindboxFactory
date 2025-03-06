using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Script.Controller.DailyMission {
    [Obsolete]
    public interface IMissionReward {
        [CanBeNull] Sprite RewardSprite { get; }
        void OnComplete();
    }
}