using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using UnityEngine;

[Serializable]
public struct BoxMaxAmount
{
    public long MaxAmount;

    public bool IsAmountValid(long currentAmount, long newAmount)
    {
        if (newAmount > MaxAmount) return false;
        if (newAmount < 0) return false;

        return true;
    }
}

public struct SaleData
{
    public int UnitPrice;
    public int TotalPrice;
    public DateTime DateTime;
    public BoxTypeName BoxTypeName;
    public int Amount;
}


namespace Script.Controller
{
    [Serializable]
    public class BoxController : ControllerBase
    {
        [SerializeField] private SerializedDictionary<BoxTypeName, BoxMaxAmount> _boxData;

        private Dictionary<BoxTypeName, long> _boxAmount = new();
        
        public ReadOnlyCollection<SaleData> SaleData => _saleData.AsReadOnly(); 
        private List<SaleData> _saleData = new();

        private long _wareHouseMaxAmount;

        public void AddSaleData(int UnitPrice, int TotalPrice, BoxTypeName BoxTypeName, int Amount, DateTime DateTime)
        {
            _saleData.Add(new SaleData { UnitPrice = UnitPrice,
                TotalPrice = TotalPrice,
                DateTime = DateTime,
                BoxTypeName = BoxTypeName,
                Amount = Amount});
        }

        public void AddSaleData(SaleData SaleData)
        {
            _saleData.Add(SaleData);
        }

        public bool TryGetData(BoxTypeName boxType, out BoxMaxAmount boxData, out long currentAmount)
        {
            currentAmount = default;
            var ret = _boxData.TryGetValue(boxType, out boxData) &&
                      TryGetAmount(boxType, out currentAmount);
            return ret;
        }

        public bool TryUpdateData(BoxTypeName boxType, BoxMaxAmount boxData)
        {
            try
            {
                _boxData[boxType] = boxData;
                return true;
            }
            catch
            {
                Debug.LogError($"Failed to update box data: {boxType}");
            }
            return false;
        }

        public bool TryGetAmount(BoxTypeName boxType, out long amount) =>
            _boxAmount.TryGetValue(boxType, out amount);

        public bool TrySetAmount(BoxTypeName boxType, long amount)
        {
            if (!_boxData.TryGetValue(boxType, out var data)) return false;
            if (!TryGetAmount(boxType, out var currentAmount)) return false;
            if (!data.IsAmountValid(currentAmount, amount)) return false;

            long totalBlindBoxAmount = GetTotalBlindBoxAmount() - currentAmount + amount;
            if (totalBlindBoxAmount > _wareHouseMaxAmount)
            {
                Debug.LogWarning($"Cannot set amount for {boxType}: Exceeds warehouse max cap {_wareHouseMaxAmount}");
                return false;
            }

            _boxAmount[boxType] = amount;
            return true;
        }

        public override void Load()
        {
            //throw new System.NotImplementedException();
        }

        public override void Save()
        {
            //throw new System.NotImplementedException();
        }

        public override void OnAwake()
        {
            base.OnAwake();

            Enum.GetValues(typeof(BoxTypeName)).Cast<BoxTypeName>()
                .Where(b => !_boxAmount.ContainsKey(b) && _boxData.ContainsKey(b))
                .ForEach(b => _boxAmount.TryAdd(b, 0));
        }

        public override void OnValidate()
        {
            base.OnValidate();
        }

        private long GetTotalBlindBoxAmount()
        {
            return _boxAmount.Sum(b => b.Value);
        }

        public bool TryGetAllBoxAmounts(out Dictionary<BoxTypeName, long> boxAmounts)
        {
            if (_boxAmount == null || _boxAmount.Count == 0)
            {
                boxAmounts = new Dictionary<BoxTypeName, long>();
                return false;
            }
            boxAmounts = new Dictionary<BoxTypeName, long>(_boxAmount);
            return true;
        }

    }
}
