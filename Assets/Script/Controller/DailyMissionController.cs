using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class DailyMissionController : ControllerBase {
        public override void Load() { throw new System.NotImplementedException(); }
        public override void Save() { throw new System.NotImplementedException(); }
        private DateTime _lastUpdate = DateTime.MinValue;
        [SerializeField] private TimeAsSerializable _resetTime;
        [SerializeField] private List<Quest.Quest> _dailyMissionsList;
        private HashSet<Quest.Quest> _dailyMissions;
        
        public HashSet<Quest.Quest> DailyMissions {
            get => _dailyMissions?.ToHashSet() ?? CreateDailyMissions();
        }

        private HashSet<Quest.Quest> CreateDailyMissions(bool save = false) {
            var list = new List<Quest.Quest>();

            if (_dailyMissions.IsNullOrEmpty()) Debug.LogError("Daily mission list is empty!");
            else {
                list = _dailyMissionsList;
                _dailyMissionsList.ForEach(q => q.ClearQuestData());
            }

            if (save) _dailyMissions = list.ToHashSet();
            return list.ToHashSet();
        }


        public override void OnUpdate(float deltaTime) {
            base.OnUpdate(deltaTime);

            if ((DateTime.Today <= _lastUpdate.Date || _resetTime.AsDateTime() > DateTime.Now)
                && _dailyMissions is not null) return;
            CreateDailyMissions(true);
            _lastUpdate = DateTime.Now;
        }
    }

    [Serializable]
    public struct TimeAsSerializable {
        public int Hour;
        public int Minute;

        public static DateTime ToDateTime(TimeAsSerializable time) {
            return DateTime.Today.AddHours(time.Hour).AddMinutes(time.Minute);
        }

        public static explicit operator DateTime(TimeAsSerializable time) => ToDateTime(time);
    }

    public static class TimeAsSerializableExtensions {
        public static DateTime AsDateTime (this TimeAsSerializable time) => TimeAsSerializable.ToDateTime(time);
        
    }
}