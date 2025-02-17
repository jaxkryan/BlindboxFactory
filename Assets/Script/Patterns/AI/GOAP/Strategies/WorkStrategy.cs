using System;
using System.Linq;
using Script.HumanResource.Worker;
using Script.Machine;
using UnityEngine;

public class WorkStrategy : IActionStrategy {
    public bool CanPerform => !Complete;
    public bool Complete { get; private set; }

    private readonly IMachine _machine ;
    readonly IWorker _worker;
    
    public WorkStrategy(IMachine machine, IWorker worker) {
        _machine = machine;
        _worker = worker;
        Complete = false;
    }

    public void Start() {
        _machine.AddWorker(_worker);

        if (_worker.Machine != _machine) {
            Debug.LogError("Adding worker to machine failed.");
            return;
        }

        _machine.onWorkerChanged += OnWorkerChanged;
        _machine.onCreateProduct += OnCreateProduct;
    }
    
    public void Stop() {
        Complete = true;
        _machine.onWorkerChanged -= OnWorkerChanged;
        _machine.onCreateProduct -= OnCreateProduct;
    }

    private void OnCreateProduct(IProduct product) {
        _machine.RemoveWorker(_worker);
    }

    private void OnWorkerChanged() {
        if (!_machine.Workers.Contains(_worker)) Stop();
    }
}