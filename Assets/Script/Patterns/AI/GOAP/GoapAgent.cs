using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public abstract class GoapAgent : MonoBehaviour {
    protected NavMeshAgent _navMeshAgent;
    protected Animator _animation;
    protected Rigidbody _rb;

    protected GameObject _target;
    protected Vector3 _destination;
    protected AgentGoal _lastGoal;
    public AgentGoal CurrentGoal;
    public ActionPlan ActionPlan;
    public AgentAction CurrentAction;
    
    public Dictionary<string, AgentBelief> Beliefs;
    public HashSet<AgentAction> Actions;
    public HashSet<AgentGoal> Goals;
    
    IGoapPlanner _planner;
    
    protected List<Timer> _timers;

    protected virtual void Awake() {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animation = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        
        _planner = new GoapPlanner();
        _timers = new ();
        Beliefs = new ();
        Goals = new ();
        Actions = new ();
    }

    protected virtual void Start() {
        SetupTimers();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    protected virtual void Update() {
        _animation.speed = _navMeshAgent.velocity.magnitude;
        _timers.ForEach(t => t.Tick(Time.deltaTime));

        if (CurrentAction == null) {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();
        }

        if (ActionPlan != null && ActionPlan.Actions.Any()) {
            _navMeshAgent.ResetPath();

            CurrentGoal = ActionPlan.AgentGoal;
            Debug.Log($"Goal: {CurrentGoal.Name} with {ActionPlan.Actions.Count} actions planned.");
            CurrentAction = ActionPlan.Actions.Pop();
            Debug.Log($"Current action is {CurrentAction.Name}");
            CurrentAction.Start();
        }

        if (ActionPlan != null && CurrentAction != null) {
            CurrentAction.Update(Time.deltaTime);

            if (CurrentAction.Complete) {
                Debug.Log($"Action: {CurrentAction.Name} is complete.");
                CurrentAction.Stop(); 
                CurrentAction = null;


                if (ActionPlan.Actions.Any()) {
                    Debug.Log("Plan complete.");
                    _lastGoal = CurrentGoal;
                    CurrentGoal = null;
                }
            }
        }
    }

    private void CalculatePlan() {
        var priority = CurrentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = Goals;

        if (CurrentGoal != null) {
            Debug.Log("Current goal exists, checking goals with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(Goals.Where(g => g.Priority > priority));
        }

        var potentialPlan = _planner.Plan(this, goalsToCheck, _lastGoal);
        if (potentialPlan != null) {
            ActionPlan = potentialPlan;
        }
    }

    protected virtual void SetupTimers() { }
    protected virtual void SetupBeliefs() { Beliefs = new(); }
    protected virtual void SetupActions() { Actions = new(); }
    protected virtual void SetupGoals() { Goals = new(); }
}