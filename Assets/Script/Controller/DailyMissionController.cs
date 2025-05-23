using System;
using System.Collections.Generic;
using ZLinq;
using MyBox;
using Newtonsoft.Json;
using Script.Alert;
using Script.Controller.SaveLoad;
using Script.Quest;
using Script.Utils;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class DailyMissionController : ControllerBase {
        private DateTime _lastUpdate = DateTime.MinValue;
        [SerializeField] private TimeAsSerializable _resetTime;
        [SerializeField] private List<Quest.Quest> _dailyMissionsList;
        private List<Quest.Quest> _dailyMissions;
        
        public List<Quest.Quest> DailyMissions {
            get => _dailyMissions ?? CreateDailyMissions(true);
        }

        private List<Quest.Quest> CreateDailyMissions(bool save = false) {
            var list = new List<Quest.Quest>();

            if (_dailyMissionsList.IsNullOrEmpty()) Debug.LogError("Daily mission list is empty!");
            else {
                list = _dailyMissionsList.Clone();
            }

            if (save) {
                _dailyMissions?.ForEach(q => q.ClearQuestData());
                _dailyMissions = list;
            }

            list.ForEach(m => m.Evaluate());
            return list;
        }


        public override void OnUpdate(float deltaTime) {
            base.OnUpdate(deltaTime);

            if ((DateTime.Today <= _lastUpdate.Date || _resetTime.AsDateTime() > DateTime.Now)
                && _dailyMissions is not null) return;
            CreateDailyMissions(true); //Tu add de debug clear
            _lastUpdate = DateTime.Now;
        }

        public override void Load(SaveManager saveManager) {
            
            try {
                if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                      || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data) return;

                DailyMissions.ForEach(d => d.State = Quest.QuestState.Locked);
                _lastUpdate = data.LastUpdate;
                for (var i = 0; i < data.DailyMissionsState.Count; i++) {
                    _dailyMissions[i].State = data.DailyMissionsState[i];
                }           

                DailyMissions.ForEach(q => q.Evaluate());
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot load {GetType()}");
                Debug.LogException(ex);
                ex.RaiseException();
                return;
            }
        }

        public override void Save(SaveManager saveManager) {
            DailyMissions.ForEach(q => q.Evaluate());

            var newSave = new SaveData() {
                LastUpdate = _lastUpdate,
                DailyMissionsState = _dailyMissions?.AsValueEnumerable().Select(m => m.State).ToList() ?? new (),
            };
            
            
            try {
                var serialized = SaveManager.Serialize(newSave);
                saveManager.AddOrUpdate(this.GetType().Name, serialized);
                // if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                //     || SaveManager.Deserialize<SaveData>(saveData) is SaveData data)
                //     saveManager.SaveData.TryAdd(this.GetType().Name,
                //         SaveManager.Serialize(newSave));
                // else
                //     saveManager.SaveData[this.GetType().Name]
                //         = SaveManager.Serialize(newSave);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot save {GetType()}");
                Debug.LogException(ex);                
                ex.RaiseException();
            }
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