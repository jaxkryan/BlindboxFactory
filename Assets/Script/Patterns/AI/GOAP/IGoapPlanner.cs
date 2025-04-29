using System;
using System.Collections.Generic;
using ZLinq;
using UnityEngine;
using UnityEngine.Pool;

public interface IGoapPlanner {
    ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
}

public class GoapPlanner : IGoapPlanner {
    public bool CanRepeatAction {get; set;} = true;
    public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null) {
        // Debug.Log($"Checking goals: {string.Join(", ", goals.Select(a => a.Name))}");
        var orderedGoals = goals
            .AsValueEnumerable()
            .Where(g => g.DesiredEffects.AsValueEnumerable().Any(b => !b.Evaluate()))
            .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01f : g.Priority)
            .ToList();
        // Debug.Log($"Ordered goals: {string.Join(", ", orderedGoals.Select(a => a.Name))}");

        foreach (var goal in orderedGoals) {
            var goalNode = new GoapNode(null, null, goal.DesiredEffects, 0);

            // Debug.LogWarning($"Checking goal: {goal.Name}");
            if (FindPath(goalNode, agent.Actions)) {
                if (goalNode.IsLeafDead) continue;
                
                Stack<AgentAction> actionStack = new Stack<AgentAction>();

                while (goalNode.Children.AsValueEnumerable().Any()) {
                    var cheapest = goalNode.Children.AsValueEnumerable().OrderBy(c => c.Cost).First();
                    goalNode = cheapest;
                    actionStack.Push(cheapest.Action);
                }
                
                return new ActionPlan(goal, actionStack, goalNode.Cost);
            }
        }
        
        Debug.LogWarning("No plan found");
        return null;
    }
    

    private bool FindPath(GoapNode parent, HashSet<AgentAction> agentActions) {
        // if (parent.Action is not null) Debug.LogWarning($"Action: {parent.Action.Name}");
        foreach (var action in agentActions) {
            var requiredEffects = HashSetPool<AgentBelief>.Get();
            requiredEffects.UnionWith(parent.RequiredEffects);

            requiredEffects.RemoveWhere(b => b.Evaluate());

            if (!requiredEffects.AsValueEnumerable().Any()) return true;

            var match = false;
            foreach (var effect in action.Effects) {
                if (requiredEffects.Contains(effect)) {
                    match = true;
                    break;
                }
            }
            
            if (match) {
                requiredEffects.ExceptWith(action.Effects);
                requiredEffects.UnionWith(action.Preconditions);

                var newAvailableAction = new HashSet<AgentAction>(agentActions);
                if(!CanRepeatAction) newAvailableAction.Remove(action);
                
                var newNode = new GoapNode(parent, action, requiredEffects, parent.Cost + action.Cost);

                if (FindPath(newNode, newAvailableAction)) {
                    parent.Children.Add(newNode);
                    requiredEffects.ExceptWith(newNode.Action.Preconditions);
                }

                if (requiredEffects.Count == 0) {
                    HashSetPool<AgentBelief>.Release(requiredEffects);
                    return true;
                }
            }
            HashSetPool<AgentBelief>.Release(requiredEffects);
        }

        // Debug.LogWarning("Path died");
        return false;
    }
}

public class GoapNode : Node<GoapNode> {

    public GoapNode(GoapNode parent, AgentAction action, HashSet<AgentBelief> requiredEffects, float cost) {
        Parent = parent;
        Action = action;
        RequiredEffects = requiredEffects;
        Cost = cost;
        Children = new List<GoapNode>();
    }
    public override GoapNode Parent { get; }
    public override IList<GoapNode> Children { get; }
    public AgentAction Action { get; }
    public HashSet<AgentBelief> RequiredEffects { get; }
    public float Cost { get; }

    public bool IsLeafDead => !Children.AsValueEnumerable().Any() && Action == null;
}