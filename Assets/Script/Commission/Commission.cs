using System.Collections.Generic;
using System.Collections.ObjectModel;
using AYellowpaper.SerializedCollections;
using Script.Quest;
using UnityEngine;

namespace Script.Controller.Commission {
    public class Commission {
        public ReadOnlyDictionary<BoxTypeName, int> Items { get; private set; }
        
        public QuestReward Reward { get; private set; }

        public class Builder {
            private Commission _commission;
            private Dictionary<BoxTypeName, int> _items;
            public Builder() {
                _commission = new Commission();
                _items = new();
            }

            public Builder WithReward(QuestReward reward) {
                _commission.Reward = reward;
                return this;
            }

            public Builder WithItem(BoxTypeName item, int quantity) {
                _items.Add(item, quantity);
                return this;
            }

            public Builder WithItems(params (BoxTypeName BoxType, int Quantity)[] items) {
                items.ForEach(i => WithItem(i.BoxType, i.Quantity));
                return this;
            }

            public Commission Build() {
                _commission.Items = new ReadOnlyDictionary<BoxTypeName, int>(_items);
                return _commission;
            }
        }
    }
}