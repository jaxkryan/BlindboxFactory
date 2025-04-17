using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller.SaveLoad;
using UnityEngine;

[Serializable]
public struct BoxMaxAmount {
    public long MaxAmount;

    public bool IsAmountValid(long currentAmount, long newAmount) {
        if (newAmount > MaxAmount) return false;
        if (newAmount < 0) return false;

        return true;
    }
}

public struct SaleData {
    public int UnitPrice;
    public int TotalPrice;
    public DateTime DateTime;
    public BoxTypeName BoxTypeName;
    public int Amount;
}

public struct BoxLog {
    public DateTime DateTime;
    public int Amount;
}


namespace Script.Controller {
    [Serializable]
    public class BoxController : ControllerBase {
        [SerializeField] private SerializedDictionary<BoxTypeName, BoxMaxAmount> _boxData;
        [SerializeField] private int _expireTime;

        [SerializeField] private SerializedDictionary<BoxTypeName, long> _boxAmount = new();

        private Dictionary<BoxTypeName, List<BoxLog>> _boxLog = new();

        public ReadOnlyCollection<SaleData> SaleData => _saleData.AsReadOnly();
        private List<SaleData> _saleData = new();

        public event Action<BoxTypeName, long, long> onBoxAmountChanged = delegate { };
        public event Action<BoxTypeName, BoxMaxAmount> onBoxDataChanged = delegate { };

        [SerializeField] private long _wareHouseMaxAmount;

        public void AddSaleData(int UnitPrice, int TotalPrice, BoxTypeName BoxTypeName, int Amount, DateTime DateTime) {
            _saleData.Add(new SaleData {
                UnitPrice = UnitPrice,
                TotalPrice = TotalPrice,
                DateTime = DateTime,
                BoxTypeName = BoxTypeName,
                Amount = Amount
            });
        }

        public void AddSaleData(SaleData SaleData) { _saleData.Add(SaleData); }

        public bool TryGetData(BoxTypeName boxType, out BoxMaxAmount boxData, out long currentAmount) {
            currentAmount = default;
            var ret = _boxData.TryGetValue(boxType, out boxData) && TryGetAmount(boxType, out currentAmount);
            return ret;
        }

        public bool TryUpdateData(BoxTypeName boxType, BoxMaxAmount boxData) {
            try {
                _boxData[boxType] = boxData;
                return true;
            }
            catch { Debug.LogError($"Failed to update box data: {boxType}"); }

            return false;
        }

        public bool TryGetAmount(BoxTypeName boxType, out long amount) => _boxAmount.TryGetValue(boxType, out amount);

        public bool TrySetAmount(BoxTypeName boxType, long amount) {
            RemoveExpiredLogs();
            if (!TryGetAmount(boxType, out var currentAmount)) return false;
            if (_boxData.TryGetValue(boxType, out var data) && !data.IsAmountValid(currentAmount, amount)) return false;

            long totalBlindBoxAmount = GetTotalBlindBoxAmount() - currentAmount + amount;
            if (totalBlindBoxAmount > _wareHouseMaxAmount) {
                Debug.LogWarning($"Cannot set amount for {boxType}: Exceeds warehouse max cap {_wareHouseMaxAmount}");
                return false;
            }

            _boxAmount[boxType] = amount;

            if (amount != 0) {
                var newBoxLog = new BoxLog() {
                    DateTime = DateTime.Now,
                    Amount = (int)(amount - currentAmount),
                };
                if (!_boxLog.TryGetValue(boxType, out var logs)) {
                    _boxLog.Add(boxType, new List<BoxLog>() { newBoxLog });
                }
                else { logs.Add(newBoxLog); }
            }

            return true;

            void RemoveExpiredLogs() {
                var keys = _boxLog.Keys;
                keys.ForEach(k => {
                    if (_boxLog.TryGetValue(k, out var logs)) {
                        var expLogs = logs.Where(log => log.DateTime.AddHours(_expireTime) <= DateTime.Now).ToList()
                            .Clone();
                        expLogs.ForEach(log => logs.Remove(log));
                    }
                });
            }
        }

