using UnityEngine;
using UnityEngine.AI;

public class WanderStrategy : IActionStrategy {
    readonly NavMeshAgent agent;
    readonly float wanderRadius;

    public bool CanPerform => !Complete;
    public bool Complete => agent.remainingDistance <= 2f && !agent.pathPending;

    public WanderStrategy(NavMeshAgent agent, float wanderRadius) {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
    }

    public void Start() {
        for (int i = 0; i < 5; i++) {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius);
            randomDirection.y = 0;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1)) {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}