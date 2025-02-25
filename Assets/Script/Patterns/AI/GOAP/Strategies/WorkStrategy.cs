using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using UnityEngine;

public class WorkStrategy : IActionStrategy {
    public bool CanPerform => !Complete;
    public bool Complete { get; private set; }

    private readonly MachineSlot _slot ;
    readonly IWorker _worker;
    
    public WorkStrategy(IWorker worker) {
        _slot = worker.Director.TargetSlot;
        _worker = worker;
        Complete = false;
    }

    public void Start() {
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
    }
}