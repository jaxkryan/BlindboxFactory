using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Machine;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [Serializable]
    [RequireComponent(typeof(WorkerDirector))]
    public abstract class Worker : MonoBehaviour, IWorker {
        [Header("Identity")]
        [SerializeField] private string _id;
        public string Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        [SerializeField] private string _name;
        public string Description { get => _description; set => _description = value; }
        [SerializeField] private string _description;
        public Sprite Portrait { get => _portrait; set => _portrait = value; }
        [SerializeField] private Sprite _portrait;
        public WorkerDirector Director { get => _director; set => _director = value; }
        private WorkerDirector _director;
        [Header("Cores")] 
        [SerializeField] private SerializedDictionary<CoreType, float> _maximumCores;
        [SerializeField] private SerializedDictionary<CoreType, float> _startingCores;
        private Dictionary<CoreType, float> _coreDrainTimeIntervals = new ();
        public Dictionary<CoreType, float> CurrentCores { get => _currentCores; }
        private Dictionary<CoreType, float> _currentCores;
        public Dictionary<CoreType, CoreDrain> CoreDrains { get => _coreDrains; }
        [SerializeField] private SerializedDictionary<CoreType, CoreDrain> _coreDrains;
        public event Action<CoreType, float> onCoreChanged = delegate { };

        [Header("Work")]
        [SerializeField] private MachineBase _machine;
        public IMachine Machine { get => _machine; set => _machine = (MachineBase)value; }
        public BonusManager BonusManager { get => _bonusManager; }
        [SerializeField] BonusManager _bonusManager;
        public HashSet<Bonus> Bonuses { get => _bonuses;  }
        [SerializeReference, SubclassSelector] private HashSet<Bonus> _bonuses;
        public event Action onWorking = delegate { };
        public event Action onStopWorking = delegate { };

        public void DrainCores(DrainType drainType) {
            switch (drainType) {
                case DrainType.Work:
                    foreach (CoreType coreType in Enum.GetValues(typeof(CoreType))) {
                        if (!_coreDrains.ContainsKey(coreType) || (_coreDrains[coreType]?.DrainOnWork ?? 0) == 0) continue;
                        _currentCores[coreType] -= _coreDrains[coreType]?.DrainOnWork ?? 0;
                        onCoreChanged?.Invoke(coreType, -(_coreDrains[coreType]?.DrainOnWork ?? 0));
                    }
                    break;
                case DrainType.Time:
                    foreach (CoreType coreType in Enum.GetValues(typeof(CoreType))) {
                        if (!_coreDrains.ContainsKey(coreType) || (_coreDrains[coreType]?.DrainOnWork ?? 0) == 0) continue;
                        if (!(_coreDrains[coreType].TimeInterval <= _coreDrainTimeIntervals[coreType])) continue;
                        _currentCores[coreType] -= _coreDrains[coreType]?.DrainOverTime ?? 0;
                        _coreDrainTimeIntervals[coreType] -= _coreDrains[coreType]?.TimeInterval ?? _coreDrainTimeIntervals[coreType];
                        onCoreChanged?.Invoke(coreType, -(_coreDrains[coreType]?.DrainOverTime ?? 0));
                    }
                    break;
                default:
                    Debug.LogError($"Invalid drain type: {drainType}");
                    return;
            }
        }

        public void RefillCore(CoreType core, float amount) {
            if (_currentCores[core] >= _maximumCores[core]) {
                Debug.LogWarning($"Core {core} has reached its maximum amount.");
                _currentCores[core] = _maximumCores[core];
                return;
            }
            amount = _currentCores[core] + amount <= _maximumCores[core] ? amount : _maximumCores[core] - _currentCores[core];
            _currentCores[core] += amount;
            onCoreChanged?.Invoke(core, amount);
        }
        public void DoWork() {
            if (_machine is null) {
                Debug.LogError($"No machine assigned to Worker");
                return;
            }
            _machine.AddWorker(this);
            onWorking?.Invoke();
        }
        public void StopWorking() {
            if (_machine is null) {
                Debug.LogError($"No machine assigned to Worker");
                return;
            }
            _machine.RemoveWorker(this);
            onStopWorking?.Invoke();
        }
        public void AddBonus(Bonus bonus) {
            _bonuses.Add(bonus);
            bonus.OnStart();
        }
        public void RemoveBonus(Bonus bonus) {
            _bonuses.Remove(bonus);
            bonus.OnStop();
        }

        private void Awake() {
            _director = GetComponent<WorkerDirector>();
        }

        private void Start() {
            foreach (CoreType coreType in Enum.GetValues(typeof(CoreType))) {
                if (!_currentCores.ContainsKey(coreType) && _startingCores.ContainsKey(coreType)) {
                    _currentCores.Add(coreType, _startingCores[coreType]);
                }

                if (_coreDrainTimeIntervals.ContainsKey(coreType)) {
                    _coreDrainTimeIntervals.Add(coreType, 0);
                }
            }
        }
        
        private void FixedUpdate() {
            foreach (var key in _coreDrainTimeIntervals.Keys) {
                _coreDrainTimeIntervals[key] += Time.fixedDeltaTime;
            }
            if (_coreDrainTimeIntervals.Any(c 
                    => _coreDrains.ContainsKey(c.Key) && c.Value > (_coreDrains[c.Key]?.TimeInterval ?? 9999999999)))
                DrainCores(DrainType.Time);
        }

        private void OnValidate() {
            foreach (CoreType coreType in Enum.GetValues(typeof(CoreType))) {
                _startingCores.TryAdd(coreType, 0);
                _maximumCores.TryAdd(coreType, 100);
                _coreDrainTimeIntervals.TryAdd(coreType, 1);
            }
        }

        private void Update() {
            _bonuses.ForEach(b => b.OnUpdate(Time.deltaTime));
        }
    }
}