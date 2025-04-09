using System;
using Script.HumanResource.Worker;
using UnityEngine;
using UnityEngine.AI;

public class MoveStrategy : IActionStrategy {
    readonly NavMeshAgent _agent;
    readonly Func<Vector3> _destination;

    public bool CanPerform => !Complete;
    public bool Complete => _agent.remainingDistance <= 0.2f && !_agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination) {
        this._agent = agent;
        this._destination = destination;
    }

    public void Start() => _agent.SetDestination(_destination());
    public void Stop() => _agent.ResetPath();
}

public class MoveToSlotStrategy : IActionStrategy {
    readonly NavMeshAgent _agent;
    readonly Worker _worker;

    public bool CanPerform => !Complete;
    public bool Complete => _agent.remainingDistance <= 0.4f && !_agent.pathPending;

    public MoveToSlotStrategy(Worker worker) {
        this._agent = worker.Agent;
        this._worker = worker;
    }

    public void Start() {
        if (_worker.Director.TargetSlot is null) _agent.SetDestination(_worker.transform.position);
        else {
            if (NavMesh.SamplePosition(_worker.Director.TargetSlot.transform.position, out var hit, Single.MaxValue,
                    1)) {
                var newPath = new NavMeshPath();
                if (_agent.CalculatePath(hit.position, newPath)) _agent.SetPath(newPath);
                else _agent.SetDestination(_worker.transform.position);
            }
        }
        Debug.Log($"Slot position is: {_worker.Director.TargetSlot?.transform.position ?? Vector3.zero}.Pathing from: {_agent.transform.position} to {_agent.destination}");
    }
    public void Stop() => _agent.ResetPath();
}