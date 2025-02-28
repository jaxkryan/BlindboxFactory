using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Script.Controller;
using Script.Machine;
using Script.Machine.WorkDetails;
using Script.Patterns.AI.GOAP.Strategies;
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
                    needs.ForEach(n => minimumCores.Add(n.Key, n.Value));
                }
                else {
                    Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ForEach(c => minimumCores.Add(c, 0.0f));
                }
                while (ContinueAfterProductCreated(coreValues, CoreChangePerSec, _worker.MaximumCore, minimumCores, _slot.Machine.MaxProgress, remainingTime)) {
                    coreValues = EstCoreValuesWhenWorkDone(coreValues, CoreChangePerSec, _slot.Machine.MaxProgress - remainingTime);
                    estTime += _slot.Machine.MaxProgress;
                    remainingTime = Mathf.Ceil(remainingTime) - remainingTime + _slot.Machine.MaxProgress;
                    if ((remainingTime - _slot.Machine.MaxProgress) > 1f)
                        remainingTime -= Mathf.Floor(remainingTime - _slot.Machine.MaxProgress);
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

        protected override void SetupBeliefs() {
            base.SetupBeliefs();
            BeliefFactory bf = new(this, Beliefs);
            
            bf.AddBelief("Nothing", () => false);
            
            
            bf.AddBelief($"{_worker.Name}Idle", () => !_navMeshAgent.hasPath);
            bf.AddBelief($"{_worker.Name}Walking", () => _navMeshAgent.hasPath);
            _worker.Bonuses.ForEach(bonus => {
                var condition = bonus.Condition;
                bf.AddBelief($"{_worker.Name}HasBonus: {bonus.Name}", () => condition.IsApplicable(_worker));
            });
            bf.AddBelief($"{_worker.Name}HasWorkableMachine", () => _workableMachines(this).Any());
            bf.AddBelief($"{_worker.Name}HasNoWorkableMachine", () => !_workableMachines(this).Any());
            bf.AddBelief($"{_worker.Name}WishListedAMachine", () => GameController.Instance.MachineController.Machines.Any(m => m.Slots.Any(s => s.WishListWorker != null && (Worker)s.WishListWorker == _worker)));
            bf.AddBelief($"{_worker.Name}WishListedMachineIsWorkMachine", 
                () => GameController.Instance.MachineController.Machines
                    .Any(m 
                        => m.Slots.Any(s => s.WishListWorker != null 
                                            && (Worker)s.WishListWorker == _worker) 
                           && _workMachines(this).Contains(m)));
            bf.AddBelief($"{_worker.Name}HasNoWishListedMachine", () => GameController.Instance.MachineController.Machines.Any(m => m.Slots.Any(s => s.WishListWorker != null && (Worker)s.WishListWorker == _worker)));
            bf.AddBelief($"{_worker.Name}HasTargetMachine", () => _slot?.Machine is not null);
            
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
                bf.AddBelief($"{_worker.Name}Has{core}RecoveryMachine", () => _recoveryMachines(this, new[]{core}).Any());
                bf.AddBelief($"{_worker.Name}HasNo{core}RecoveryMachine", () => !_recoveryMachines(this, new[]{core}).Any());
                bf.AddBelief($"{_worker.Name}WishListedMachineIs{core}RecoveryMachine", 
                    () => GameController.Instance.MachineController.Machines
                        .Any(m => m.Slots
                                      .Any(s => s.WishListWorker != null 
                                                && (Worker)s.WishListWorker == _worker) 
                                  && _recoveryMachines(this, new[]{core}).Contains(m)));
            }
            
            bf.AddSensorBelief($"{_worker.Name}AtMachine", _workMachineSensor);
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

            Actions.Add(new AgentAction.Builder("Standing")
                .WithStrategy(new IdleStrategy(2f))
                .AddEffect(Beliefs["Nothing"])
                .Build());
            Actions.Add(new AgentAction.Builder("Wander")
                .WithStrategy(new WanderStrategy(_navMeshAgent, 10f))
                .AddEffect(Beliefs[$"{_worker.Name}Walking"])
                .Build());
            Actions.Add(new AgentAction.Builder($"MoveToMachine")
                .WithCost(2)
                .WithStrategy(new MoveStrategy(_navMeshAgent, () => _slot.transform.position))
                .AddPrecondition(Beliefs[$"{_worker.Name}HasTargetMachine"])
                .Build());
            Actions.Add(new AgentAction.Builder("WorkAtMachine")
                .WithCost(3)
                .WithStrategy(new WorkStrategy(_worker))
                .AddPrecondition(Beliefs[$"{_worker.Name}WishListedMachineIsWorkMachine"])
                .AddEffect(Beliefs[$"{_worker.Name}Working"])
                .Build());

            Actions.Add(new AgentAction.Builder("ConsiderWorkMachine")
                .WithStrategy(new WishlistMachineStrategy(_worker, _workMachines(this), _navMeshAgent, 3))
                .AddPrecondition(Beliefs[$"{_worker.Name}HasWorkableMachine"])
                .AddPrecondition(Beliefs[$"{_worker.Name}HasNoWishListedMachine"])
                .AddEffect(Beliefs[$"{_worker.Name}WishListedAMachine"])
                .AddEffect(Beliefs[$"{_worker.Name}HasTargetMachine"])
                .Build());

            foreach (CoreType core in Enum.GetValues(typeof(CoreType))) {
                Actions.Add(new AgentAction.Builder($"Consider{core}RecoveryMachine")
                    .WithStrategy(new WishlistMachineStrategy(_worker, _recoveryMachines(this, new[] { core }),
                        _navMeshAgent, 3))
                    .AddPrecondition(Beliefs[$"{_worker.Name}{Enum.GetName(typeof(CoreType), core)}NeedsDepleted"])
                    .AddPrecondition(Beliefs[$"{_worker.Name}HasNoWishListedMachine"])
                    .AddPrecondition(Beliefs[$"{_worker.Name}Has{core}RecoveryMachine"])
                    .AddEffect(Beliefs[$"{_worker.Name}WishListedAMachine"])
                    .AddEffect(Beliefs[$"{_worker.Name}HasTargetMachine"])
                    .Build());
                Actions.Add(new AgentAction.Builder($"{core}RecoverAtMachine")
                    .WithCost(3)
                    .WithStrategy(new WorkStrategy(_worker))
                    .AddPrecondition(Beliefs[$"{_worker.Name}WishListedMachineIs{core}RecoveryMachine"])
                    .AddEffect(Beliefs[$"{_worker.Name}Working"])
                    .Build());
            }
        }
        
        Func<WorkerDirector, HashSet<MachineBase>> _workableMachines = (director) => GameController.Instance.MachineController.FindWorkableMachines(director._worker).ToHashSet();
        Func<WorkerDirector, CoreType[], HashSet<MachineBase>> _recoveryMachines = (director, cores) =>
            GameController.Instance.MachineController
                .FindWorkableMachines(
                    director._worker
                    , GameController.Instance.MachineController.RecoveryMachines
                        .Where(m 
                            => m.Value
                                .Any(v => v.Worker == IWorker.ToWorkerType(director._worker) && cores.Contains(v.Core)))
                        .Select(m => m.Key)).ToHashSet();
        Func<WorkerDirector, HashSet<MachineBase>> _workMachines = (director) => 
            director._workableMachines.Invoke(director)
                .Except(
                    director._recoveryMachines.Invoke(director, Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ToArray()))
                .ToHashSet();
    }
}