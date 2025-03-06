using System;

namespace Script.Controller.DailyMission {
    [Obsolete]
    public class DailyMission {
        public string Name { get; }
        private Func<bool> _condition = () => false;
        public bool Evaluate() => _condition();
        public IMissionReward Reward { get; private set; }
        private DailyMission(string name) => Name = name;

        public class Builder {
            readonly DailyMission _mission;

            public Builder(string name) {
                _mission = new DailyMission(name);
                _mission.Reward = new BlankReward();
            }

            public Builder WithCondition(Func<bool> condition) {
                _mission._condition = condition;
                return this;
            }

            public Builder WithReward(MissionReward reward) {
                _mission.Reward = reward;
                return this;
            }
            
            public DailyMission Build() => _mission;
        }
    }
}