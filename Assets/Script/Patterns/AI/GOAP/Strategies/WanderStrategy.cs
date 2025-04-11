using UnityEngine;
using UnityEngine.AI;

public class WanderStrategy : IActionStrategy {
    readonly NavMeshAgent agent;
    readonly float wanderRadius;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 0.5f && !agent.pathPending;

    public WanderStrategy(NavMeshAgent agent, float wanderRadius) {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
    }

    public void Start() {
        int retry = 10;
        while (retry > 0) {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitCircle * wanderRadius);
            randomDirection.z = 0;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, NavMesh.AllAreas)
                && Vector3.Distance(hit.position, agent.transform.position) > wanderRadius / 2) {
                
                agent.SetDestination(hit.position);
                return;
            }

            retry--;
        }
    }
}