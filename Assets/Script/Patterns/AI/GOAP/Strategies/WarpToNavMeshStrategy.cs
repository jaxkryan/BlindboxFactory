using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using UnityEngine;
using UnityEngine.AI;

namespace Script.Patterns.AI.GOAP.Strategies {
    public class WarpToNavMeshStrategy : IActionStrategy {
        public bool CanPerform => !Complete;
        public bool Complete { get; private set; } = false;
        private NavMeshAgent _agent;
        private float _radius;
        
        public WarpToNavMeshStrategy(NavMeshAgent agent) {
            _agent = agent.gameObject.GetComponent<NavMeshAgent>();
            _radius = _agent.radius;
        }

        public void Start() {
            var z = GameController.Instance.NavMeshSurface.transform.position.z;
            var targetPosition = ShortestRay(_agent.transform.position.ToVector2().ToVector3(z));

            _agent.enabled = false;
            _agent.transform.position = targetPosition;
            if (NavMesh.SamplePosition(targetPosition, out var hit, 5f,
                    1 << NavMesh.GetAreaFromName("Walkable"))) {
                // _agent.transform.position = Vector3.zero;
                _agent.transform.position = hit.position;
                _agent.enabled = true;
                // _agent.Warp(Vector3.zero);
                // _agent.Warp(hit.position);
                Debug.Log($"Hit NavMesh at {hit.position}");
            }

            _agent.enabled = true;
            Complete = true;
        }


        private Vector3 ShortestRay(Vector3 source, int retries = 0) {
            var maxRetries = 10;
            if (retries >= maxRetries) {
                Debug.LogError("Cannot get ray to walkable NavMesh.");
                return source;
            }

            var plusMinus = new Vector3(1, -1);
            var radius = _radius * (retries + 2);
            var mask = 1 << NavMesh.GetAreaFromName("Walkable");
            var directions = new Vector3[] {
                Vector3.down, -Vector2.one, plusMinus, Vector3.left, Vector3.right, -plusMinus, Vector2.one, Vector3.up,
            };
            var list = new List<NavMeshHit>();

            foreach (var d in directions) {
                if (!NavMesh.SamplePosition(radius * d + source, out var h, radius, mask)) continue;
                if (!NavMesh.Raycast(h.position, source, out var hit, mask)) continue;
                if (hit.distance <= _agent.radius) continue;
                
                list.Add(h);
            }
            
            if (!list.Any()) return ShortestRay(source, ++retries);

            var x = list.OrderByDescending(l => l.distance).Select(l => l.position).First();
            Debug.Log($"Closest point on the navmesh is: {x}. Origin: {source}");
            return x;
        }
    }
}