using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Newtonsoft.Json;
using Script.Quest;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class DailyMissionController : ControllerBase {
        private DateTime _lastUpdate = DateTime.MinValue;
        [SerializeField] private TimeAsSerializable _resetTime;
        [SerializeField] private List<Quest.Quest> _dailyMissionsList;
        private List<Quest.Quest> _dailyMissions;
        
        public List<Quest.Quest> DailyMissions {
            get => _dailyMissions ?? CreateDailyMissions();
        }

        private List<Quest.Quest> CreateDailyMissions(bool save = false) {
            var list = new List<Quest.Quest>();

            if (_dailyMissionsList.IsNullOrEmpty()) Debug.LogError("Daily mission list is empty!");
            else {
                list = _dailyMissionsList.Clone();
            }

            if (save) {
                _dailyMissions.ForEach(q => q.ClearQuestData());
                _dailyMissions = list;
            }
            return list;
        }


        public override void OnUpdate(float deltaTime) {
            base.OnUpdate(deltaTime);

            if ((DateTime.Today <= _lastUpdate.Date || _resetTime.AsDateTime() > DateTime.Now)
                && _dailyMissions is not null) return;
            CreateDailyMissions(true);
            _lastUpdate = DateTime.Now;
        }

        public override void Load() {
            if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                || JsonConvert.DeserializeObject<SaveData>(saveData) is not SaveData data) return;
            
            _lastUpdate = data.LastUpdate;
            for (var i = 0; i < data.DailyMissionsState.Count; i++) {
                _dailyMissions[i].State = data.DailyMissionsState[i];
            }
        }

        public override void Save() {
            var newSave = new SaveData() {
                LastUpdate = _lastUpdate,
                DailyMissionsState = _dailyMissions.Select(m => m.State).ToList(),
            };
            
            if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                || JsonConvert.DeserializeObject<SaveData>(saveData) is SaveData data) 
                GameController.Instance.SaveManager.SaveData.Add(this.GetType().Name, JsonConvert.SerializeObject(newSave));
            else GameController.Instance.SaveManager.SaveData[this.GetType().Name] = JsonConvert.SerializeObject(newSave);
        }

        public class SaveData {
            public DateTime LastUpdate;
            public List<QuestState> DailyMissionsState;
        }
    }

    [Serializable]
    public struct TimeAsSerializable {
        [MaxValue(24)]public int Hour;
        [MaxValue(60)]public int Minute;

        public static DateTime ToDateTime(TimeAsSerializable time) {
            return DateTime.Today.AddHours(time.Hour).AddMinutes(time.Minute);
        }

        public static explicit operator DateTime(TimeAsSerializable time) => ToDateTime(time);
    }

    public static class TimeAsSerializableExtensions {
        public static DateTime AsDateTime (this TimeAsSerializable time) => TimeAsSerializable.ToDateTime(time);
        
    }
}