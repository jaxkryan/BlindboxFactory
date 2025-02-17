using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IGoapPlanner {
    ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
}

public class GoapPlanner : IGoapPlanner {
    public bool CanRepeatAction {get; set;} = true;
    public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null) {
        var orderedGoals = goals
            .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
            .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01f : g.Priority)
            .ToList();

        foreach (var goal in orderedGoals) {
            var goalNode = new GoapNode(null, null, goal.DesiredEffects, 0);

            if (FindPath(goalNode, agent.Actions)) {
                if (goalNode.IsLeafDead) continue;
                
                Stack<AgentAction> actionStack = new Stack<AgentAction>();

                while (goalNode.Children.Any()) {
                    var cheapest = goalNode.Children.OrderBy(c => c.Cost).First();
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
        foreach (var action in agentActions) {
            var requiredEffects = parent.RequiredEffects;

            requiredEffects.RemoveWhere(b => b.Evaluate());

            if (!requiredEffects.Any()) return true;

            if (action.Effects.Any(requiredEffects.Contains)) {
                var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
                newRequiredEffects.ExceptWith(action.Effects);
                newRequiredEffects.UnionWith(action.Preconditions);

                var newAvailableAction = new HashSet<AgentAction>(agentActions);
                if(!CanRepeatAction) newAvailableAction.Remove(action);
                
                var newNode = new GoapNode(parent, action, newRequiredEffects, parent.Cost + action.Cost);

                if (FindPath(newNode, newAvailableAction)) {
                    parent.Children.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                }

                if (!newRequiredEffects.Any()) return true;
            }
        }
        
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

    public bool IsLeafDead => !Children.Any() && Action == null;
}