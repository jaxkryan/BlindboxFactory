using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MyBox;
using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using Newtonsoft.Json;
using Script.Controller.SaveLoad;
using Script.HumanResource.Administrator;
using Script.HumanResource.Worker;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Script.Controller {
    public class GameController : PersistentSingleton<GameController> {
        public MachineController MachineController = new();
        public BoxController BoxController = new();
        public WorkerController WorkerController = new();
        public ShardController ShardController = new();
        public MascotController MascotController = new();
        public ResourceController ResourceController = new();
        public PowerGridController PowerGridController = new();
        public QuestController QuestController = new();
        public CommissionController CommissionController = new();
        public DailyMissionController DailyMissionController = new();
        public WorkerSpawner WorkerSpawner = new();
        public Transform WorkerSpawnPoint;
        public Grid Grid;
        [FormerlySerializedAs("Background")] public Tilemap Ground;
        public Tilemap ConstructionLayer;
        public Tilemap CollisionLayer;
        public NavMeshSurface NavMeshSurface;
        public TileBase GroundTile;

        [Space] 
        [Header("Save")] 
        [SerializeField]
        public bool HasSaveTimer;
        [ConditionalField(nameof(HasSaveTimer))] 
        [SerializeField]
        public float MinutesBetweenSave = 5f;
        public SaveManager SaveManager;
        private Timer _saveTimer;
        
        
        public List<Vector2Int> GroundAddedTiles = new();

        private List<ControllerBase> _controllers =>
            typeof(GameController).GetFields()
                .Where(f => f.FieldType.IsSubclassOf(typeof(ControllerBase)))
                .Where(f => ((ControllerBase)f.GetValue(this)) != null)
                .Select(fieldInfo => (ControllerBase)fieldInfo.GetValue(this))
                .ToList();

        protected override void Awake() {
            base.Awake();
            _controllers.ForEach(c => c.OnAwake());
            SaveManager = new();
        }

        public void BuildNavMesh() {
            Debug.Log("Rebuilding NavMesh");
            Physics2D.SyncTransforms();
            NavMeshSurface.BuildNavMesh();
        }

        private void OnDestroy() => _controllers.ForEach(c => c.OnDestroy());

        private void OnApplicationQuit() {
            _controllers.ForEach(c => c.OnApplicationQuit());
            Task.Run(async () => await Save(SaveManager));
        }
        
        private void OnEnable() => _controllers.ForEach(c => c.OnEnable());

        private void OnDisable() => _controllers.ForEach(c => c.OnDisable());

        private void Start() {
            if (HasSaveTimer) {
                _saveTimer = new CountdownTimer(MinutesBetweenSave * 60);
                _saveTimer.OnTimerStop += () => Task.Run(async () => await OnSaveTimerOnTimerStop(SaveManager));
                _saveTimer.Start();
            }

            //Retrieve changes to the ground tilemap 
            Tilemap.tilemapTileChanged += (tilemap, tiles) => {
                if (tilemap is null) return;
                if (tilemap == Ground) {
                    foreach (var tile in tiles) {
                        if (tile.tile is null) GroundAddedTiles.Remove(tile.position.ToVector2Int());
                        else GroundAddedTiles.Add(tile.position.ToVector2Int());
                    }
                }
            };

            _controllers.ForEach(c => c.OnStart());
            Task.Run(async () => await LoadOnStart(SaveManager));
            BuildNavMesh();
            return;

            async Task LoadOnStart(SaveManager saveManager) { await Load(saveManager); }

            async Task OnSaveTimerOnTimerStop(SaveManager saveManager) {
                await Save(saveManager);
                _saveTimer.Start();
            }
        }

        private void Update() {
            _controllers.ForEach(c => c.OnUpdate(Time.deltaTime));
            _saveTimer?.Tick(Time.deltaTime);
        }

        private void OnValidate() => _controllers.ForEach(c => c.OnValidate());

        private async Task Load(SaveManager saveManager) {
            Debug.LogWarning("Loading");
            await SaveManager.LoadFromCloud();
            await SaveManager.LoadFromLocal();

            #region Game Controller's own save

            if (saveManager.SaveData.TryGetValue(nameof(HasSaveTimer), out string hasSaveTimerString)) {
                HasSaveTimer = hasSaveTimerString == bool.TrueString; 
            }

            if (saveManager.SaveData.TryGetValue(nameof(MinutesBetweenSave), out string minutesBetweenSaveString)) {
                if (float.TryParse(minutesBetweenSaveString, out var minutesBetweenSave))
                    MinutesBetweenSave = minutesBetweenSave;
            }

            if (saveManager.SaveData.TryGetValue(nameof(GroundAddedTiles), out string groundAddedTilesString)) {
                var list = JsonConvert.DeserializeObject<List<V2Int>>(groundAddedTilesString);

                list.Select(v => (Vector2Int)v).ForEach(v => Ground.SetTile(v.ToVector3Int(), GroundTile));
            }

            #endregion
            
            _controllers.ForEach(c => c.Load(saveManager));
        }

        private async Task Save(SaveManager saveManager) {
            Debug.LogWarning("Saving");
            try { 
                _controllers.ForEach(c => c.Save(saveManager));

                #region Game Controller's own save
                saveManager.SaveData.AddOrUpdate(nameof(HasSaveTimer), HasSaveTimer ? bool.TrueString : bool.FalseString, (s, s1) => HasSaveTimer ? bool.TrueString : bool.FalseString);
                saveManager.SaveData.AddOrUpdate(nameof(MinutesBetweenSave), MinutesBetweenSave.ToString(CultureInfo.InvariantCulture),
                    (s, s1) => MinutesBetweenSave.ToString(CultureInfo.InvariantCulture));
                saveManager.SaveData.AddOrUpdate(nameof(GroundAddedTiles), JsonConvert.SerializeObject(GroundAddedTiles.Select(V2Int.ToV2Int)), (s, s1) => JsonConvert.SerializeObject(GroundAddedTiles));
                #endregion
            }
            catch (System.Exception e) {
                Debug.LogWarning(e);
                throw;
            }

            Debug.LogWarning("Saving to file");
            await SaveManager.SaveToLocal();
            await SaveManager.SaveToCloud();
        }

        private struct V2Int {
            public int x;
            public int y;
            public static V2Int ToV2Int(Vector2Int v) => new V2Int() { x = v.x, y = v.y };
            public static implicit operator Vector2Int(V2Int v) => new Vector2Int(v.x, v.y);
        }
    }
}