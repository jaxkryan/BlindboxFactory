using System;
using System.Collections.Generic;
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
        [Header("Hunger")]
        [SerializeField] [Min(1f)]private float _maximumHunger = 1f;
        [SerializeField] [Min(1f)]private float _startingHunger = 1f;
        public float CurrentHunger { get => _currentHunger; set => _currentHunger = value; }
        private float _currentHunger = -10f;
        public CoreDrain HungerDrain { get => _hungerDrain; set => _hungerDrain = value; }
        [SerializeField] private CoreDrain _hungerDrain;
        private float _hungerCoreDrainTimeInterval = 0;
        public event Action<float> onHungerChanged;
        [Header("Happiness")]
        [SerializeField] [Min(1f)]private float _maximumHappiness = 1f;
        [SerializeField] [Min(1f)]private float _startingHappiness = 1f;
        public float CurrentHappiness { get => _currentHappiness; set => _currentHappiness = value; }
        private float _currentHappiness = -10f;
        public CoreDrain HappinessDrain { get => _happinessDrain; set => _happinessDrain = value; }
        [SerializeField] private CoreDrain _happinessDrain;

        private float _happinessCoreDrainTimeInterval = 0;
        public event Action<float> onHappinessChanged;
        public event Action onCoreDrained;
        
        [Header("Work")]
        [SerializeField] private MachineBase _machine;
        public IMachine Machine { get => _machine; set => _machine = (MachineBase)value; }
        public IEnumerable<Bonus> Bonuses { get => _bonuses;  }
        [SerializeReference, SubclassSelector] private List<Bonus> _bonuses;
        public event Action onWorking;
        public event Action onStopWorking;
        public void DrainCores(DrainType drainType) {
            switch (drainType) {
                case DrainType.Work:
                    _currentHunger -= _hungerDrain.DrainOnWork;
                    onHungerChanged?.Invoke(_currentHunger);
                    _currentHappiness -= _happinessDrain.DrainOnWork;
                    onHappinessChanged?.Invoke(_currentHunger);
                    break;
                case DrainType.Time:
                    if (_hungerDrain.TimeInterval <= _hungerCoreDrainTimeInterval) {
                        _currentHunger -= _hungerDrain.DrainOverTime;
                        _hungerCoreDrainTimeInterval -= _hungerDrain.TimeInterval;
                        onHungerChanged?.Invoke(_currentHunger);
                    }

                    if (_happinessDrain.TimeInterval <= _happinessCoreDrainTimeInterval) {
                        _currentHappiness -= _happinessDrain.DrainOverTime;
                        _happinessCoreDrainTimeInterval -= _happinessDrain.TimeInterval;
                        onHappinessChanged?.Invoke(_currentHunger);
                    }
                    break;
                default:
                    Debug.LogError($"Invalid drain type: {drainType}");
                    return;
            }

            onCoreDrained?.Invoke();
        }

        public void RefillHunger(float amount) {
            if (_maximumHunger <= _currentHunger) {
                Debug.LogWarning("Worker is fulled.");
                return;
            }
            amount = _currentHunger + amount <= _maximumHunger ? amount : _maximumHunger - _currentHunger;
            _currentHunger += amount;
            onHungerChanged?.Invoke(_currentHunger);
        }

        public void RefillHappiness(float amount) {
            if (_maximumHappiness <= _currentHappiness) {
                Debug.LogWarning("Worker is fulled.");
                return;
            }
            amount = _currentHappiness + amount <= _maximumHappiness ? amount : _maximumHappiness - _currentHappiness;
            _currentHappiness += amount;
            onHappinessChanged?.Invoke(_currentHappiness);
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
            if (_currentHunger < -1f) _currentHunger = +_startingHunger;
            if (_currentHappiness < -1f) _currentHappiness = +_startingHappiness;
        }
        
        private void FixedUpdate() {
            _hungerCoreDrainTimeInterval += Time.fixedDeltaTime;
            _happinessCoreDrainTimeInterval += Time.fixedDeltaTime;
            if (_hungerCoreDrainTimeInterval >= _hungerDrain.TimeInterval || _happinessCoreDrainTimeInterval >= _happinessDrain.TimeInterval) 
                DrainCores(DrainType.Time);
        }

        private void Update() {
            _bonuses.ForEach(b => b.OnUpdate(Time.deltaTime));
        }
    }
}