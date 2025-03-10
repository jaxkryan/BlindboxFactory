using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Gacha.Base;
using Script.Gacha.Shard;
using UnityEngine;

namespace Script.Controller {
    public interface IShardController {
        IShardInventory Inventory { get; }
        Dictionary<ILoot, IEnumerable<(IShard Shard, int Quantity)>> ShardDictionary { get; }
        IShard DefaultShard { get; }
        int DefaultShardQuantity { get; }
        IEnumerable<IShard> ShatterItem(ILoot item, int quantity);
    }
    
    public class ShardController : ControllerBase, IShardController {
        public IShardInventory Inventory { get; }
        public Dictionary<ILoot, IEnumerable<(IShard Shard, int Quantity)>> ShardDictionary { get; }
        public IShard DefaultShard { get; }
        public int DefaultShardQuantity { get; }
        
        public IEnumerable<IShard> ShatterItem(ILoot item, int quantity) {
            #warning Unimplemented
            //throw new System.NotImplementedException();
            if (!ShardDictionary.TryGetValue(item, out var shardQuantity)) return GetShards((DefaultShard, DefaultShardQuantity));
            return GetShards(shardQuantity.ToArray());
            
            List<IShard> GetShards(params (IShard Shard, int Quantity)[] shards) {
                var ret = new List<IShard>();

                foreach (var shardQuantity in shards) {
                    for (int i = 0; i < shardQuantity.Quantity; i++) {
                        var shard = Activator.CreateInstance(shardQuantity.Shard.GetType());
                        ret.Add((IShard)shard);
                    }
                }
                
                return ret;
            }
        }

        public override void Load() { 
            //throw new NotImplementedException(); 
        }
        public override void Save() { 
            //throw new NotImplementedException(); 
        }
    }
}