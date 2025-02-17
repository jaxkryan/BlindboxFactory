using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Script.Gacha.Base;

namespace Script.Gacha.Machine {
    public interface IItemRequirement<TItem> : IChainOfResponsibility<IItemRequirement<TItem>> where TItem : class {
        IEnumerable<TItem> ProcessItemPool(IGachaMachine<TItem> machine, IEnumerable<TItem> items);
        [CanBeNull] TItem ProcessPulledItem(IGachaMachine<TItem> machine, TItem item);
        
    }

    [Serializable]
    public abstract class ItemRequirementBase<TItem> : IItemRequirement<TItem> where TItem : class {
        private IItemRequirement<TItem> _next;
        public static IItemRequirement<TItem> None => new EmptyItemRequirement<TItem>();
        public IEnumerable<TItem> ProcessItemPool(IGachaMachine<TItem> machine, IEnumerable<TItem> items) {
            OnProcessItemPool(machine, ref items);
            return _next != null ? _next.ProcessItemPool(machine, items) : items;
        }

        protected virtual void OnProcessItemPool(IGachaMachine<TItem> machine, ref IEnumerable<TItem> items) { }

        [CanBeNull]
        public TItem ProcessPulledItem(IGachaMachine<TItem> machine, TItem item) {
            OnProcessPulledItem(machine, ref item);
            return _next != null ? _next.ProcessPulledItem(machine, item) : item;
        }

        protected virtual void OnProcessPulledItem(IGachaMachine<TItem> machine, ref TItem item) { }

        public IItemRequirement<TItem> SetNext(IItemRequirement<TItem> next) {
            if (_next == null) _next = next;
            else _next.SetNext(next);
            return this;
        }

        [Obsolete]
        public void Process() { }
    }
}