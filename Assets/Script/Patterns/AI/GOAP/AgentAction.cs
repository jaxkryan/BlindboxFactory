using System.Collections.Generic;
using ZLinq;

public class AgentAction {
        public string Name { get; }
        public float Cost { get; private set; }

        private AgentAction(string name) {
            Name = name;
        }

        public HashSet<AgentBelief> Preconditions { get; } = new();
        public HashSet<AgentBelief> Effects { get; } = new();

        IActionStrategy strategy;
        public bool Complete => strategy.Complete;

        public void Start() => strategy.Start();

        public void Update(float deltaTime) {
            if (strategy.CanPerform) {
                strategy.Update(deltaTime);
            }

            if (!strategy.Complete) return;
            Effects.AsValueEnumerable().ForEach(e => e.Evaluate());
        }

        public void Stop() => strategy.Stop();

        public class Builder {
            private readonly AgentAction _action;

            public Builder(string name) {
                _action = new AgentAction(name) { Cost = 1f };
            }

            public Builder WithCost(float cost) {
                _action.Cost = cost;
                return this;
            }

            public Builder WithStrategy(IActionStrategy strategy) {
                _action.strategy = strategy;
                return this;
            }
            
            public Builder AddPrecondition(AgentBelief belief) {
                _action.Preconditions.Add(belief);
                return this;
                }

            public Builder AddEffect(AgentBelief belief) {
                _action.Effects.Add(belief);
                return this;
            }
            
            public AgentAction Build() => _action;
        }
    }
