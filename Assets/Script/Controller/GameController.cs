using System;
using System.Collections.Generic;
using System.Linq;
using Script.Gacha.Shard;
using Script.HumanResource.Administrator;
using UnityEngine;

namespace Script.Controller {
    public class GameController : PersistentSingleton<GameController> {
        public MachineController MachineController = new ();
        public WorkerController WorkerController = new ();
        public ShardController ShardController = new ();
        public AdministratorController AdministratorController = new();

        private List<ControllerBase> _controllers => typeof(GameController).GetFields()
                .Where(f => f.FieldType.IsSubclassOf(typeof(ControllerBase)))
                .Where(f => ((ControllerBase)f.GetValue(this)) != null)
                .Select(fieldInfo => (ControllerBase)fieldInfo.GetValue(this))
                .ToList();
        
        protected override void Awake() {
            base.Awake();

            Load();
            _controllers.ForEach(c => c.OnAwake());
        }

        private void OnDestroy() => _controllers.ForEach(c => c.OnDestroy());
        private void OnEnable() => _controllers.ForEach(c => c.OnEnable());
        private void OnDisable() => _controllers.ForEach(c => c.OnDisable());
        private void Start() => _controllers.ForEach(c => c.OnStart());
        private void Update() => _controllers.ForEach(c => c.OnUpdate(Time.deltaTime));

        private void Load() { }
        
        
        
    }
}