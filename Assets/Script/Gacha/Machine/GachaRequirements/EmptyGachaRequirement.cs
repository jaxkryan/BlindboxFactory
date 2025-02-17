using System;
using Script.Gacha.Base;

namespace Script.Gacha.Machine {
    [Serializable]
    public class EmptyItemRequirement<TItem> : ItemRequirementBase<TItem> where TItem : class {
    }
}