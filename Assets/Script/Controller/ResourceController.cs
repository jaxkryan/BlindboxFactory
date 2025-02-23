using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.Resources;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class ResourceController : ControllerBase {
        [SerializeField] private SerializedDictionary<ResourceConversionPair, float> _resourceConversion;

        [SerializeField] private SerializedDictionary<Resource, ResourceData> _resourceData;

        private Dictionary<Resource, int> _resourceAmount = new();

        public bool TryGetData(Resource resource, out ResourceData resourceData, out int currentAmount) {
            currentAmount = default;
            var ret = _resourceData.TryGetValue(resource, out resourceData) &&
                      TryGetAmount(resource, out currentAmount);
            return ret;
        }

        public bool TryUpdateData(Resource resource, ResourceData resourceData) {
            try {
                _resourceData[resource] = resourceData;
                return true;
            }
            catch {
                Debug.LogError($"Failed to update resource data: {resource}");
            }

            return false;
        }

        public bool TryGetAmount(Resource resource, out int amount) =>
            _resourceAmount.TryGetValue(resource, out amount);

        public bool TrySetAmount(Resource resource, int amount) {
            if (!_resourceData.TryGetValue(resource, out var data)) return false;
            if (!TryGetAmount(resource, out var currentAmount)) return false;
            if (!data.IsAmountValid(currentAmount, amount)) return false;

            _resourceAmount[resource] = amount;

            return true;
        }

        public bool TryGetConversionRate(ResourceConversionPair resourceConversionPair, out List<(Resource
            ConversionNode, float Rate)> conversionPath, bool tryFindExchange = true)
            => _resourceConversion.TryGetConversionRates(resourceConversionPair, out conversionPath, tryFindExchange);

        public bool TryGetConversionRate(Resource from, Resource to, out List<(Resource
            ConversionNode, float Rate)> conversionPath, bool tryFindExchange = true)
            => _resourceConversion.TryGetConversionRates(from, to, out conversionPath, tryFindExchange);

        public bool TryConversion(ResourceConversionPair resourceConversionPair, int amount, out int result,
            bool tryFindingExchangeRate = true, bool roundDownEachConversion = true)
            => _resourceConversion.TryConversion(resourceConversionPair, amount, out result, tryFindingExchangeRate,
                roundDownEachConversion);

        public bool TryConversion(Resource from,
            Resource to, int amount, out int result, bool tryFindingExchangeRate = true,
            bool roundDownEachConversion = true)
            => _resourceConversion.TryConversion(from, to, amount, out result, tryFindingExchangeRate,
                roundDownEachConversion);

        public override void Load() {
            throw new System.NotImplementedException();
        }

        public override void Save() {
            throw new System.NotImplementedException();
        }

        public override void OnAwake() {
            base.OnAwake();

            Enum.GetValues(typeof(Resource)).Cast<Resource>()
                .Where(r => !_resourceAmount.ContainsKey(r) && _resourceData.ContainsKey(r))
                .ForEach(r => _resourceAmount.TryAdd(r, 0));

            _resourceConversion.Where(r => r.Key.From == r.Key.To)
                .Select(r => r.Key).ToList()
                .ForEach(_resourceConversion.Remove);
        }

        public override void OnValidate() {
            base.OnValidate();


            foreach (var conversion in _resourceConversion) {
                if (conversion.Key.From != conversion.Key.To) continue;
                var ex = new ArgumentException($"Cannot convert from {conversion.Key.From} to {conversion.Key.To}");
                Debug.LogException(ex);
            }
        }
    }
}