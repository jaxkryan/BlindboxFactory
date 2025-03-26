using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using MyBox;
using Script.Controller;
using Script.Machine;
using UnityEngine;
using UnityEngine.AI;

namespace Script.HumanResource.Worker {
    [Serializable]
    [RequireComponent(typeof(WorkerDirector))]
    [DisallowMultipleComponent]
    public abstract class Worker : MonoBehaviour, IWorker {
        [Header("Identity")]
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
        [CanBeNull] public IMachine Machine {get; private set;}
        [CanBeNull] public MachineSlot WorkingSlot {get; private set;}
        public List<Bonus> Bonuses { get => _bonuses; }
        [SerializeReference, SubclassSelector] private List<Bonus> _bonuses;
        public event Action onWorking = delegate { };
        public event Action onStopWorking = delegate { };

        [Header("Animation")] 
        [SerializeField] private RuntimeAnimatorController _runtimeAnimator;
        private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
        private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
        public Animator Animator {
            get => GetComponent<Animator>();
        }

        public NavMeshAgent Agent {
            get => GetComponent<NavMeshAgent>();
        } 
        public void UpdateCore(CoreType core, float amount, bool trigger = true) {
            var current = _currentCores[core];
            var max = _maximumCores[core];
            float NewAmount() => current + amount;
            if (NewAmount() > max) amount = max - current;
            if (NewAmount() < 0) amount = current;
            
            if (Mathf.Approximately(NewAmount(), current)) return;
            
            _currentCores[core] = NewAmount();
            Bonus.RecalculateBonuses(this);
            if (trigger) onCoreChanged?.Invoke(core, amount);
        }
        
        public virtual void StartWorking(MachineSlot slot) {
            if (Machine is not null) {
                Debug.LogError($"{Name} is already working");
                return;
            }

            if (!ReferenceEquals(slot.CurrentWorker, this)) {
                Debug.LogError($"Machine slot ({slot.Machine}) is not being worked by {Name}");
                return;
            }
            
            WorkingSlot = slot;
            Machine = slot.Machine;
            Agent.enabled = false;
            transform.position = WorkingSlot.transform.position;
            onWorking?.Invoke();
        }
        public virtual void StopWorking() {
            if (Machine is null || WorkingSlot is null) {
                Debug.LogError($"{Name} is not working");
                return;
            }

            if (!ReferenceEquals(WorkingSlot.CurrentWorker, this)) {
                Debug.LogError($"Machine slot ({WorkingSlot.Machine}) is being worked by {Name}");
                return;
            }

            WorkingSlot = null;
            Machine = null;
            Agent.enabled = true;
            if (NavMesh.SamplePosition(transform.position, out var hit, Single.MaxValue, 1))
                transform.position = hit.position;
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
            Agent.updateRotation = false;
            Agent.updateUpAxis = false;
            _director = GetComponent<WorkerDirector>();
            _currentCores = new();
            if (_runtimeAnimator) Animator.runtimeAnimatorController = _runtimeAnimator;
        }

        private void Start() {
            foreach (CoreType coreType in Enum.GetValues(typeof(CoreType))) {
                if (!_currentCores.ContainsKey(coreType) && _startingCores.ContainsKey(coreType)) {
                    _currentCores.Add(coreType, _startingCores[coreType]);
                }
            }
        }
        
        private void OnValidate() {
            if (_startingCores is null) _startingCores = new();
            if (_maximumCores is null) _maximumCores = new();
            foreach (CoreType coreType in Enum.GetValues(typeof(CoreType))) {
                _startingCores.TryAdd(coreType, 0);
                _maximumCores.TryAdd(coreType, 100);
            }
        }

        private void Update() {
            _bonuses.ForEach(b => b.OnUpdate(Time.deltaTime));
            
            Animator.SetFloat(HorizontalMovement, Agent.velocity.x);
            Animator.SetFloat(VerticalMovement, Agent.velocity.y);
        }

        public virtual SaveData Save() => new SaveData() {
            Name = Name,
            Description = Description,
            PortraitIndex = GameController.Instance.WorkerController.PortraitSprites.Any(p => p == Portrait)
            ? GameController.Instance.WorkerController.PortraitSprites.FirstIndex(p => p == Portrait)
            : 0,
            Position = transform.position,
            MaximumCores = MaximumCore,
            StartingCores = _startingCores,
            CurrentCores = CurrentCores,
            Bonuses = Bonuses,
            MachinePrefabName = Machine is MachineBase mbf ? mbf?.PrefabName ?? string.Empty : string.Empty,
            MachinePosition = Machine is MachineBase mbp ? mbp?.Position ?? Vector2Int.zero : Vector2Int.zero,
            MachineSlotName = WorkingSlot is not null ? WorkingSlot.name : string.Empty,
        };

        public virtual void Load(SaveData data) {
            Name = data.Name;
            _description = data.Description;
            Portrait = GameController.Instance.WorkerController.PortraitSprites.Count >= data.PortraitIndex
                ? GameController.Instance.WorkerController.PortraitSprites[data.PortraitIndex] : default;
            transform.position = data.Position;
            _maximumCores = new SerializedDictionary<CoreType, float>(data.MaximumCores);
            _startingCores = new SerializedDictionary<CoreType, float>(data.StartingCores);
            _currentCores = data.CurrentCores;
            _bonuses = data.Bonuses;

            if (data.MachinePrefabName != string.Empty) {
                var machine = GameController.Instance.MachineController.Machines.FirstOrDefault(m =>
                    m.PrefabName == data.MachinePrefabName && m.Position == data.MachinePosition);
                if (machine is not null) {
                    var slot = machine.Slots.FirstOrDefault(s => s.name == data.MachineSlotName);
                    if (slot is not null && slot.CurrentWorker is null && slot.CanAddWorker(this)) {
                        machine.AddWorker(this, slot);
                    }
                }
            }
            
            
        }

        public class SaveData {
            public string Name;
            public string Description;
            public int PortraitIndex;
            public Vector3 Position;
            public Dictionary<CoreType, float> MaximumCores;
            public Dictionary<CoreType, float> StartingCores;
            public Dictionary<CoreType, float> CurrentCores;
            public List<Bonus> Bonuses;
            public string MachinePrefabName;
            public Vector2Int MachinePosition;
            public string MachineSlotName;
        }
    }
}