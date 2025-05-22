using System;
using System.Collections.Generic;
using ZLinq;
using AYellowpaper.SerializedCollections;
using BuildingSystem;
using JetBrains.Annotations;
using MyBox;
using Script.Controller;
using Script.Machine;
using Script.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Script.HumanResource.Worker {
    [Serializable]
    [RequireComponent(typeof(WorkerDirector))]
    [DisallowMultipleComponent]
    public abstract class Worker : MonoBehaviour, IWorker {
        [Header("Identity")]
        public string Name {
            get => _name;
            set => _name = value;
        }

        [SerializeField] private EmployeeName _name;

        public string Description {
            get => _description;
            set => _description = value;
        }

        [SerializeField] private string _description;

        public Sprite Portrait {
            get => _portrait;
            set => _portrait = value;
        }

        [SerializeField] private Sprite _portrait;

        public WorkerDirector Director {
            get => _director;
            set => _director = value;
        }

        private WorkerDirector _director;

        [Header("Cores")]
        public Dictionary<CoreType, float> MaximumCore {
            get => _maximumCores;
        }

        [SerializeField] private SerializedDictionary<CoreType, float> _maximumCores;
        [SerializeField] private SerializedDictionary<CoreType, float> _startingCores;

        public Dictionary<CoreType, float> CurrentCores {
            get => _currentCores;
        }

        [SerializeField] private SerializedDictionary<CoreType, float> _currentCores;
        public event Action<CoreType, float> onCoreChanged = delegate { };

        [Header("Work")] [CanBeNull] public IMachine Machine { get; private set; }
        [CanBeNull] public MachineSlot WorkingSlot { get; private set; }

        public List<Bonus> Bonuses {
            get => _bonuses;
        }

        [SerializeReference, SubclassSelector] private List<Bonus> _bonuses;
        public event Action onWorking = delegate { };
        public event Action onStopWorking = delegate { };

        [Header("Animation")] [SerializeField] private RuntimeAnimatorController _runtimeAnimator;
        private SpriteRenderer _renderer;
        private static readonly int VerticalMovement = Animator.StringToHash("VerticalMovement");
        private static readonly int HorizontalMovement = Animator.StringToHash("HorizontalMovement");
        private static readonly int IsWorking = Animator.StringToHash("IsWorking");
        private static readonly int IsResting = Animator.StringToHash("IsResting");
        private static readonly int IsDining = Animator.StringToHash("IsDining");
        private static readonly int OnWorkStateChanged = Animator.StringToHash("OnWorkStateChanged");

        
        private bool _isWorking;
        private bool _isResting;
        private bool _isDining;

        private void SetBool(int hash, bool value) {
            if (hash == IsWorking) {
                if (_isWorking == value) return;
                _isWorking = value;
                Animator.SetBool(IsWorking, value);
                Animator.SetTrigger(OnWorkStateChanged);
            }

            if (hash == IsResting) {
                if (_isResting == value) return;
                _isResting = value;
                Animator.SetBool(IsResting, value);
                Animator.SetTrigger(OnWorkStateChanged);
            }

            if (hash == IsDining) {
                if (_isDining == value) return;
                _isDining = value;
                Animator.SetBool(IsDining, value);
                Animator.SetTrigger(OnWorkStateChanged);
            }
        }
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

            SetBool(IsWorking, true);
            var controller = GameController.Instance.MachineController;

            if (controller.IsRecoveryMachine(slot.Machine, this.ToWorkerType(), out var recoveries)) {
                if (recoveries.AsValueEnumerable().Any(r => r.Core == CoreType.Happiness)) SetBool(IsResting, true);
                else if (recoveries.AsValueEnumerable().Any(r => r.Core == CoreType.Hunger)) SetBool(IsDining, true);
            }
            
            
            // //Get prefab name of the working machine
            // var prefabName = slot.Machine.PrefabName;
            // //Get the prefab
            // var prefab = controller.Buildables.Find(prefab => prefab.Name == prefabName)?.gameObject;
            // if (prefab != null && prefab.TryGetComponent<MachineBase>(out var recoveryMachine)) {
            //     //Check if prefab is a resting machine prefab
            //     // var recovery = controller.RecoveryMachines.Any(m => m.Key.GetType() == recoveryMachine.GetType()) 
            //     //     ? controller.RecoveryMachines.FirstOrDefault(m => m.Key.GetType() == recoveryMachine.GetType()) : new ();
            //     // if (recovery is not null && recovery.Any(r => r.Worker == IWorker.ToWorkerType(this))) {
            //     //     var r = recovery.Where(r => r.Worker == IWorker.ToWorkerType(this)).ToList();
            //     //     //Check which core the prefab recover 
            //     //     if (r.Any(re => re.Core == CoreType.Happiness)) Animator.SetBool(IsResting, true);
            //     //     if (r.Any(re => re.Core == CoreType.Hunger)) Animator.SetBool(IsDining, true);
            //     // }
            //     if (controller.IsRecoveryMachine(recoveryMachine, out var forWorkers, out var recoveries)) { }
            // }


            WorkingSlot = slot;
            if (WorkingSlot?.FlipWorker ?? false) _renderer.flipX = true;
            Machine = slot.Machine;
            onWorking?.Invoke();
        }

        public virtual void StopWorking() {
            // if (Machine is null || WorkingSlot is null) {
            //     Debug.LogError($"{Name} is not working");
            //     return;
            // }
            //
            // if (!ReferenceEquals(WorkingSlot.CurrentWorker, this)) {
            //     Debug.LogError($"Machine slot ({WorkingSlot.Machine}) is being worked by {Name}");
            //     return;
            // }

            SetBool(IsWorking, false);
            SetBool(IsDining, false);
            SetBool(IsResting, false);

            if (WorkingSlot?.FlipWorker ?? false) _renderer.flipX = false;
            WorkingSlot = null;
            Machine = null;
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
            
            if (!TryGetComponent(out _renderer)) throw new MissingComponentException(nameof(SpriteRenderer));

            Animator.SetBool(IsWorking, false);
            Animator.SetBool(IsResting, false);
            Animator.SetBool(IsDining, false);
            SetBool(IsWorking, false);
            SetBool(IsResting, false);
            SetBool(IsDining, false);
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

            if (!_isWorking) {
                Animator.SetFloat(HorizontalMovement, Agent.velocity.x);
                Animator.SetFloat(VerticalMovement, Agent.velocity.y);
                if (_workingClipNames.AsValueEnumerable().Any(c => Animator.GetCurrentAnimatorStateInfo(0).IsName(c))) 
                    Animator.SetTrigger(OnWorkStateChanged);
            }
            else if (_workingClipNames.AsValueEnumerable().All(c => !Animator.GetCurrentAnimatorStateInfo(0).IsName(c))) 
                Animator.SetTrigger(OnWorkStateChanged);
            
            SetOrderInLayer();
        }
        
        private static readonly HashSet<string> _workingClipNames = new(){"working_Clip", "eatfront_Clip", "sleep_Clip" };

        protected virtual void SetOrderInLayer() {
            if (Machine != null && Machine is MachineBase mb && mb.TryGetComponent<SpriteRenderer>(out var sr)) {
                GetComponent<SpriteRenderer>().sortingOrder = sr.sortingOrder + 1;
            }
            else {
                GetComponent<SpriteRenderer>().sortingOrder
                    = ConstructionLayer.SortingOrder(new Vector3(transform.position.x, transform.position.y - 1f));
            }
        }

        public virtual SaveData Save() => new SaveData() {
            Name = Name,
            Description = Description,
            PortraitIndex = GameController.Instance.WorkerController.PortraitSprites.AsValueEnumerable().Any(p => p == Portrait)
                ? GameController.Instance.WorkerController.PortraitSprites.FirstIndex(p => p == Portrait)
                : 0,
            Position = new V3(transform.position),
            // MaximumCores = MaximumCore,
            // StartingCores = _startingCores,
            CurrentCores = CurrentCores,
            // Bonuses = Bonuses,
            // MachinePrefabName = Machine is MachineBase mbf ? mbf?.PrefabName ?? string.Empty : string.Empty,
            // MachinePosition = new V3(Machine is MachineBase mbp ? mbp?.Position ?? Vector3.zero : Vector3.zero),
            // MachineSlotName = WorkingSlot is not null ? WorkingSlot.name : string.Empty,
        };

        public virtual void Load(SaveData data) {
            Name = data.Name;
            _description = data.Description;
            Portrait = GameController.Instance.WorkerController.PortraitSprites.Count >= data.PortraitIndex
                ? GameController.Instance.WorkerController.PortraitSprites.AsValueEnumerable().ElementAtOrDefault(data.PortraitIndex - 1)
                : default;
            //NavMesh
            if (TryGetComponent<NavMeshAgent>(out var agent)) {
                var z = GameController.Instance.NavMeshSurface.transform.position.z;
                var targetPosition = ((Vector3)data.Position).ToVector2().ToVector3(z);

                agent.enabled = false;
                agent.transform.position = targetPosition;
                if (NavMesh.SamplePosition(targetPosition, out var hit, 5f,
                        1 << NavMesh.GetAreaFromName("Walkable"))) {
                    agent.transform.position = hit.position;
                    agent.enabled = true;
                }

                agent.enabled = true;
            }

            // _maximumCores = new SerializedDictionary<CoreType, float>(data.MaximumCores);
            // _startingCores = new SerializedDictionary<CoreType, float>(data.StartingCores);
            _currentCores = new(data.CurrentCores);
            // _bonuses = data.Bonuses;

            // if (data.MachinePrefabName != string.Empty) {
            //     var machine = GameController.Instance.MachineController.Machines.FirstOrDefault(m =>
            //         m.PrefabName == data.MachinePrefabName && m.Position == data.MachinePosition);
            //     if (machine is not null) {
            //         var slot = machine.Slots.FirstOrDefault(s => s.name == data.MachineSlotName);
            //         if (slot is not null && slot.CurrentWorker is null && slot.CanAddWorker(this)) {
            //             Director.TargetSlot = slot;
            //             Director.CurrentAction = new AgentAction.Builder("Loading work machine")
            //                 .WithStrategy(new WorkStrategy(this))
            //                 .Build();
            //             // machine.AddWorker(this, slot);
            //         }
            //     }
            // }
        }

        public class SaveData {
            public string Name;
            public string Description;
            public int PortraitIndex;
            public V3 Position;
            public Dictionary<CoreType, float> MaximumCores;
            public Dictionary<CoreType, float> StartingCores;

            public Dictionary<CoreType, float> CurrentCores;

            // public List<Bonus> Bonuses;
            public string MachinePrefabName;
            public V3 MachinePosition;
            public string MachineSlotName;
        }
    }
}