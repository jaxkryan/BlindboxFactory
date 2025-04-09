using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyBox;
using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using Newtonsoft.Json;
using Script.Controller.Permissions;
using Script.Controller.SaveLoad;
using Script.HumanResource.Administrator;
using Script.HumanResource.Worker;
using Script.Utils;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Script.Controller {
    [RequireComponent(typeof(UnityMainThreadDispatcher))]
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
            PermissionHandler.RequestPermissionIfNeeded(Permission.ExternalStorageWrite);
            PermissionHandler.RequestPermissionIfNeeded(Permission.ExternalStorageRead);
            var dispatcher = UnityMainThreadDispatcher.Instance;
        }

        public void BuildNavMesh() {
            Debug.Log("Rebuilding NavMesh");
            Physics2D.SyncTransforms();
            NavMeshSurface.BuildNavMesh();
        }

        private void OnDestroy() => _controllers.ForEach(c => c.OnDestroy());

        private void OnApplicationQuit() => _controllers.ForEach(c => c.OnApplicationQuit());
        
        private void OnEnable() => _controllers.ForEach(c => c.OnEnable());

        private void OnDisable() => _controllers.ForEach(c => c.OnDisable());

        private IEnumerator Start() {
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
            Application.wantsToQuit += WantsToQuit;
            StartCoroutine(LoadOnStart(SaveManager).AsCoroutine());
            BuildNavMesh();
            yield break;

            async Task LoadOnStart(SaveManager saveManager) { await Load(saveManager); }

            async Task OnSaveTimerOnTimerStop(SaveManager saveManager) {
                await Save(saveManager);
                _saveTimer.Start();
            }
        }

        private bool _isSaving = false;
        private bool _quitNow = false;
        private bool WantsToQuit() {
            if (_quitNow) return true;
            if (_isSaving)
            {
                Debug.Log("Quit requested, but save is in progress.");
                return false; // Block quitting while saving
            }

            StartCoroutine(SaveAndQuit());
            return false; //Hold quitting until save finishes
        }

        private IEnumerator SaveAndQuit() {
            if (_isSaving)
            {
                Debug.Log("Save already in progress.");
                yield break;
            }

            _isSaving = true;

            Debug.Log("Saving game...");
            yield return Save(SaveManager).AsCoroutine();

            Debug.Log("Save complete. Quitting app.");
            _isSaving = false;
            _quitNow = true;
#if UNITY_EDITOR
            //Application.Quit() does not work in the editor so
            // this need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
        }

        private void Update() {
            _controllers.ForEach(c => c.OnUpdate(Time.deltaTime));
            _saveTimer?.Tick(Time.deltaTime);
        }

        private void OnValidate() => _controllers.ForEach(c => c.OnValidate());

        private async Task Load(SaveManager saveManager) {
            Debug.Log($"Loading game on {Thread.CurrentThread} thread");
            await SaveManager.LoadFromCloud();
            await SaveManager.LoadFromLocal();


            try { 
                #region Game Controller's own save

                if (saveManager.SaveData.TryGetValue(nameof(HasSaveTimer), out string hasSaveTimerString)) {
                    HasSaveTimer = hasSaveTimerString == bool.TrueString; 
                }

                if (saveManager.SaveData.TryGetValue(nameof(MinutesBetweenSave), out string minutesBetweenSaveString)) {
                    if (float.TryParse(minutesBetweenSaveString, out var minutesBetweenSave))
                        MinutesBetweenSave = minutesBetweenSave;
                }

                if (saveManager.SaveData.TryGetValue(nameof(GroundAddedTiles), out string groundAddedTilesString)) {
                    var list =SaveManager.Deserialize<List<V2Int>>(groundAddedTilesString);

                    list.Select(v => (Vector2Int)v).ForEach(v => Ground.SetTile(v.ToVector3Int(), GroundTile));
                }
                #endregion
                _controllers.ForEach(c => {
                    Debug.Log($"Loading {c.GetType().Name}");
                    c.Load(saveManager);
                }); 
                
            }
            catch (System.Exception ex) { Debug.LogError(ex); }
        }

        private async Task Save(SaveManager saveManager) {
            Debug.Log($"Saving game on {Thread.CurrentThread} thread");
            try {
                _controllers.ForEach(c => {
                    Debug.Log($"Saving {c.GetType().Name}");
                    c.Save(saveManager);
                });


                #region Game Controller's own save
                saveManager.SaveData.AddOrUpdate(nameof(HasSaveTimer), HasSaveTimer ? bool.TrueString : bool.FalseString, (s, s1) => HasSaveTimer ? bool.TrueString : bool.FalseString);
                saveManager.SaveData.AddOrUpdate(nameof(MinutesBetweenSave), MinutesBetweenSave.ToString(CultureInfo.InvariantCulture),
                    (s, s1) => MinutesBetweenSave.ToString(CultureInfo.InvariantCulture));
                saveManager.SaveData.AddOrUpdate(nameof(GroundAddedTiles), SaveManager.Serialize(GroundAddedTiles.Select(V2Int.ToV2Int).ToList()), (s, s1) => SaveManager.Serialize(GroundAddedTiles.Select(V2Int.ToV2Int).ToList()));
                #endregion
            }
            catch (System.Exception e) {
                Debug.LogWarning(e);
            }

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