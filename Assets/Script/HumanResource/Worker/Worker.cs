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
        [SerializeField] private EmployeeName _name;
        public string Description { get => _description; set => _description = value; }
        [SerializeField] private string _description;
        public Sprite Portrait { get => _portrait; set => _portrait = value; }
        [SerializeField] private Sprite _portrait;
        public WorkerDirector Director { get => _director; set => _director = value; }
        private WorkerDirector _director;
        [Header("Cores")] 
        public Dictionary<CoreType, float> MaximumCore {
            get => _maximumCores;
        }
        [SerializeField] private SerializedDictionary<CoreType, float> _maximumCores;
        [SerializeField] private SerializedDictionary<CoreType, float> _startingCores;
        public Dictionary<CoreType, float> CurrentCores { get => _currentCores; }
        private Dictionary<CoreType, float> _currentCores;
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
        public void UpdateCore(CoreType core, float amount) {
            var current = _currentCores[core];
            var max = _maximumCores[core];
            float NewAmount() => current + amount;
            if (NewAmount() > max) amount = max - current;
            if (NewAmount() < 0) amount = current;
            
            if (Mathf.Approximately(NewAmount(), current)) return;
            
            _currentCores[core] = NewAmount();
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
            bonus.Worker = this;
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
            }
        }
        
        private void OnValidate() {
            foreach (CoreType coreType in Enum.GetValues(typeof(CoreType))) {
                _startingCores.TryAdd(coreType, 0);
                _maximumCores.TryAdd(coreType, 100);
            }
        }

        private void Update() {
            _bonuses.ForEach(b => b.OnUpdate(Time.deltaTime));
        }
    }
}