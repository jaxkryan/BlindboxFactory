using System.Linq;
using Script.Controller;
using Script.Machine;
using UnityEngine;

namespace Script.HumanResource.Worker {
    [RequireComponent(typeof(Worker))]
    public class WorkerDirector : GoapAgent {
        // [Header("Sensor")]
        // [SerializeField] private Sensor _workMachineSensor;
        
        
        [Header("Locations")]
        [SerializeField] public Transform MachineLocation;
        
        Worker _worker;

        protected override void Awake() {
            base.Awake();
            _worker = GetComponent<Worker>();
        }

        protected override void SetupTimers() {
            base.SetupTimers();
        }

        protected override void SetupBeliefs() {
            base.SetupBeliefs();
            BeliefFactory bf = new(this, Beliefs);
            
            bf.AddBelief("Nothing", () => false);
            
            bf.AddBelief("WorkerIdle", () => !_navMeshAgent.hasPath);
            bf.AddBelief("WorkerWalking", () => !_navMeshAgent.hasPath);
            // bf.AddBelief("WorkerHungerLow", () => _worker.CurrentHunger <= 50f);
            // bf.AddBelief("WorkerHappinessLow", () => _worker.CurrentHappiness <= 50f);
            // bf.AddBelief("WorkerHungerNormal", () => _worker.CurrentHunger > 50f);
            // bf.AddBelief("WorkerHappinessNormal", () => _worker.CurrentHappiness > 50f);
            bf.AddBelief("WorkerHasWorkableMachine", () => GameController.Instance.MachineController.FindWorkableMachines(_worker).Any());
            bf.AddBelief("WorkerHasNoWorkableMachine", () => !GameController.Instance.MachineController.FindWorkableMachines(_worker).Any());
            
            // bf.AddLocationBelief("MachineInWorkingRange", );
            bf.AddBelief("WorkerWorking", () => false);
        }

        protected override void SetupGoals() {
            base.SetupGoals();
            
            
        }

        protected override void SetupActions() {
            base.SetupActions();
        }
    }
}