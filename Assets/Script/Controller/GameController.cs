using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.HumanResource.Administrator;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Script.Controller {
    public class GameController : PersistentSingleton<GameController> {
        public MachineController MachineController = new ();
        public BoxController BoxController = new ();
        public WorkerController WorkerController = new ();
        public ShardController ShardController = new ();
        public MascotController MascotController = new();
        public ResourceController ResourceController = new();
        public PowerGridController PowerGridController = new();
        public QuestController QuestController = new();
        public Grid Grid;
        public Tilemap ConstructionLayer;
        public Tilemap CollisionLayer;
        [SerializeField] private bool _hasSaveTimer;
        [ConditionalField(nameof(_hasSaveTimer))]
        [SerializeField] private float _timeBetweenSave = 5f;
        private Timer _saveTimer;
        private List<ControllerBase> _controllers => typeof(GameController).GetFields()
                .Where(f => f.FieldType.IsSubclassOf(typeof(ControllerBase)))
                .Where(f => ((ControllerBase)f.GetValue(this)) != null)
                .Select(fieldInfo => (ControllerBase)fieldInfo.GetValue(this))
                .ToList();
        
        protected override void Awake() {
            base.Awake();
            Load();
            if (!Grid) {
                Grid = FindFirstObjectByType<Grid>();
            }
            _controllers.ForEach(c => c.OnAwake());
        }

        private void OnDestroy() => _controllers.ForEach(c => c.OnDestroy());
        private void OnEnable() => _controllers.ForEach(c => c.OnEnable());

        private void OnDisable() {
            _controllers.ForEach(c => c.OnDisable());
            Save();
        }

        private void Start() {
            if (_hasSaveTimer) {
                _saveTimer = new CountdownTimer(_timeBetweenSave);
                _saveTimer.OnTimerStop += () => {
                    Save();
                    _saveTimer.Start();
                };
                _saveTimer.Start();
            }
            _controllers.ForEach(c => c.OnStart());
        }

        private void Update() {
            _controllers.ForEach(c => c.OnUpdate(Time.deltaTime));
            _saveTimer?.Tick(Time.deltaTime);
        }

        private void OnValidate() => _controllers.ForEach(c => c.OnValidate());

        private void Load() { 
            _controllers.ForEach(c => c.Load());
        }

        private void Save() {
            _controllers.ForEach(c => c.Save());
        }

    }
}