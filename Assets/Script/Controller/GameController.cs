using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBox;
using Script.Controller.SaveLoad;
using Script.HumanResource.Administrator;
using Script.HumanResource.Worker;
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
        public CommissionController CommissionController = new();
        public DailyMissionController DailyMissionController = new();
        public WorkerSpawner WorkerSpawner = new();
        public Transform WorkerSpawnPoint;
        public Grid Grid;
        public Tilemap Background;
        public Tilemap ConstructionLayer;
        public Tilemap CollisionLayer;
        
        [Space]
        [Header("Save")]
        [SerializeField] private bool _hasSaveTimer;
        [ConditionalField(nameof(_hasSaveTimer))]
        [SerializeField] private float _timeBetweenSave = 5f;
        public SaveManager SaveManager;
        private Timer _saveTimer;
        private List<ControllerBase> _controllers => typeof(GameController).GetFields()
                .Where(f => f.FieldType.IsSubclassOf(typeof(ControllerBase)))
                .Where(f => ((ControllerBase)f.GetValue(this)) != null)
                .Select(fieldInfo => (ControllerBase)fieldInfo.GetValue(this))
                .ToList();
        
        protected override void Awake() {
            base.Awake();

            Task.Run(LoadOnAwake);
            return;

            async Awaitable LoadOnAwake(){
                await Load();
                _controllers.ForEach(c => c.OnAwake());
            }
        }

        private void OnDestroy() => _controllers.ForEach(c => c.OnDestroy());
        private void OnEnable() => _controllers.ForEach(c => c.OnEnable());

        private void OnDisable() {
            _controllers.ForEach(c => c.OnDisable());
            Task.Run(Save);
        }

        private void Start() {
            if (_hasSaveTimer) {
                _saveTimer = new CountdownTimer(_timeBetweenSave);
                _saveTimer.OnTimerStop += () => Task.Run(OnSaveTimerOnTimerStop);
                _saveTimer.Start();
            }
            _controllers.ForEach(c => c.OnStart());
        }

        private async Task OnSaveTimerOnTimerStop() {
            await Save();
            _saveTimer.Start();
        }

        private void Update() {
            _controllers.ForEach(c => c.OnUpdate(Time.deltaTime));
            _saveTimer?.Tick(Time.deltaTime);
        }

        private void OnValidate() => _controllers.ForEach(c => c.OnValidate());

        private async Awaitable Load() { 
            await SaveManager.LoadFromCloud();
            await SaveManager.LoadFromLocal();
            
            _controllers.ForEach(c => c.Load());
        }

        private async Awaitable Save() {
            _controllers.ForEach(c => c.Save());
            
            await SaveManager.SaveToLocal();
            await SaveManager.SaveToCloud();
        }

    }
}