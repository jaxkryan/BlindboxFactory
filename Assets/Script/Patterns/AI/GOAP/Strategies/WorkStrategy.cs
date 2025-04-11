using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Machine.Products;
using UnityEngine;
using UnityEngine.AI;

public class WorkStrategy : IActionStrategy {
    public bool CanPerform => !Complete;
    public bool Complete { get; private set; }
    
    public float MinimumCoreValue = 0f;

    private MachineSlot _slot ;
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
        _worker.transform.position = _slot.transform.position;
        _worker.onCoreChanged += ConsiderStopWorking;
        _slot.Machine.onCreateProduct += ConsiderStopWorking;
        _worker.onStopWorking += Unsubscribe;
        
    }

    private void Unsubscribe() {
        _slot.Machine.onCreateProduct -= ConsiderStopWorking;
        _worker.onCoreChanged -= ConsiderStopWorking;
        _worker.onStopWorking -= Unsubscribe;
    }

    public void Update(float deltaTime) {
        if (!Complete && (_slot?.Machine is null || _slot.CurrentWorker != _worker)) StopWorking();
    }

    private void ConsiderStopWorking(ProductBase product) => ConsiderStopWorking(true);
    private void ConsiderStopWorking(CoreType core, float amount) => ConsiderStopWorking();
    private void ConsiderStopWorking(bool isProductCreate = false) {
        var min = new Dictionary<CoreType, float>();
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
        
        //For bb machines
        if (_slot.Machine is BlindBoxMachine bbMachine && bbMachine.amount <= 0) {
            StopWorking();
            return;
        }
        
        var controller = GameController.Instance.MachineController;
        List<MachineBase> recoveryMachine = new();
        foreach (var coreType in Enum.GetValues(typeof(CoreType)).Cast<CoreType>()) {
            recoveryMachine.AddRange(controller.FindRecoveryMachine(coreType, (Worker)_worker));
        }

        //For normal machines
        if (_worker.CurrentCores.Any(c => c.Value <= 0f)
            && recoveryMachine.All(r => r != _slot.Machine)) {
            StopWorking();
            return;
        }

        //For recovering machines
        if (_worker.CurrentCores.Any(c => c.Value >= _worker.MaximumCore[c.Key])
            && recoveryMachine.Any(r => r == _slot.Machine)) {
            StopWorking();
            return;
        }
    }

    private void StopWorking() {
        Debug.LogWarning($"Worker stop working!");
        Complete = true;
    }

    public void Stop() {
            Debug.LogError($"{GetType().Name} stop!");
        
        if (_slot is null) {
            Debug.LogError("Cannot find worked slot!");
            return;
        }

        var outPos = _worker.transform.position +
                     Vector3.Normalize(_slot.Machine.transform.position - _worker.transform.position) *
                     (Vector3.Distance(_slot.Machine.transform.position, _worker.transform.position) + 0.5f);
            if (NavMesh.SamplePosition(outPos, out var hit, Single.MaxValue, NavMesh.AllAreas))
                _worker.Agent.Warp(hit.position);
            _slot.Machine.RemoveWorker(_worker);

        _worker.Director.TargetSlot = null;
        _worker.Agent.enabled = true;
    }
}