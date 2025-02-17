#nullable enable
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Script.Gacha.Base;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Script.Gacha.Machine {
    public interface IGachaMachine<TItem> : IRandomMachine<TItem> where TItem : class {
        TItem? Pull(IEnumerable<TItem> itemPool);
        IEnumerable<TItem> ItemPool { get; }
        IItemRequirement<TItem> Requirement { get; }
        void SetRequirement(IItemRequirement<TItem> requirement);
        void ResetRequirement();
    }

    [Serializable]
    public abstract class ScriptableGachaMachineBase<TItem> : ScriptableObject, IGachaMachine<TItem>
        where TItem : class {
        public int Pulls { get; set; }
        protected int _requirementFailPullsCount = 10;
        public virtual TItem? Pull() => Pull(ItemPool);
        public abstract TItem? Pull(IEnumerable<TItem> itemPool);
        public virtual IEnumerable<TItem> PullMultiple(int times) {
            var pulledItems = new List<TItem>();
            while (times-- > 0) {
                var pulled = Pull();
                if (pulled == null) break;
                pulledItems.Add(pulled);
            }

            return pulledItems;
        }

        public IList<TItem> PullHistory { get; set; } = new List<TItem>();
        public virtual IEnumerable<TItem> ItemPool { get => _itemPool; }
        [FormerlySerializedAs("itemPool")] [SerializeField] protected List<TItem> _itemPool = new();
        public IItemRequirement<TItem> Requirement { get => _requirement ?? _serializeRequirements.Compose(); }
        private IItemRequirement<TItem>? _requirement = null;
        [SerializeReference, SubclassSelector] protected List<IItemRequirement<TItem>> _serializeRequirements = new();

        public void SetRequirement(IItemRequirement<TItem> requirement) {
            _requirement ??= _serializeRequirements.Compose();
            _requirement.SetNext(requirement);
        }
        public void ResetRequirement() => _requirement = _serializeRequirements.Compose();
    } 
}