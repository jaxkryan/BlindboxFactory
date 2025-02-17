using System.Collections.Generic;

public class AgentGoal {
    public string Name { get; }
    public float Priority { get; private set; }

    public HashSet<AgentBelief> DesiredEffects { get; } = new();

    private AgentGoal(string name) {
        Name = name;
    }

    public class Builder {
        readonly AgentGoal _goal;

        public Builder(string name) {
            _goal = new AgentGoal(name);
        }

        public Builder WithPriority(float priority) {
            _goal.Priority = priority;
            return this;
        }

        public Builder WithDesiredEffects(AgentBelief desiredEffect) {
            _goal.DesiredEffects.Add(desiredEffect);
            return this;
        }

        public AgentGoal Build() => _goal;
    }
}