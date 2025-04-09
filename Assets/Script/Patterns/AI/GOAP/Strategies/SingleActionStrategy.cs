using System;

namespace Script.Patterns.AI.GOAP.Strategies {
    public class SingleActionStrategy : IActionStrategy {
        public bool CanPerform { get; private set; } = true;
        public bool Complete { get; private set; } = false;

        private Action _action;
        
        public SingleActionStrategy(Action action) {
            _action = action;
        }

        public void Start() {
            Complete = false;
            _action?.Invoke();
            Complete = true;
        }
    }
}