using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Quest;
using UnityEngine;

namespace Script.Controller.Commission {
    public class Commission {
        public ReadOnlyDictionary<BoxTypeName, int> Items;
        public QuestReward Reward { get; private set; }
        private long _basePrice => CalculateBasePrice();
        public long BonusPrice => _bonusPrice;
        private long _bonusPrice = 0;
        public long Price => _basePrice + _bonusPrice;

        public DateTime ExpireDate;

        private long CalculateBasePrice() {
            long basePrice = 0;
            foreach (var pair in Items) {
                try {
                    var boxData = BoxTypeManager.Instance.GetBoxData(pair.Key);
                    basePrice += boxData.value * pair.Value;
                }
                catch  {
                    Debug.LogError($"Cannot get box data of {pair.Key}");;
                    continue;
                }
            }

            return basePrice;
        }


        public bool IsFulfilled(out float progress) {
            if (Items is null || Items.Count == 0) {
                progress = 100;
                return true;
            }

            var total = Items.Values?.Sum() ?? 0;
            progress = 0;
            foreach (var pair in Items) {
                if (GameController.Instance.BoxController.TryGetAmount(pair.Key, out var amount)) {
                    progress += amount > pair.Value ? pair.Value : amount;
                }
            }
            progress = progress / total * 100;
            return progress  >= 100;
        }

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

            public Builder WithBonusPrice(long bonusPrice) {
                _commission._bonusPrice = bonusPrice;
                return this;
            }

            public Builder WithExpiredDate(DateTime expiredDate) {
                _commission.ExpireDate = expiredDate;
                return this;
            }

            public Commission Build() {
                _commission.Items = new ReadOnlyDictionary<BoxTypeName, int>(_items);
                return _commission;
            }
        }
    }
}