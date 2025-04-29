using System;
using System.Collections.Generic;
using System.Linq;
using ZLinq;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using Script.Controller;
using Script.Machine;
using Script.Machine.WorkDetails;
using Script.Patterns.AI.GOAP.Strategies;
using Script.Utils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using ZLinq.Linq;

namespace Script.HumanResource.Worker {
    [RequireComponent(typeof(Worker))]
    public class WorkerDirector : GoapAgent {
        [FormerlySerializedAs("_workMachineSensor")] [Header("Sensor")] [SerializeField] public Sensor WorkMachineSensor;


        [Header("Locations")] [SerializeField] [CanBeNull]
        public Transform MachineLocation;

        public Dictionary<CoreType, float> CoreChangePerSec {
            get {
                var dict = new Dictionary<CoreType, float>();

                Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ForEach(c => dict.Add(c, 0.0f));
                var coreBonuses = _worker.Bonuses.AsValueEnumerable().Where(b => b is CoreChangeBonus).Cast<CoreChangeBonus>().ToHashSet();
                foreach (var b in coreBonuses) {
                    foreach (var c in b.CoreChanges)
                        dict[c.Key] += c.Value / b.TimeInterval;
                }

                if (_slot) {
                    var workDetails = _slot.Machine.WorkDetails.AsValueEnumerable().Where(d => d.IsRunning)
                        .Where(d => d is CoreChangeWorkDetail).Cast<CoreChangeWorkDetail>().ToHashSet();
                    foreach (var d in workDetails) { dict[d.Core] += d.IncreaseBy; }
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
                        out var needs)) { needs.ForEach(n => minimumCores.Add(n.Key, n.Value)); }
                else { Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ForEach(c => minimumCores.Add(c, 0.0f)); }

                while (ContinueAfterProductCreated(coreValues, CoreChangePerSec, _worker.MaximumCore, minimumCores,
                           _slot.Machine.MaxProgress, remainingTime)) {
                    coreValues = EstCoreValuesWhenWorkDone(coreValues, CoreChangePerSec,
                        _slot.Machine.MaxProgress - remainingTime);
                    estTime += _slot.Machine.MaxProgress;
                    remainingTime = Mathf.Ceil(remainingTime) - remainingTime + _slot.Machine.MaxProgress;
                    if ((remainingTime - _slot.Machine.MaxProgress) > 1f)
                        remainingTime -= Mathf.Floor(remainingTime - _slot.Machine.MaxProgress);
                }

                return estTime;
            }
        }

        public static Dictionary<CoreType, float> EstCoreValuesWhenWorkDone(Dictionary<CoreType, float> cores,
            Dictionary<CoreType, float> coreChangesPerSec, float estWorkingTime) {
            var collection = new Dictionary<CoreType, float>(cores);
            cores.Keys.ForEach(k => collection[k] += coreChangesPerSec[k] * estWorkingTime);

            return collection;
        }

        public static bool ContinueAfterProductCreated(Dictionary<CoreType, float> coreValues,
            Dictionary<CoreType, float> coreChangesPerSec, Dictionary<CoreType, float> max,
            Dictionary<CoreType, float> min, float totalTime, float workedTime = 0f) {
            var newCores = EstCoreValuesWhenWorkDone(coreValues, coreChangesPerSec, totalTime - workedTime);

            return newCores.AsValueEnumerable().Any(c => c.Value > max[c.Key] || c.Value < min[c.Key]);
        }

        Worker _worker;

        public MachineSlot TargetSlot {
            get => _slot;
            set {
                MachineLocation = value?.transform;
                WorkMachineSensor.Target = value?.Machine.gameObject;
                _slot = value;
            }
        }

        [SerializeField][CanBeNull] private MachineSlot _slot;


        protected override void Awake() {
            base.Awake();
            _worker = GetComponent<Worker>();
            if (_slot) WorkMachineSensor.Target = _slot.gameObject;
        }
        
