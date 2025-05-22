using System.Collections.Generic;
using System.Linq;
using ZLinq;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator.Policies {
    [CreateAssetMenu(menuName = "HumanResource/Policies/CoreChangeOnWorkPolicy")]
    public class CoreChangeOnWorkPolicy : Policy {
        [SerializedDictionary("Core Type", "Min | Max")]
        [SerializeField] public SerializedDictionary<CoreType, Vector2> Additives; 
        [SerializedDictionary("Core Type", "Min | Max")]
        [SerializeField] public SerializedDictionary<CoreType, Vector2> Multiplier; 
        [SerializeField] private bool _forAllWorkers;
        [ConditionalField(nameof(_forAllWorkers), true)] [SerializeField] private CollectionWrapperList<Worker.Worker> _workerType = new();
        public override void OnAssign() {
            //Apply bonus to all workers
            var controller = GameController.Instance.WorkerController;
            var list = new List<Worker.Worker>();
            foreach (var p in controller.WorkerList)
                list.AddRange(p.Value);
            if (!_forAllWorkers) {
                list = list.AsValueEnumerable().Where(w => _workerType.Value.AsValueEnumerable().Any(t => t.GetType() == w.GetType())).ToList();
            }
            list.ForEach(ApplyBonus);
            
            //Subscribe to add and remove worker events
            //Add bonus to new workers and remove them from deleted workers
            controller.onWorkerAdded += ApplyBonus;
            controller.onWorkerRemoved += UnapplyBonus;
        }

        protected override void ResetValues() {
            //Remove bonus from all workers
            _appliedWorkers.Clone().ForEach(UnapplyBonus);

            //Unsubscribe from add and remove workers events
            var controller = GameController.Instance.WorkerController;
            controller.onWorkerAdded -= ApplyBonus;
            controller.onWorkerRemoved -= UnapplyBonus;
        }
        
        
        
        //protected override string FormatDescription() {
        //    var text = "";

        //    foreach (CoreType core in Enum.GetValues(typeof(CoreType))) {
        //        text += _description.DescriptionFormatter(
        //            (() => Additives.ContainsKey(core), new object[] { GetFloatRange(Additives, core) }),
        //            (() => Additives.ContainsKey(core) && Multiplier.ContainsKey(core), new object[]{}),
        //            (() => Multiplier.ContainsKey(core), new object[] { GetFloatRange(Multiplier, core) }),
        //            (() => Additives.ContainsKey(core) || Multiplier.ContainsKey(core), new object[]{ Enum.GetName(typeof(CoreType), core)})
        //            ) + "\n";
        //    }
            
        //    return text;

        //    string GetFloatRange(SerializedDictionary<CoreType, Vector2> dict, CoreType key) {
        //        return Mathf.Approximately(dict.GetValueOrDefault(key).x, dict.GetValueOrDefault(key).y)
        //            ? dict.GetValueOrDefault(key).x.ToString(CultureInfo.InvariantCulture)
        //            : $"{dict.GetValueOrDefault(key).x} - {dict.GetValueOrDefault(key).y}";
        //    }
        //}

        private List<IWorker> _appliedWorkers = new();
        private void ApplyBonus(IWorker worker) {
            _appliedWorkers.Add(worker);
            worker.onCoreChanged += Evt;
                
            void Evt(CoreType core, float amount) {
                if (_appliedWorkers.Contains(worker)) OnCoreChanged(worker, core, amount);
                else worker.onCoreChanged -= Evt;
            };
        }
        
        private void UnapplyBonus(IWorker worker) {
            _appliedWorkers.Remove(worker);
        }

        private void OnCoreChanged(IWorker worker, CoreType core, float amount) {
            if (amount <= 0) return;

            var changeAmount = 0f;
            if (Additives.TryGetValue(core, out var add)) changeAmount += PickRandom(add);
            if (Multiplier.TryGetValue(core, out var multiply)) changeAmount += PickRandom(multiply) * (changeAmount + amount);
            
            worker.CurrentCores[core] += changeAmount;
        }

        private float PickRandom(Vector2 range) => new Unity.Mathematics.Random().NextFloat(range.x, range.y);

        public override SaveData Save() {
            var data = base.Save().CastToSubclass<CoreChangeOnWorkData, SaveData>();
            if (data is null) return base.Save();

            data.Additives = Additives.Select(d => new KeyValuePair<CoreType, V2>(d.Key, new(d.Value))).ToDictionary(d => d.Key, d => d.Value);
            data.Multiplier = Multiplier.Select(d => new KeyValuePair<CoreType, V2>(d.Key, new(d.Value))).ToDictionary(d => d.Key, d => d.Value);
            data.ForAllWorkers = _forAllWorkers;
            data.WorkerType = _workerType?.Value ?? new ();
            return data;
        }

        public override void Load(SaveData data) {
            base.Load(data);
            
            if (data is not CoreChangeOnWorkData coreData) return;

            Additives = new SerializedDictionary<CoreType, Vector2>(coreData.Additives.Select(d => new KeyValuePair<CoreType, Vector2>(d.Key, d.Value)));
            Multiplier = new SerializedDictionary<CoreType, Vector2>(coreData.Multiplier.Select(d => new KeyValuePair<CoreType, Vector2>(d.Key, d.Value)));
            _forAllWorkers = coreData.ForAllWorkers;
            _workerType.Value = coreData?.WorkerType ?? new();
        }

        public class CoreChangeOnWorkData : SaveData {
            public Dictionary<CoreType, V2> Additives; 
            public Dictionary<CoreType, V2> Multiplier; 
            public bool ForAllWorkers;
            public List<Worker.Worker> WorkerType;
        }
    }
}