using System.Collections.Generic;

public class ActionPlan {
        public ActionPlan(AgentGoal agentGoal, Stack<AgentAction> actions, float totalCost) {
            AgentGoal = agentGoal;
            Actions = actions;
            TotalCost = totalCost;
        }
        public AgentGoal AgentGoal {get;}
        public Stack<AgentAction> Actions {get;}
        public float TotalCost {get; set; }
    }
