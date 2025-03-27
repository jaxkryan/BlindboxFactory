using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Machine.Products;
using UnityEngine;

public class WorkStrategy : IActionStrategy {
    public bool CanPerform => !Complete;
    public bool Complete { get; private set; }

    private MachineSlot _slot ;
    readonly Worker _worker;
    
    public WorkStrategy(Worker worker) {
        _worker = worker;
        Complete = false;
    }

    public void Start() {
        _slot = _worker.Director.TargetSlot;
        _slot.Machine.AddWorker(_worker, _slot);

        if (!ReferenceEquals(_worker.Machine, _slot.Machine)) {
            Debug.LogError("Adding worker to machine failed.");
            return;
        }

        _slot.Machine.onCreateProduct += ConsiderStopWorking;
        _worker.onStopWorking += () => _slot.Machine.onCreateProduct -= ConsiderStopWorking;
    }

    private void ConsiderStopWorking(ProductBase obj) {
        var min = new Dictionary<CoreType, float>();
        Enum.GetValues(typeof(CoreType)).Cast<CoreType>()
            .ForEach(c => min[c] = 0);
        if (!WorkerDirector
                .ContinueAfterProductCreated(
                    _worker.CurrentCores
                    , _worker.Director.CoreChangePerSec
                    , _worker.MaximumCore
                    , min
                    , 0f)) 
            _slot.Machine.RemoveWorker(_worker);
        
        var controller = GameController.Instance.MachineController;
        List<MachineBase> recoveryMachine = new();
        foreach (var coreType in Enum.GetValues(typeof(CoreType)).Cast<CoreType>()) {
            recoveryMachine.AddRange(controller.FindRecoveryMachine(coreType, (Worker)_worker));
        }

        if (_worker.CurrentCores.Any(c => c.Value <= 0f)
            && recoveryMachine.All(r => r != _slot.Machine)) _slot.Machine.RemoveWorker(_worker);

        if (_worker.CurrentCores.Any(c => c.Value >= _worker.MaximumCore[c.Key])
            && recoveryMachine.Any(r => r == _slot.Machine)) _slot.Machine.RemoveWorker(_worker);
    }
}