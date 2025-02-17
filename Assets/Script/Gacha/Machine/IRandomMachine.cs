#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace Script.Gacha.Machine {
    public interface IRandomMachine<TItem> where TItem : class {
        int Pulls { get; }
        TItem? Pull();
        IEnumerable<TItem> PullMultiple(int times);
        IList<TItem> PullHistory { get; }
    }
}