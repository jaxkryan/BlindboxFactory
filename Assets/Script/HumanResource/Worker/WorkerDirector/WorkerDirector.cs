using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Controller;
using Script.Machine;
using Script.Machine.WorkDetails;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [RequireComponent(typeof(Worker))]
    public class WorkerDirector : GoapAgent {
        [Header("Sensor")]
        [SerializeField] private Sensor _workMachineSensor;
        
        
        [Header("Locations")]
        [SerializeField] public Transform MachineLocation;

        public Dictionary<CoreType, float> CoreChangePerSec {
            get {
                var dict = new Dictionary<CoreType, float>();
                
                Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ForEach(c => dict.Add(c, 0.0f));
                var coreBonuses = _worker.Bonuses.Where(b => b is CoreChangeBonus).Cast<CoreChangeBonus>().ToHashSet();
                foreach (var b in coreBonuses) {
                    foreach (var c in b.CoreChanges) 
                        dict[c.Key] += c.Value/b.TimeInterval;
                }

                if (_slot) {
                    var workDetails = _slot.Machine.WorkDetails.Where(d => d.IsRunning).Where(d => d is CoreChangeWorkDetail).Cast<CoreChangeWorkDetail>().ToHashSet();
                    foreach (var d in workDetails) {
                        dict[d.Core] += d.Amount;
                    }
                }

                return dict;
            }
        }

        public float EstWorkingTime {
            get {
                if (!_slot) return 0.0f;
                float estTime = _slot.Machine.EstimateCompletionTime;
                float remainingTime = _slot.Machine.EstimateCompletionTime;
                var coreValues = _worker.CurrentCores;
                var minimumCores = new Dictionary<CoreType, float>();
                if (GameController.Instance.WorkerController.WorkerNeedsList.TryGetValue(IWorker.ToWorkerType(_worker),
                        out var needs)) {
                    
                }
                else {
                    Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ForEach(c => minimumCores.Add(c, 0.0f));
                }
                while (ContinueAfterProductCreated(coreValues, CoreChangePerSec, _worker.MaximumCore, minimumCores, _slot.Machine.Product.MaxProgress, remainingTime)) {
                    coreValues = EstCoreValuesWhenWorkDone(coreValues, CoreChangePerSec, _slot.Machine.Product.MaxProgress - remainingTime);
                    estTime += _slot.Machine.Product.MaxProgress;
                    remainingTime = Mathf.Ceil(remainingTime) - remainingTime + _slot.Machine.Product.MaxProgress;
                    if ((remainingTime - _slot.Machine.Product.MaxProgress) > 1f)
                        remainingTime -= Mathf.Floor(remainingTime - _slot.Machine.Product.MaxProgress);
                }

                return estTime;
            }
        }

        public static Dictionary<CoreType, float> EstCoreValuesWhenWorkDone (Dictionary<CoreType, float> cores, Dictionary<CoreType, float> coreChangesPerSec, float estWorkingTime){
                cores.Keys.ForEach(k => cores[k] += coreChangesPerSec[k] * estWorkingTime);

                return cores;
        }

        public static bool ContinueAfterProductCreated(Dictionary<CoreType, float> coreValues, Dictionary<CoreType, float> coreChangesPerSec, Dictionary<CoreType, float> max, Dictionary<CoreType, float> min, float totalTime, float workedTime = 0f) {
            var newCores = EstCoreValuesWhenWorkDone(coreValues, coreChangesPerSec, totalTime - workedTime);

            return newCores.Any(c => c.Value > max[c.Key] || c.Value < min[c.Key]);
        }

        Worker _worker;
        public MachineSlot TargetSlot {
            get => _slot;
            set {
                _workMachineSensor.Target = value.gameObject;
                _slot = value;
            }
        }
        private MachineSlot _slot;
        
        
        protected override void Awake() {
            base.Awake();
            _worker = GetComponent<Worker>();
            if (_slot) _workMachineSensor.Target = _slot.gameObject;
        }

        protected override void SetupTimers() {
            base.SetupTimers();
        }

        protected override void SetupBeliefs() {
            base.SetupBeliefs();
            BeliefFactory bf = new(this, Beliefs);
            
            bf.AddBelief("Nothing", () => false);
            
            
            bf.AddBelief($"{_worker.Name}Idle", () => !_navMeshAgent.hasPath);
            bf.AddBelief($"{_worker.Name}Walking", () => _navMeshAgent.hasPath);
            _worker.BonusManager.BonusConditions.Select(b => b.Key).ForEach(bonus => {
                var condition = _worker.BonusManager.BonusConditions.GetValueOrDefault(bonus);
                bf.AddBelief($"{_worker.Name}HasBonus: {bonus.Name}", () => condition.IsApplicable(_worker));
            });
            bf.AddBelief($"{_worker.Name}HasWorkableMachine", () => GameController.Instance.MachineController.FindWorkableMachines(_worker).Any());
            bf.AddBelief($"{_worker.Name}HasNoWorkableMachine", () => !GameController.Instance.MachineController.FindWorkableMachines(_worker).Any());
            bf.AddBelief($"{_worker.Name}WishListedAMachine", () => GameController.Instance.MachineController.Machines.Any(m => m.Slots.Any(s => s.WishListWorker != null && (Worker)s.WishListWorker == _worker)));
            
            
            foreach (CoreType core in Enum.GetValues(typeof(CoreType))) {
                var workerType = IWorker.ToWorkerType(_worker);
                var needDict = new Dictionary<CoreType, int>();
                if (GameController.Instance.WorkerController.WorkerNeedsList.TryGetValue(workerType, out var needs)) 
                    needs.ForEach(n => needDict.Add(n.Key, n.Value)); 
                else 
                    Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ForEach(c => needDict.Add(c, 0));
                bf.AddBelief($"{_worker.Name}{Enum.GetName(typeof(CoreType), core)}NeedsDepleted", () => 
                    _worker.CurrentCores[core] < needDict.GetValueOrDefault(core));
                bf.AddBelief($"{_worker.Name}{Enum.GetName(typeof(CoreType), core)}NeedsFulfilled", () => 
                    _worker.CurrentCores[core] < needDict.GetValueOrDefault(core));
                //Beliefs for machines that improve cores
                throw new NotImplementedException();
            }
            
            bf.AddSensorBelief("MachineInWorkingRange", _workMachineSensor);
            bf.AddBelief($"{_worker.Name}Working", () => _worker.Machine is not null);
        }

        protected override void SetupGoals() {
            base.SetupGoals();
            
            Goals.Add(new AgentGoal.Builder("Chill")
                .WithPriority(1)
                .WithDesiredEffects(Beliefs["Nothing"])
                .Build());
            Goals.Add(new AgentGoal.Builder("Wander")
                .WithPriority(1)
                .WithDesiredEffects(Beliefs[$"{_worker.Name}Walking"])
                .Build());
            Goals.Add(new AgentGoal.Builder("Work")
                .WithPriority(2)
                .WithDesiredEffects(Beliefs[$"{_worker.Name}Working"])
                .Build());
            foreach (CoreType core in Enum.GetValues(typeof(CoreType))) {
                Goals.Add(new AgentGoal.Builder($"Keep{core}CoreUp")
                    .WithPriority(3)
                    .WithDesiredEffects(Beliefs[$"{_worker.Name}{Enum.GetName(typeof(CoreType), core)}NeedsFulfilled"])
                    .Build());
            }
        }

        protected override void SetupActions() {
            base.SetupActions();

            Actions.Add(new AgentAction.Builder("Wander")
                .WithStrategy(new WanderStrategy(_navMeshAgent, 10f))
                .AddEffect(Beliefs["Nothing"])
                .Build());
        }
    }
}