        public override void Load(SaveManager saveManager) {
            try {
                if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                    || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data) {
                    // Initialize defaults if no save data
                    foreach (BoxTypeName btn in Enum.GetValues(typeof(BoxTypeName))) { _boxAmount[btn] = 100; }

                    return;
                }

                _boxData = new(data.BoxData);
                _boxAmount = new(data.BoxAmount);
                _wareHouseMaxAmount = data.WarehouseMaxAmount;
                _saleData = data.SaleData != null ? new List<SaleData>(data.SaleData) : new List<SaleData>();
                _boxLog = data.BoxLog != null
                    ? new Dictionary<BoxTypeName, List<BoxLog>>(data.BoxLog)
                    : new Dictionary<BoxTypeName, List<BoxLog>>();

                    try {
                        _boxAmount.ForEach(b => onBoxAmountChanged?.Invoke(b.Key, 0, b.Value));
                        _boxData.ForEach(b => onBoxDataChanged?.Invoke(b.Key, b.Value));
                    }
                    catch (System.Exception e) { Debug.LogException(e); }
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot load {GetType()}");
                Debug.LogException(ex);
            }
        }

        public override void Save(SaveManager saveManager) {
            var newSave = new SaveData {
                BoxData = _boxData,
                BoxAmount = _boxAmount,
                WarehouseMaxAmount = _wareHouseMaxAmount,
                SaleData = _saleData,
                BoxLog = _boxLog
            };

            try {
                var serialized = SaveManager.Serialize(newSave);
                saveManager.SaveData.AddOrUpdate(this.GetType().Name, serialized, (key, oldValue) => serialized);
                // if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                //     || SaveManager.Deserialize<SaveData>(saveData) is SaveData data)
                //     saveManager.SaveData.TryAdd(this.GetType().Name,
                //         SaveManager.Serialize(newSave));
                // else
                //     saveManager.SaveData[this.GetType().Name]
                //         = SaveManager.Serialize(newSave);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot save {GetType()}");
                Debug.LogException(ex);
            }
        }

        public override void OnAwake() {
            base.OnAwake();

            Enum.GetValues(typeof(BoxTypeName)).Cast<BoxTypeName>()
                .Where(b => !_boxAmount.ContainsKey(b) && _boxData.ContainsKey(b))
                .ForEach(b => _boxAmount.TryAdd(b, 0));
        }

        public override void OnValidate() { base.OnValidate(); }

        public long GetTotalBlindBoxAmount() {
            long boxamount = 0;
            foreach (var box in _boxAmount) {
                if (box.Key != BoxTypeName.Null) { boxamount += box.Value; }
            }

            return boxamount;
        }

        public bool TryGetAllBoxAmounts(out Dictionary<BoxTypeName, long> boxAmounts) {
            if (_boxAmount == null || _boxAmount.Count == 0) {
                boxAmounts = new Dictionary<BoxTypeName, long>();
                return false;
            }

            boxAmounts = new Dictionary<BoxTypeName, long>(_boxAmount);
            return true;
        }

        public bool TrySetWarehouseMaxAmount(long newMaxAmount) {
            if (newMaxAmount < 0) {
                Debug.LogWarning("Warehouse max amount cannot be negative!");
                return false;
            }

            _wareHouseMaxAmount = newMaxAmount;
            Debug.Log($"Warehouse max amount updated to: {_wareHouseMaxAmount}");
            return true;
        }

        public bool TryGetWarehouseMaxAmount(out long maxAmount) {
            maxAmount = _wareHouseMaxAmount;
            return true;
        }

        public class SaveData {
            public Dictionary<BoxTypeName, BoxMaxAmount> BoxData;
            public Dictionary<BoxTypeName, long> BoxAmount;
            public long WarehouseMaxAmount;
            public List<SaleData> SaleData;
            public Dictionary<BoxTypeName, List<BoxLog>> BoxLog;
        }
    }
}