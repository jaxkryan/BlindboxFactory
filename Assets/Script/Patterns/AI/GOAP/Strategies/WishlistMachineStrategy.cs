using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MyBox;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using UnityEngine;
using UnityEngine.AI;

namespace Script.Patterns.AI.GOAP.Strategies {
    public class WishlistMachineStrategy : IActionStrategy {
        public bool CanPerform => !Complete;
        public bool Complete { get; private set; }

        private Worker _worker;
        private Func<WorkerDirector, IEnumerable<MachineBase>> _machines;
        private NavMeshAgent _agent;
        private int _retryAttempts;
        private CountdownTimer _timer;
        
        public WishlistMachineStrategy(Worker worker, Func<WorkerDirector, IEnumerable<MachineBase>> machines, NavMeshAgent agent, int retryAttempts = 0, float retryTimer = 0.5f) {
            _worker = worker;
            _machines = machines;
            _agent = agent;
            _retryAttempts = retryAttempts;
            _timer = new CountdownTimer(retryTimer);            
            Complete = false;
        }

        public void Start() {
            Complete = false;
            //Clear wishlist
            GameController.Instance.MachineController.Machines
                .ForEach(m => m.Slots
                    .Where(s => s.WishListWorker == _worker).ForEach(s => s.SetWishlist()));
            if (TryWishlistMachine() || _retryAttempts <= 0) {
                Debug.Log($"Wishlisted: {_worker.Director?.TargetSlot?.name ?? "Empty"}");
                Complete = true;
                return;
            }
            _timer.OnTimerStop += Retry;
            _timer.Start();
        }
        private void Retry() {
            if (TryWishlistMachine() || --_retryAttempts <= 0) {
                Complete = true;
                return;
            }
            Debug.Log("retry");
            _timer.Start();
        }

        private bool TryWishlistMachine() {
            var slots = new Dictionary<MachineSlot, (int Weight, NavMeshPath Path, MachineBase Machine)>();
            foreach (var machine in _machines(_worker.Director)) {
                foreach (var slot in machine.Slots) {
                    var path = new NavMeshPath();
                    if (!slot.CanAddWorker(_worker)) continue;
                    if (slot.CurrentWorker != null) continue;
                    if (slot.WishListWorker != null) continue;
                    
                    NavMeshHit hit;

                    if (!NavMesh.SamplePosition(machine.transform.position, out hit, Single.MaxValue, NavMesh.AllAreas)) continue;
                    if (!_agent.CalculatePath(hit.position, path)) {
                        Debug.LogWarning($"Cannot calculate path to machine. From {_agent.transform.position} to {hit.position}");
                        continue;
                    }
                    
                    slots.Add(slot, (CalculateWeight(slot, _agent, path), path, machine));
                }
            }

            // _machines
            //     .Where(m => m.Slots
            //         .Any(s => 
            //             s.CanAddWorker(_worker) 
            //             && s.WishListWorker == null
            //             && _agent.CalculatePath(s.transform.position, new NavMeshPath())))
            //     .ToHashSet();
            var weighted = slots.OrderBy(s => s.Value.Weight);
            

            //Evaluate options
            foreach (var w in weighted) {
                //Move down the weighted list until a machine is wish listed
                if (w.Key.SetWishlist(_worker)) {
                    _worker.Director.TargetSlot = w.Key;
                    
                    Complete = true;
                    return true;
                }
            }
            
            return false;
        }

        private int CalculateWeight(MachineSlot slot, NavMeshAgent agent, NavMeshPath path) {
            List<float> weight = new();

            //Weight them based on time (travel time, time for current worker to finish,... )
            //Current worker time
            if (slot.CurrentWorker is not null) {
                var machine = slot.Machine;
                var worker = slot.CurrentWorker;
                weight.Add(worker.Director.EstWorkingTime);
            }
            
            //Travel time
            weight.Add(path.GetLength() / agent.speed);

            return (int) weight.OrderByDescending(w => w).First();
        }

        public void Update(float deltaTime) {
            _timer.Tick(deltaTime);
        }

        public void Stop() {
            _timer.OnTimerStop -= Retry;
        }
    }
}