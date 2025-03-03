using System;
using Script.HumanResource.Worker;
using UnityEngine;
using UnityEngine.AI;

public class MoveStrategy : IActionStrategy {
    readonly NavMeshAgent agent;
    readonly Func<Vector3> destination;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination) {
        this.agent = agent;
        this.destination = destination;
    }

    public void Start() => agent.SetDestination(destination());
    public void Stop() => agent.ResetPath();
}

public class MoveToSlotStrategy : IActionStrategy {
    readonly NavMeshAgent agent;
    readonly Worker worker;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 0.01f && !agent.pathPending;

    public MoveToSlotStrategy(Worker worker) {
        this.agent = worker.Agent;
        this.worker = worker;
    }

    public void Start() {
        if (worker.Director.TargetSlot is null) agent.SetDestination(worker.transform.position); 
        else agent.SetDestination(worker.Director.TargetSlot.transform.position);
    }
    public void Stop() => agent.ResetPath();
}