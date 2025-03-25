using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBox;
using Script.Controller.SaveLoad;
using Script.HumanResource.Administrator;
using Script.HumanResource.Worker;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Serialization;
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
        public NavMeshSurface NavMeshSurface;
        
        [Space]
        [Header("Save")]
        [SerializeField] private bool _hasSaveTimer;
        [FormerlySerializedAs("_timeBetweenSave")]
        [ConditionalField(nameof(_hasSaveTimer))]
        [SerializeField] private float _minutesBetweenSave = 5f;
        public SaveManager SaveManager;
        private Timer _saveTimer;
        private List<ControllerBase> _controllers => typeof(GameController).GetFields()
                .Where(f => f.FieldType.IsSubclassOf(typeof(ControllerBase)))
                .Where(f => ((ControllerBase)f.GetValue(this)) != null)
                .Select(fieldInfo => (ControllerBase)fieldInfo.GetValue(this))
                .ToList();
        
        protected override void Awake() {
            base.Awake();
            _controllers.ForEach(c => c.OnAwake());
            SaveManager = new();
        }

        public void BuildNavMesh() => NavMeshSurface?.BuildNavMesh();
        
        private void OnDestroy() => _controllers.ForEach(c => c.OnDestroy());
        private void OnEnable() => _controllers.ForEach(c => c.OnEnable());

        private void OnDisable() {
            _controllers.ForEach(c => c.OnDisable());
            Task.Run(async () => await Save(SaveManager));
        }

        private void Start() {
            if (_hasSaveTimer) {
                _saveTimer = new CountdownTimer(_minutesBetweenSave * 60);
                _saveTimer.OnTimerStop += () => Task.Run(async () => await OnSaveTimerOnTimerStop(SaveManager));
                _saveTimer.Start();
            }

            
            _controllers.ForEach(c => c.OnStart());
            // _controllers.ForEach(c => Debug.LogWarning(c.GetType().Name));
            Task.Run(async () => await LoadOnStart(SaveManager));  
            
            return;

            async Task LoadOnStart(SaveManager saveManager){
                await Load(saveManager);
            }
        }

        private async Task OnSaveTimerOnTimerStop(SaveManager saveManager) {
            await Save(saveManager);
            _saveTimer.Start();
        }

        private void Update() {
            _controllers.ForEach(c => c.OnUpdate(Time.deltaTime));
            _saveTimer?.Tick(Time.deltaTime);
        }

        private void OnValidate() => _controllers.ForEach(c => c.OnValidate());

        private async Task Load(SaveManager saveManager) { 
            await SaveManager.LoadFromCloud();
            await SaveManager.LoadFromLocal();

            _controllers.ForEach(c => c.Load(saveManager));
        }

        private async Task Save(SaveManager saveManager) {
            _controllers.ForEach(c => c.Save(saveManager));
            
            await SaveManager.SaveToLocal();
            await SaveManager.SaveToCloud();
        }

    }
}