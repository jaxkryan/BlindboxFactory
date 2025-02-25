using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [Serializable]
    public class CoreChangeBonus : Bonus{
        public override string Description { get => _description.DescriptionFormatter(
            ); }
        
        [TextArea]
        [SerializeField] private string _description;
        
        private Timer _timer;
        [SerializedDictionary("Core", "Amount")]
        public SerializedDictionary<CoreType, float> CoreChanges;

        [SerializeField][Min(0.1f)] public float TimeInterval = 1f;

        public override void OnStart() {
            base.OnStart();

            _timer = new CountdownTimer(TimeInterval);
            _timer.OnTimerStop += OnTimerStop;
        }

        private void OnTimerStop() {
            CoreChanges.ForEach(c => Worker.UpdateCore(c.Key, c.Value));
            _timer.Start();
        }

        public override void OnStop() {
            base.OnStop();
            
            _timer.Pause();
        }
        
        
    }
}