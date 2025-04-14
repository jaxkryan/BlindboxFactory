using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MyBox;
using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using Newtonsoft.Json;
using Script.Alert;
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
        [SerializeField][Min(0.1f)]
        public float MinutesBetweenSave = 5f;
        [SerializeField] private int _maxSavesCount = 10;
        public SaveManager SaveManager;
        private Timer _saveTimer;
        

        public int SessionCount {get; private set;}
        public bool CompletedTutorial { get; private set; } = false;
        public long TotalPlaytime { get; private set; } = 0;
        public long SessionPlaytime { get; private set; } = 0;
        [CanBeNull] public string SessionId { get; private set; } = null;
        public DateTime SessionStartTime { get; private set; } = DateTime.MinValue;
        
        [Space][Header("Log")]
        [SerializeField] private bool _log;
        public bool Log => _log;
        
        
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
            var pool = WorkerPooling.Instance;
        }
        
        public void BuildNavMesh() {
            if (Log) Debug.Log("Rebuilding NavMesh");
            Physics2D.SyncTransforms();
            NavMeshSurface.BuildNavMesh();
        }

        public void FinishTutorial() => CompletedTutorial = true; 

        private void OnDestroy() => _controllers.ForEach(c => c.OnDestroy());

        private void OnApplicationQuit() => _controllers.ForEach(c => c.OnApplicationQuit());
        
        private void OnEnable() => _controllers.ForEach(c => c.OnEnable());

        private void OnDisable() => _controllers.ForEach(c => c.OnDisable());

        private IEnumerator Start() {
            if (HasSaveTimer) {
                _saveTimer = new CountdownTimer(MinutesBetweenSave * 60);
                _saveTimer.Start();
                _saveTimer.OnTimerStop += () => StartCoroutine(OnSaveTimerOnTimerStop(SaveManager).AsCoroutine());
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
            StartCoroutine(LoadThenStartSession());
            BuildNavMesh();
            yield break;


            IEnumerator LoadThenStartSession() {
                yield return StartCoroutine(LoadOnStart(SaveManager).AsCoroutine());
                StartNewSession();
            }
            async Task LoadOnStart(SaveManager saveManager) { await Load(saveManager); }

            void StartNewSession() {
                if (SessionId is null) {
                    SessionId = Guid.NewGuid().ToString();
                    SessionCount++;
                    SessionStartTime = DateTime.UtcNow;
                }
            }

            async Task OnSaveTimerOnTimerStop(SaveManager saveManager) {
                await Save(saveManager);
                _saveTimer.Start();
            }
        }

        #region Quiting
        private bool _isSaving = false;
        private bool _quitNow = false;
        private bool WantsToQuit() {
            if (_quitNow) return true;
            if (_isSaving)
            {
                if (Log) Debug.Log("Quit requested, but save is in progress.");
                return false; // Block quitting while saving
            }

            StartCoroutine(SaveAndQuit());
            return false; //Hold quitting until save finishes
        }

        private IEnumerator SaveAndQuit() {
                if (Log) Debug.Log("Save and quiting.");
            
            if (_isSaving)
            {
                if (Log) Debug.Log("Save already in progress.");
                yield break;
            }

            _isSaving = true;

            if (Log) Debug.Log("Saving game...");
            yield return Save(SaveManager).AsCoroutine();

            if (Log) Debug.Log("Save complete. Quitting app.");
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
        #endregion

        private float _playtimeTimer = 0;
        private void Update() {
            _controllers.ForEach(c => c.OnUpdate(Time.deltaTime));
            _saveTimer?.Tick(Time.deltaTime);
            
            if (SessionId != null) {
                _playtimeTimer += Time.deltaTime;
                if (_playtimeTimer > 1) {
                    _playtimeTimer -= 1;
                    TotalPlaytime++;
                    SessionPlaytime++;
                }
            }
        }

        private void OnValidate() => _controllers.ForEach(c => c.OnValidate());

        private async Task Load(SaveManager saveManager) {
            if (Log) Debug.Log($"Loading game on {Thread.CurrentThread} thread");
            await SaveManager.LoadFromCloud();
            await SaveManager.LoadFromLocal();


            try {
                #region Game Controller's own save

                if (saveManager.SaveData.TryGetValue(nameof(SessionCount), out string sessionCountString)) {
                    if (int.TryParse(sessionCountString, out var sessionCount)) SessionCount = sessionCount;
                }
                if (saveManager.SaveData.TryGetValue(nameof(CompletedTutorial), out string completedTutorialString)) {
                    CompletedTutorial = completedTutorialString == bool.TrueString;
                }
                if (saveManager.SaveData.TryGetValue(nameof(TotalPlaytime), out string totalPlaytimeString)) {
                    if (long.TryParse(totalPlaytimeString, out var totalPlaytime)) {
                        if (!saveManager.SaveData.TryGetValue(nameof(SessionId), out var saveSessionId)) TotalPlaytime += totalPlaytime;
                        TotalPlaytime = saveSessionId == SessionId ? TotalPlaytime : totalPlaytime + TotalPlaytime;
                    }
                }

                if (saveManager.SaveData.TryGetValue(nameof(HasSaveTimer), out string hasSaveTimerString)) {
                    HasSaveTimer = hasSaveTimerString == bool.TrueString;
                }

                if (saveManager.SaveData.TryGetValue(nameof(MinutesBetweenSave), out string minutesBetweenSaveString)) {
                    if (float.TryParse(minutesBetweenSaveString, out var minutesBetweenSave))
                        MinutesBetweenSave = minutesBetweenSave;
                }

                if (saveManager.SaveData.TryGetValue(nameof(GroundAddedTiles), out string groundAddedTilesString)) {
                    var list = SaveManager.Deserialize<List<V2Int>>(groundAddedTilesString);

                    list.Select(v => (Vector2Int)v).ForEach(v => Ground.SetTile(v.ToVector3Int(), GroundTile));
                }

                #endregion

                _controllers.ForEach(c => {
                    if (Log) Debug.Log($"Loading {c.GetType().Name}");
                    c.Load(saveManager);
                });

            }
            catch (System.Exception ex) {
                Debug.LogError(ex);
                ex.RaiseException();
            }
        }

        private async Task Save(SaveManager saveManager) {
            if (Log) Debug.Log($"Saving game on {Thread.CurrentThread} thread");
            try {
                _controllers.ForEach(c => {
                    if (Log) Debug.Log($"Saving {c.GetType().Name}");
                    c.Save(saveManager);
                });


                #region Game Controller's own save
                saveManager.SaveData.AddOrUpdate(nameof(SessionCount), SessionCount.ToString(), (s, s1) => SessionCount.ToString());
                saveManager.SaveData.AddOrUpdate(nameof(TotalPlaytime), TotalPlaytime.ToString(), (s, s1) => TotalPlaytime.ToString());
                saveManager.SaveData.AddOrUpdate(nameof(SessionPlaytime), SessionPlaytime.ToString(), (s, s1) => SessionPlaytime.ToString());
                saveManager.SaveData.AddOrUpdate(nameof(SessionId), SessionId, (s, s1) => SessionId);
                saveManager.SaveData.AddOrUpdate(nameof(SessionStartTime), SessionStartTime.Ticks.ToString(), (s, s1) => SessionStartTime.Ticks.ToString());
                saveManager.SaveData.AddOrUpdate(nameof(CompletedTutorial), CompletedTutorial ? bool.TrueString : bool.FalseString, (s, s1) => CompletedTutorial ? bool.TrueString : bool.FalseString);
                saveManager.SaveData.AddOrUpdate(nameof(HasSaveTimer), HasSaveTimer ? bool.TrueString : bool.FalseString, (s, s1) => HasSaveTimer ? bool.TrueString : bool.FalseString);
                saveManager.SaveData.AddOrUpdate(nameof(MinutesBetweenSave), MinutesBetweenSave.ToString(CultureInfo.InvariantCulture),
                    (s, s1) => MinutesBetweenSave.ToString(CultureInfo.InvariantCulture));
                saveManager.SaveData.AddOrUpdate(nameof(GroundAddedTiles), SaveManager.Serialize(GroundAddedTiles.Select(t => new V2Int(t)).ToList()), (s, s1) => SaveManager.Serialize(GroundAddedTiles.Select(t => new V2Int(t)).ToList()));
                #endregion
            }
            catch (System.Exception e) {
                Debug.LogWarning(e);
                e.RaiseException();
            }

            await SaveManager.SaveToLocal();
            await SaveManager.SaveToCloud();
        }
    }
}