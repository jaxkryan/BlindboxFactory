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
    protected Rigidbody2D _rb;

    protected GameObject _target;
    protected Vector3 _destination;
    protected AgentGoal _lastGoal;
    public AgentGoal CurrentGoal;
    public ActionPlan ActionPlan;
    public AgentAction CurrentAction {
        get => _currentAction;
        set {
            _current = value?.Name;
            _currentAction = value;
        }
    }

    [SerializeField] private string _current;
    public AgentAction _currentAction;
    
    public Dictionary<string, AgentBelief> Beliefs;
    public HashSet<AgentAction> Actions;
    public HashSet<AgentGoal> Goals;

    [SerializeField] protected bool _log;
    
    IGoapPlanner _planner;
    
    protected List<Timer> _timers;

    protected virtual void Awake() {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animation = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
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
    void Update() {
        _timers?.ForEach(t =>t.Tick(Time.deltaTime));
        _animation.speed = _navMeshAgent.velocity.magnitude;
        
        // Update the plan and current action if there is one
        if (CurrentAction == null) {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (ActionPlan != null && ActionPlan.Actions.Count > 0) {
                if (!_navMeshAgent.isOnNavMesh) {
                    _navMeshAgent.enabled = true;
                    if (NavMesh.SamplePosition(_navMeshAgent.transform.position, out var hit, Single.MaxValue, 1)) {
                        Debug.LogWarning($"Warping agent to {hit.position}");
                        _navMeshAgent.Warp(hit.position);
                    }
                }
                _navMeshAgent.ResetPath();

                CurrentGoal = ActionPlan.AgentGoal;
                if (_log) Debug.Log($"Goal: {CurrentGoal.Name} with {ActionPlan.Actions.Count} actions in plan ({string.Join(", ", ActionPlan.Actions.Select(a => a.Name))}).");
                CurrentAction = ActionPlan.Actions.Pop();
                if (_log) Debug.Log($"Popped action: {CurrentAction.Name}");
                // Verify all precondition effects are true
                if (CurrentAction.Preconditions.All(b => b.Evaluate())) {
                    CurrentAction.Start();
                } else {
                    var condition = CurrentAction.Preconditions.Where(c => !c.Evaluate()).Select(a => a.Name);
                    if (_log) Debug.Log($"Preconditions not met, clearing current action and goal ({string.Join(", ", condition)})");
                    CurrentAction = null;
                    CurrentGoal = null;
                }
            }
        }

        // If we have a current action, execute it
        if (ActionPlan != null && CurrentAction != null) {
            CurrentAction.Update(Time.deltaTime);

            if (CurrentAction.Complete) {
                if (_log) Debug.Log($"{CurrentAction.Name} complete");
                CurrentAction.Stop();
                CurrentAction = null;

                if (ActionPlan.Actions.Count == 0) {
                    if (_log) Debug.Log("Plan complete");
                    _lastGoal = CurrentGoal;
                    CurrentGoal = null;
                }
            }
        }
    }
    private void CalculatePlan() {
        var priorityLevel = CurrentGoal?.Priority ?? 0;
        
        HashSet<AgentGoal> goalsToCheck = Goals;
        
        // If we have a current goal, we only want to check goals with higher priority
        if (CurrentGoal != null) {
            if (_log) Debug.Log("Current goal exists, checking goals with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(Goals.Where(g => g.Priority > priorityLevel));
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