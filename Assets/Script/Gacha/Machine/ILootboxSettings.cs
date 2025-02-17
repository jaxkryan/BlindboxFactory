using System.Collections.Generic;
using Script.Gacha.Base;

namespace Script.Gacha.Machine {
    public interface ILootboxSettings<TLoot> where TLoot : ILoot {
        float Rate { get; }
        IEnumerable<TLoot> ItemPool { get; }
    }
}