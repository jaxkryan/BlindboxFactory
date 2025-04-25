using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Machine.Products;
using Script.Utils;
using UnityEngine;
using UnityEngine.AI;

public class WorkStrategy : IActionStrategy {
    public bool CanPerform => !Complete;
    public bool Complete { get; private set; }

    public float MinimumCoreValue = 0f;

    private MachineSlot _slot;
    readonly Worker _worker;

    public WorkStrategy(Worker worker) {
        _worker = worker;
        Complete = false;
    }

    public void Start() {
        Complete = false;
        _slot = _worker.Director.TargetSlot;
        if (_slot == null) {
            Complete = true;
            return;
        }

        _slot.Machine.AddWorker(_worker, _slot);

        if (!ReferenceEquals(_worker.Machine, _slot.Machine)) {
            Debug.LogWarning("Adding worker to machine failed.");
            return;
        }

        _worker.Agent.enabled = false;
        _worker.transform.position = _slot.transform.position.ToVector2().ToVector3(_worker.transform.position.z);
        _worker.onCoreChanged += ConsiderStopWorking;
        _slot.Machine.onCreateProduct += ConsiderStopWorking;
        _slot.Machine.onMachineCloseStatusChanged += ConsiderStopWorkingOnCloseStatusChanged;
        _slot.Machine.onMachineDestroyed += StopWorking;

        _worker.onStopWorking += Unsubscribe;
    }

    private void Unsubscribe() {
        _slot.Machine.onCreateProduct -= ConsiderStopWorking;
        _worker.onCoreChanged -= ConsiderStopWorking;
        _slot.Machine.onMachineCloseStatusChanged -= ConsiderStopWorking;
        _slot.Machine.onMachineDestroyed -= StopWorking;

        _worker.onStopWorking -= Unsubscribe;
    }

    public void Update(float deltaTime) {
        if (!Complete &&
            (_slot?.Machine is null
             || _slot.CurrentWorker != _worker
             || !GameController.Instance.MachineController.Machines.Contains(_slot.Machine))) ConsiderStopWorking();
    }

    private void ConsiderStopWorkingOnCloseStatusChanged(bool value) => ConsiderStopWorking(false);
    private void ConsiderStopWorking(ProductBase product) => ConsiderStopWorking(true);
    private void ConsiderStopWorking(CoreType core, float amount) => ConsiderStopWorking(false);
    private void ConsiderStopWorking() => ConsiderStopWorking(false);

    private void ConsiderStopWorking(bool isProductCreate) {
        // var min = new Dictionary<CoreType, float>();
        // Enum.GetValues(typeof(CoreType)).Cast<CoreType>()
        //     .ForEach(c => min[c] = MinimumCoreValue);
        // if (isProductCreate && !WorkerDirector
        //         .ContinueAfterProductCreated(
        //             _worker.CurrentCores
        //             , _worker.Director.CoreChangePerSec
        //             , _worker.MaximumCore
        //             , min
        //             , 0f)){
        //     StopWorking();
        //     return;
        // }
        

        if (_slot?.Machine is null) {
            StopWorking();
            return;
        }
        // else Debug.LogWarning("Machine is not null");

        if (!isProductCreate) {
            if (_slot.Machine.IsClosed) {
                StopWorking();
                return;
            }
        }
        
        if (!_slot.Machine.IsWorkable || !_slot.Machine.gameObject.activeInHierarchy ||
            !GameController.Instance.MachineController.Machines.Contains(_slot.Machine)) {
            StopWorking();
            return;
        }
        // else Debug.LogWarning("Machine is workable and active ");

        //For bb machines
        if (_slot.Machine is BlindBoxMachine bbMachine && bbMachine.amount <= 0) {
            StopWorking();
            return;
        }
        // else Debug.LogWarning("Machine is not bb machine");

        var controller = GameController.Instance.MachineController;
        var workerNeeds
            = GameController.Instance.WorkerController.WorkerNeedsList.GetValueOrDefault(_worker.ToWorkerType()) ??
              new();
        if (!controller.IsRecoveryMachine(_slot.Machine, out var forWorkers, out var recovery) &&
            !forWorkers.Contains(_worker.ToWorkerType())) {
            // Debug.LogWarning("Machine is normal machine");
            //For normal machines
            if (_worker.CurrentCores.Any(c => c.Value <= workerNeeds.GetValueOrDefault(c.Key))) {
                StopWorking();
                return;
            }
            // else Debug.LogWarning("Worker core not depleted");
        }
        else {
            // Debug.LogWarning("Machine is recovering machine");
            //For recovering machines
            var recoveringCore = recovery.Select(r => r.Core);
            if (_worker.CurrentCores.Where(c => recoveringCore.Contains(c.Key)).All(c =>
                    recoveringCore.Contains(c.Key) && c.Value >= _worker.MaximumCore[c.Key])) {
                StopWorking();
                return;
            }
            // else Debug.LogWarning("Worker core not filled");
        }
    }

    private void StopWorking() {
        Complete = true;
    }

    public void Stop() {
            // Debug.LogWarning("Work strategy stopped!");
        _worker.Director.TargetSlot = null;
        _worker.Agent.enabled = true;
        var outPos = _worker.transform.position;
        if (_slot?.Machine != null) {
            outPos = _worker.transform.position +
                     Vector3.Normalize(_slot.Machine.transform.position - _worker.transform.position) *
                     (Vector3.Distance(_slot.Machine.transform.position, _worker.transform.position));
            _slot.Machine.RemoveWorker(_worker);
        }
        else _worker.StopWorking();

        if (NavMesh.SamplePosition(outPos, out var hit, float.MaxValue, NavMesh.AllAreas))
            _worker.Agent.Warp(hit.position);
    }
}