        protected override void SetupBeliefs() {
            base.SetupBeliefs();
            BeliefFactory bf = new(this, Beliefs);

            bf.AddBelief("Nothing", () => false);


            bf.AddBelief($"{_worker.Name}Idle", () => !_navMeshAgent.hasPath);
            bf.AddBelief($"{_worker.Name}Walking", () => _navMeshAgent.hasPath);
            bf.AddBelief($"{_worker.Name}IsOnNavMesh", () => _navMeshAgent.isOnNavMesh && _isOnWalkableArea(_navMeshAgent));
            bf.AddBelief($"{_worker.Name}IsNotOnNavMesh", () => !_navMeshAgent.isOnNavMesh || !_isOnWalkableArea(_navMeshAgent));
            _worker.Bonuses.ForEach(bonus => {
                var condition = bonus.Condition;
                bf.AddBelief($"{_worker.Name}HasBonus: {bonus.Name}", () => condition.IsApplicable(_worker));
            });
            bf.AddBelief($"{_worker.Name}HasWorkingMachine", () => _workMachines(this).Where(m => m.Slots.Any(s => s.CurrentWorker == null && s.WishListWorker == null)).Any());
            bf.AddBelief($"{_worker.Name}HasWorkableMachine", () => _workableMachines(this).AsValueEnumerable().Any());
            bf.AddBelief($"{_worker.Name}HasNoWorkableMachine", () => !_workableMachines(this).AsValueEnumerable().Any());
            bf.AddBelief($"{_worker.Name}WishListedAMachine",
                () => GameController.Instance.MachineController.Machines.AsValueEnumerable().Any(m =>
                    m.Slots.AsValueEnumerable().Any(s => (Worker)s.WishListWorker == _worker)));
            bf.AddBelief($"{_worker.Name}WishListedMachineIsWorkMachine",
                () => GameController.Instance.MachineController.Machines
                    .AsValueEnumerable().Any(m
                        => m.Slots.AsValueEnumerable().Any(s => (Worker)s.WishListWorker == _worker)
                           && _workMachines(this).Contains(m)));
            bf.AddBelief($"{_worker.Name}HasNoWishListedMachine",
                () => !GameController.Instance.MachineController.Machines.AsValueEnumerable().Any(m =>
                    m.Slots.Any(s => (Worker)s.WishListWorker == _worker)));
            bf.AddBelief($"{_worker.Name}HasTargetMachine", () => _slot?.Machine is not null);
            bf.AddBelief($"{_worker.Name}TargetMachineIsWorkMachine", () => {
                // if (_slot?.Machine is not null) Debug.Log("Condition 1 passed");
                // else Debug.Log("Condition 1 failed");
                // if (_slot?.Machine is not null && !GameController.Instance.MachineController.IsRecoveryMachine(_slot.Machine)) Debug.Log("Condition 2 passed");
                // else Debug.Log("Condition 2 failed");
                
                return _slot?.Machine is not null && (
                    !GameController.Instance.MachineController.IsRecoveryMachine(_slot.Machine, out var forWorker)
                    || !forWorker.Contains(_worker.ToWorkerType()));
            });
            bf.AddBelief($"{_worker.Name}TargetMachineIsRecoveryMachine", () => _slot?.Machine is not null 
                && GameController.Instance.MachineController.IsRecoveryMachine(_slot.Machine, out var forWorker) 
                && forWorker.Contains(_worker.ToWorkerType()));
            bf.AddBelief($"{_worker.Name}IsRested", () => {
                    var needList = GameController.Instance.WorkerController.WorkerNeedsList;

                    if (!needList.ContainsKey(IWorker.ToWorkerType(_worker))) {
                        Debug.LogError($"Worker needs are not configured in game controller: {IWorker.ToWorkerType(_worker)}");
                        return true;
                    }
                    
                    return _worker.CurrentCores.AsValueEnumerable().All(c =>
                        needList.ContainsKey(IWorker.ToWorkerType(_worker))
                        && c.Value > needList[IWorker.ToWorkerType(_worker)]
                            .GetValueOrDefault(c.Key));
                });

            foreach (CoreType core in Enum.GetValues(typeof(CoreType))) {
                var workerType = IWorker.ToWorkerType(_worker);
                var needDict = new Dictionary<CoreType, int>();
                if (GameController.Instance.WorkerController.WorkerNeedsList.TryGetValue(workerType, out var needs))
                    needs.ForEach(n => needDict.Add(n.Key, n.Value));
                else
                    Enum.GetValues(typeof(CoreType)).Cast<CoreType>().ForEach(c => needDict.Add(c, 0));
                bf.AddBelief($"{_worker.Name}{Enum.GetName(typeof(CoreType), core)}NeedsDepleted", () =>
                    _worker.CurrentCores[core] <= needDict.GetValueOrDefault(core) + 1);
                bf.AddBelief($"{_worker.Name}{Enum.GetName(typeof(CoreType), core)}NeedsFulfilled", () =>
                    _worker.CurrentCores[core] > needDict.GetValueOrDefault(core) + 1);
                //Beliefs for machines that improve cores
                switch (core) {
                    case CoreType.Happiness:
                        bf.AddBelief($"{_worker.Name}Has{core}RecoveryMachine",
                            () => _happinessRecoveryMachines(this).Where(m => m.Slots.Any(s => s.CurrentWorker == null && s.WishListWorker == null)).Any());
                        bf.AddBelief($"{_worker.Name}HasNo{core}RecoveryMachine",
                            () => !_happinessRecoveryMachines(this).Where(m => m.Slots.Any(s => s.CurrentWorker == null && s.WishListWorker == null)).Any());
                        bf.AddBelief($"{_worker.Name}WishListedMachineIs{core}RecoveryMachine",
                            () => GameController.Instance.MachineController.Machines
                                .AsValueEnumerable().Any(m => m.Slots
                                                                  .AsValueEnumerable().Any(s => s.WishListWorker != null
                                                                            && (Worker)s.WishListWorker == _worker)
                                                              && _happinessRecoveryMachines(this).Contains(m)));
                        break;
                    case CoreType.Hunger:
                        bf.AddBelief($"{_worker.Name}Has{core}RecoveryMachine",
                            () => _hungerRecoveryMachines(this).Where(m => m.Slots.Any(s => s.CurrentWorker == null && s.WishListWorker == null)).Any() );
                        bf.AddBelief($"{_worker.Name}HasNo{core}RecoveryMachine",
                            () => !_hungerRecoveryMachines(this).Where(m => m.Slots.Any(s => s.CurrentWorker == null && s.WishListWorker == null)).Any());
                        bf.AddBelief($"{_worker.Name}WishListedMachineIs{core}RecoveryMachine",
                            () => GameController.Instance.MachineController.Machines
                                .AsValueEnumerable().Any(m => m.Slots
                                                                  .AsValueEnumerable().Any(s => s.WishListWorker != null
                                                                            && (Worker)s.WishListWorker == _worker)
                                                              && _hungerRecoveryMachines(this).Contains(m)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            bf.AddSensorBelief($"{_worker.Name}AtMachine", WorkMachineSensor);
            bf.AddBelief($"{_worker.Name}Working", () => _worker.Machine is not null && (!GameController.Instance.MachineController.IsRecoveryMachine((MachineBase)_worker.Machine, out var forWorker) || !forWorker.Contains(_worker.ToWorkerType())));
            bf.AddBelief($"{_worker.Name}Resting", () => _worker.Machine is not null && GameController.Instance.MachineController.IsRecoveryMachine((MachineBase)_worker.Machine, out var forWorker) && forWorker.Contains(_worker.ToWorkerType()));
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
            Goals.Add(new AgentGoal.Builder("WarpOnToNavMesh")
                .WithPriority(99)
                .WithDesiredEffects(Beliefs[$"{_worker.Name}IsOnNavMesh"])
                .Build());
        }

        protected override void SetupActions() {
            base.SetupActions();

            Actions.Add(new AgentAction.Builder("Standing")
                .WithStrategy(new IdleStrategy(0.5f))
                .AddEffect(Beliefs["Nothing"])
                .Build());
            Actions.Add(new AgentAction.Builder("Wander")
                .WithStrategy(new WanderStrategy(_navMeshAgent, 10f))
                .AddPrecondition(Beliefs[$"{_worker.Name}IsOnNavMesh"])
                .AddEffect(Beliefs[$"{_worker.Name}Walking"])
                .Build());

            Actions.Add(new AgentAction.Builder("ConsiderWorkMachine")
                .WithStrategy(new WishlistMachineStrategy(_worker, (director) => _workMachines(director).ToList(), _navMeshAgent, 3))
                .AddPrecondition(Beliefs[$"{_worker.Name}HasWorkingMachine"])
                .AddPrecondition(Beliefs[$"{_worker.Name}IsOnNavMesh"])
                // .AddPrecondition(Beliefs[$"{_worker.Name}HasNoWishListedMachine"])
                .AddPrecondition(Beliefs[$"{_worker.Name}IsRested"])
                .AddEffect(Beliefs[$"{_worker.Name}TargetMachineIsWorkMachine"])
                .AddEffect(Beliefs[$"{_worker.Name}HasTargetMachine"])
                .Build());
            Actions.Add(new AgentAction.Builder($"MoveToWorkMachine")
                .WithCost(2)
                .WithStrategy(new MoveToSlotStrategy(_worker))
                .AddPrecondition(Beliefs[$"{_worker.Name}HasTargetMachine"])
                .AddPrecondition(Beliefs[$"{_worker.Name}IsOnNavMesh"])
                .AddPrecondition(Beliefs[$"{_worker.Name}TargetMachineIsWorkMachine"])
                .AddPrecondition(Beliefs[$"{_worker.Name}IsRested"])
                .AddEffect(Beliefs[$"{_worker.Name}AtMachine"])
                .Build());
            Actions.Add(new AgentAction.Builder("WorkAtMachine")
                .WithCost(3)
                .WithStrategy(new WorkStrategy(_worker))
                .AddPrecondition(Beliefs[$"{_worker.Name}HasTargetMachine"])
                .AddPrecondition(Beliefs[$"{_worker.Name}TargetMachineIsWorkMachine"])
                .AddPrecondition(Beliefs[$"{_worker.Name}AtMachine"])
                .AddPrecondition(Beliefs[$"{_worker.Name}IsRested"])
                .AddEffect(Beliefs[$"{_worker.Name}Working"])
                .Build());

            foreach (CoreType core in Enum.GetValues(typeof(CoreType))) {
                Actions.Add(new AgentAction.Builder($"Consider{core}RecoveryMachine")
                    .WithStrategy(new WishlistMachineStrategy(_worker, (director) => _recoveryMachines(director).ToList(),
                        _navMeshAgent, 3))
                    .AddPrecondition(Beliefs[$"{_worker.Name}{Enum.GetName(typeof(CoreType), core)}NeedsDepleted"])
                    // .AddPrecondition(Beliefs[$"{_worker.Name}HasNoWishListedMachine"])
                    .AddPrecondition(Beliefs[$"{_worker.Name}Has{core}RecoveryMachine"])
                    .AddPrecondition(Beliefs[$"{_worker.Name}IsOnNavMesh"])
                    .AddEffect(Beliefs[$"{_worker.Name}TargetMachineIsRecoveryMachine"])
                    .AddEffect(Beliefs[$"{_worker.Name}HasTargetMachine"])
                    .Build());
                Actions.Add(new AgentAction.Builder($"MoveTo{core}RecoveryMachine")
                    .WithCost(2)
                    .WithStrategy(new MoveToSlotStrategy(_worker))
                    .AddPrecondition(Beliefs[$"{_worker.Name}HasTargetMachine"])
                    .AddPrecondition(Beliefs[$"{_worker.Name}IsOnNavMesh"])
                    .AddPrecondition(Beliefs[$"{_worker.Name}TargetMachineIsRecoveryMachine"])
                    .AddEffect(Beliefs[$"{_worker.Name}AtMachine"])
                    .Build());
                Actions.Add(new AgentAction.Builder($"{core}RecoverAtMachine")
                    .WithCost(3)
                    .WithStrategy(new WorkStrategy(_worker))
                    .AddPrecondition(Beliefs[$"{_worker.Name}HasTargetMachine"])
                    .AddPrecondition(Beliefs[$"{_worker.Name}TargetMachineIsRecoveryMachine"])
                    .AddPrecondition(Beliefs[$"{_worker.Name}AtMachine"])
                    .AddEffect(Beliefs[$"{_worker.Name}{Enum.GetName(typeof(CoreType), core)}NeedsFulfilled"])
                    .Build());
            }

            Actions.Add(new AgentAction.Builder("WarpOnToNavMesh")
                .WithCost(0)
                .WithStrategy(new WarpToNavMeshStrategy(_navMeshAgent))
                .AddPrecondition(Beliefs[$"{_worker.Name}IsNotOnNavMesh"])
                .AddEffect(Beliefs[$"{_worker.Name}IsOnNavMesh"])
                .Build());
        }

        Func<WorkerDirector, IEnumerable<MachineBase>> _workableMachines = (director) =>
            GameController.Instance.MachineController.FindWorkableMachines(director._worker);

        private Func<WorkerDirector, ValueEnumerable<ListWhere<MachineBase>,MachineBase>> _happinessRecoveryMachines = (director) => {
            // var list = new List<MachineBase>();
            // list.AddRange(
            return GameController.Instance.MachineController
                .FindRecoveryMachine(CoreType.Happiness, director._worker);
            // );

            // Debug.LogWarning("Happiness recovery machines: " + string.Join(", ", list.Select(r => r.name)));

            // return list;
        };

        private Func<WorkerDirector, ValueEnumerable<ListWhere<MachineBase>,MachineBase>> _hungerRecoveryMachines = (director) => {
            // var list = new List<MachineBase>();
            // list.AddRange(
            return GameController.Instance.MachineController
                .FindRecoveryMachine(CoreType.Hunger, director._worker);

            // Debug.LogWarning("Hunger recovery machines: " + string.Join(", ", list.Select(r => r.name)));
            
            // return list;
        };

        private Func<WorkerDirector, ValueEnumerable< Distinct<Concat<ListWhere<MachineBase>,ListWhere<MachineBase>,MachineBase>,MachineBase>,MachineBase>> _recoveryMachines = (director) => 
            director._happinessRecoveryMachines.Invoke(director)
                .Concat(director._hungerRecoveryMachines.Invoke(director)).Distinct();


        Func<WorkerDirector, ValueEnumerable< Except<FromEnumerable<MachineBase>,Distinct<Concat<ListWhere<MachineBase>,ListWhere<MachineBase>,MachineBase>,MachineBase>,MachineBase>, MachineBase>> _workMachines = (director) => 
            director._workableMachines.Invoke(director).AsValueEnumerable()
                .Except(
                    director._recoveryMachines.Invoke(director));

        private Func<NavMeshAgent, bool> _isOnWalkableArea = (agent) => {
            var walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");
            if (NavMesh.SamplePosition(agent.transform.position, out var hit, 0.5f, NavMesh.AllAreas)) {
                return hit.mask == walkableMask;
            }

            return false;
        };
    }
}