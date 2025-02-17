using System.Collections.Generic;

namespace Script.Gacha.Shard {
    public interface IShardInventory {
        IReadOnlyDictionary<IShard, int> Inventory { get; }
        void AddShard(IShard shard, int quantity = 1);
        void RemoveShard(IShard shard, int quantity = 1);
    }
}