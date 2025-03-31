using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using Newtonsoft.Json;
using Script.Controller.SaveLoad;
using Script.Resources;
using UnityEngine;

namespace Script.Controller {
    [Serializable]
    public class ResourceController : ControllerBase {
        [SerializeField] private SerializedDictionary<ResourceConversionPair, float> _resourceConversion;

        [SerializeField] private SerializedDictionary<Resource, ResourceData> _resourceData;

        [SerializeField]private SerializedDictionary<Resource, long> _resourceAmount = new() ;
        public bool TryGetData(Resource resource, out ResourceData resourceData, out long currentAmount) {
            currentAmount = default;
            var ret = _resourceData.TryGetValue(resource, out resourceData) &&
                      TryGetAmount(resource, out currentAmount);
            return ret;
        }

        public bool TryUpdateData(Resource resource, ResourceData resourceData) {
            try {
                _resourceData[resource] = resourceData;
                onResourceDataChanged?.Invoke(resource, resourceData);
                return true;
            }
            catch {
                Debug.LogError($"Failed to update resource data: {resource}");
            }

            return false;
        }

        public bool TryGetAmount(Resource resource, out long amount) =>
            _resourceAmount.TryGetValue(resource, out amount);

        public bool TrySetAmount(Resource resource, long amount) {
            if (!TryGetAmount(resource, out var currentAmount)) return false;
            if (_resourceData.TryGetValue(resource, out var data) 
                && !data.IsAmountValid(currentAmount, amount)) return false;

            _resourceAmount[resource] = amount;
            onResourceAmountChanged?.Invoke(resource, currentAmount, amount);
            return true;
        }

        public event Action<Resource, long, long> onResourceAmountChanged = delegate { };
        public event Action<Resource, ResourceData> onResourceDataChanged = delegate { };
        
        public bool TryGetConversionRate(ResourceConversionPair resourceConversionPair, out List<(Resource
            ConversionNode, float Rate)> conversionPath, bool tryFindExchange = true)
            => _resourceConversion.TryGetConversionRates(resourceConversionPair, out conversionPath, tryFindExchange);

        public bool TryGetConversionRate(Resource from, Resource to, out List<(Resource
            ConversionNode, float Rate)> conversionPath, bool tryFindExchange = true)
            => _resourceConversion.TryGetConversionRates(from, to, out conversionPath, tryFindExchange);

        public bool TryConversion(ResourceConversionPair resourceConversionPair, long amount, out long result,
            bool tryFindingExchangeRate = true, bool roundDownEachConversion = true)
            => _resourceConversion.TryConversion(resourceConversionPair, amount, out result, tryFindingExchangeRate,
                roundDownEachConversion);

        public bool TryConversion(Resource from,
            Resource to, long amount, out long result, bool tryFindingExchangeRate = true,
            bool roundDownEachConversion = true)
            => _resourceConversion.TryConversion(from, to, amount, out result, tryFindingExchangeRate,
                roundDownEachConversion);

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

        public bool TryGetAllResourceAmounts(out Dictionary<Resource, long> resourceAmounts)
        {
            if (_resourceAmount == null || _resourceAmount.Count == 0)
            {
                resourceAmounts = new Dictionary<Resource, long>();
                return false;
            }
            resourceAmounts = new Dictionary<Resource, long>(_resourceAmount);
            return true;
        }

        public override void Load(SaveManager saveManager) {
            // Debug.Log("run Load");
            // foreach (Resource resource in Enum.GetValues(typeof(Resource)))
            // {
            //     _resourceAmount[resource] = 10000;
            // }
            
            try {
                if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                      || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data) return;

                _resourceConversion = new(data.ResourceConversion);
                _resourceData = new(data.ResourceData);
                _resourceAmount = new(data.ResourceAmount);
                _resourceAmount.ForEach(r => onResourceAmountChanged?.Invoke(r.Key, 0, r.Value));
                _resourceData.ForEach(r => onResourceDataChanged?.Invoke(r.Key, r.Value));
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot load {GetType()}");
                Debug.LogException(ex);
                return;
            }
        }

        public override void Save(SaveManager saveManager) {
            var newSave = new SaveData() {
                ResourceConversion =  _resourceConversion,
                ResourceData = _resourceData,
                ResourceAmount = _resourceAmount
            };
            
            
            try {
                if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                    || SaveManager.Deserialize<SaveData>(saveData) is SaveData data)
                    saveManager.SaveData.TryAdd(this.GetType().Name,
                        SaveManager.Serialize(newSave));
                else
                    saveManager.SaveData[this.GetType().Name]
                        = SaveManager.Serialize(newSave);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot save {GetType()}");
                Debug.LogException(ex);
            }
        }
        

        public static string FormatNumber(long number) {
            Dictionary<long, string> numberIndex = new();
            numberIndex.Add(1, "");
            numberIndex.Add(1000, "k");
            numberIndex.Add(1000_000, "M");
            numberIndex.Add(1000_000_000, "B");
            numberIndex.Add(1000_000_000_000, "T");
                Debug.LogWarning($"For number: {number}");
            
            
            
            long cutoff = Int64.MaxValue; //Cutoff switch to using ABC system (Unimplemented)
            if (number >= cutoff) {
                //Implement the ABC system
                Debug.LogWarning("Incorrect path 1");
                return number.ToString();
            }
            else if (number >= numberIndex.Keys.Min()){
                long index = 1;
                Debug.LogWarning("Correct path");
                string abbreviation = "";
                while (number / index >= 1000) {
                    index *= 1000;
                    Debug.LogWarning($"Index: {index}. {number / index > 1}");
                    
                }
                var i = index;
                while (i > 1) {
                    var max = numberIndex.Keys.Max();
                    if (i > max) {
                        i /= max;
                        abbreviation = numberIndex[max] += abbreviation;
                    }
                    else {
                        var key = numberIndex.First().Key;
                        var x = key;
                        do {
                            x *= 10;
                            i /= 10;
                            if (numberIndex.ContainsKey(x)) key = x;
                        } while (i > 1);
                        abbreviation = numberIndex[key] + abbreviation;
                    }
                }

                Debug.LogWarning("Index: " + i);
                Debug.LogWarning("Abbreviation: " + abbreviation);
                
                return (number / float.Parse(index.ToString())).ToString("###.##") + abbreviation;
            }
            else{
                Debug.LogWarning("Incorrect path 2");
                return number.ToString();
            }
            
            // if (number >= 1_000_000_000_000)
            //     return (number / 1_000_000_000_000f).ToString("0.##") + "T";
            // else if (number >= 1_000_000_000)
            //     return (number / 1_000_000_000f).ToString("0.##") + "B";
            // else if (number >= 1_000_000)
            //     return (number / 1_000_000f).ToString("0.##") + "M";
            // else if (number >= 1_000)
            //     return (number / 1_000f).ToString("0.##") + "k";
            // else
            //     return number.ToString(); // normal number
        }
        
        public class SaveData{
            public Dictionary<ResourceConversionPair, float> ResourceConversion;
            public Dictionary<Resource, ResourceData> ResourceData;
            public Dictionary<Resource, long> ResourceAmount;
            
        }
    }
}