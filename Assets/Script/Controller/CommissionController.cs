using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MyBox;
using Newtonsoft.Json;
using Script.Alert;
using Script.Controller.Commission;
using Script.Controller.SaveLoad;
using Script.Machine;
using Script.Machine.Products;
using Script.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Controller {
    [Serializable]
    public class CommissionController : ControllerBase {
        [Header("\tCommission Settings")]
        [SerializeField] private bool _forAllProducts;
        [ConditionalField(nameof(_forAllProducts), inverse: true)] 
        [SerializeField] private List<BoxTypeName> _commissionedProducts;
        [FormerlySerializedAs("_numberOfCommissions")] [SerializeField][Min(1)] private int _numberOfCommissionsPerItem;
        [SerializeField][Min(0.01f)] private float _amountModifierForNextProduct = 1f;
        [SerializeField][Min(0.1f)] private Vector2 _bonusRange;
        [SerializeField][Min(1)] private int _maximumTotalCommissions;
        [SerializeField][Min(1)] private int _baseCommission;
        [SerializeField][Min(1)] private int _expireHours;
        [SerializeField] private bool _log = true;

        public ReadOnlyCollection<Commission.Commission> Commissions { get => _commissions.AsReadOnly(); }
        private List<Commission.Commission> _commissions = new();

        public event Action OnCommissionChanged = delegate { };
        public event Action<Commission.Commission> onCommissionCompleted = delegate { };

        public HashSet<Commission.Commission> CreateCommissions() {
            if (_log) Debug.Log($"Creating commissions");
            var bb = _forAllProducts
                ? Enum.GetValues(typeof(BoxTypeName)).Cast<BoxTypeName>().ToList()
                : _commissionedProducts;

            var controller = GameController.Instance.BoxController;
            
            List<Commission.Commission> commissions = new ();
            foreach (var boxType in bb) {
                if (boxType == BoxTypeName.Null) continue;
                if (_log) Debug.Log($"Creating commissions for {boxType}");
                List<Commission.Commission> boxCommissions = new ();
                var data = BoxTypeManager.Instance.GetBoxData(boxType);
                
                //Find the number of box sold the previous day
                var prevSales = controller.SaleData.Where(s => s.BoxTypeName == boxType && s.DateTime >= DateTime.Now.AddDays(-1)).ToList();
                var prevBoxSold = prevSales.Sum(s => s.Amount);
                //If sold 0 box or fewer than base, use the base number instead
                if (prevBoxSold < _baseCommission) prevBoxSold = _baseCommission;
                var prevBoxSoldPerCommission = prevBoxSold / (prevSales.Count == 0 ? 1 : prevSales.Count);
                if (_log) Debug.Log($"PrevBoxSold{prevBoxSold}\tPrevSalesCount{prevSales.Count}\tPrevBoxSoldPerCommission: {prevBoxSoldPerCommission}");

                //Use the number of box sold as medium, find upper and lower number of box should be sold based on amount modifier
                var lowerBound = _numberOfCommissionsPerItem / 2;
                var upperBound = lowerBound + _numberOfCommissionsPerItem + (_numberOfCommissionsPerItem % 2 == 0 ? 0 : 1);
                if (_log) Debug.Log($"Commission per item bounds {lowerBound} - {upperBound}");

                List<int> newCommissionsCount = new();
                for (int i = 0; i < upperBound; i++) {
                    var modifierNumber = (int)Math.Floor(prevBoxSoldPerCommission * _amountModifierForNextProduct) - prevBoxSoldPerCommission;
                    newCommissionsCount.Add(prevBoxSoldPerCommission + modifierNumber * i);

                    if (_log) Debug.Log($"Adding upper bound commission to counter. Amount of item: {prevBoxSoldPerCommission + modifierNumber * i}");
                }
                for (int i = 0; i < lowerBound; i++) {
                    var modifierNumber = (int)Math.Floor(prevBoxSoldPerCommission * _amountModifierForNextProduct) - prevBoxSoldPerCommission;
                    newCommissionsCount.Add(prevBoxSoldPerCommission - modifierNumber * i);
                    
                    if (_log) Debug.Log($"Adding upper bound commission to counter. Amount of item: {prevBoxSoldPerCommission - modifierNumber * i}");
                }
                //Calculate bonus 
                List<(int CommissionsCount, long Base, long Bonus)> newCommissionPayout = new();
                foreach (var count in newCommissionsCount) {
                    var basePrice = count * data.value;
                    var bonus = (long)(basePrice * new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks).NextDouble(_bonusRange.x, _bonusRange.y));
                    newCommissionPayout.Add((count, basePrice, bonus));

                    if (_log) Debug.Log($"Calculating commission payout for {count} {boxType}: Base: {basePrice}, Bonus: {bonus}");
                }
                //Create the commission
                foreach (var payout in newCommissionPayout) {
                    var reward = new CommissionReward();
                    var commission = new Commission.Commission.Builder()
                        .WithItem(boxType, payout.CommissionsCount)
                        .WithBonusPrice(payout.Bonus)
                        .WithExpiredDate(DateTime.Now.AddHours(_expireHours))
                        .WithReward(reward)
                        .Build();
                    reward.SetCommission(commission);
                    boxCommissions.Add(commission);
                    
                    if (_log) Debug.Log($"Creating commission: Item: {boxType}, Amount: {payout.CommissionsCount}, Base price: {payout.Base}, Bonus: {payout.Bonus}, Expired by: {DateTime.Now.AddHours(_expireHours)}, Reward: {reward.GetType().Name}");
                }
                
                commissions.AddRange(boxCommissions);
            }
            return commissions.ToHashSet();
        }

        public bool TryAddCommission(Commission.Commission commission) {
            UpdateCommissions();
            
            if (_maximumTotalCommissions == Commissions.Count) return false;
            
            _commissions.Add(commission);
            OnCommissionChanged?.Invoke();
            return true;
        }

        public bool TryRemoveCommission(Commission.Commission commission) {
            var ret =  _commissions.Remove(commission);
            if (ret) OnCommissionChanged?.Invoke();
            return ret;
        }

        public override void OnEnable() {
            base.OnStart();
            
            Subscribe();
        }

        public override void OnDisable() {
            base.OnDisable();
            
            Unsubscribe();
        }

        private void Subscribe(){
            var controller = GameController.Instance.MachineController;
            controller.Machines.ForEach(AddMachine);

            controller.onMachineAdded += AddMachine;
            controller.onMachineRemoved += RemoveMachine;
        }

        private void Unsubscribe() {
            var controller = GameController.Instance.MachineController;
            controller.Machines.ForEach(RemoveMachine);

            controller.onMachineAdded -= AddMachine;
            controller.onMachineRemoved -= RemoveMachine;
        }
        
        private void AddMachine(MachineBase machine) {
            machine.onCreateProduct +=  UpdateCommissions;
        }

        private void RemoveMachine(MachineBase machine) {
            machine.onCreateProduct -=  UpdateCommissions;
        }

        private void UpdateCommissions(ProductBase product) => UpdateCommissions();

        private void UpdateCommissions() {
            Commissions.Where(c => c.IsFulfilled(out _)).ForEach(c => {
                c.Reward.Grant();
                onCommissionCompleted?.Invoke(c);
                TryRemoveCommission(c);
            });

            _commissions.Where(c => c.ExpireDate <= DateTime.Now).ForEach(TryRemoveCommission);


            if (Commissions.Count == 0) {
                #warning Notify player that there's no commission
            }
        } 
        public override void Load(SaveManager saveManager) { 
            
            try {
                if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                      || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data) return;
            
                _forAllProducts = data.ForAllProducts;
                _commissionedProducts = data.CommissionedProducts;
                _numberOfCommissionsPerItem = data.NumberOfCommissionsPerItem;
                _amountModifierForNextProduct = data.AmountModifierForNextProduct;
                _bonusRange = new (data.BonusRange.Min, data.BonusRange.Max);
                _maximumTotalCommissions = data.MaximumTotalCommissions;
                _baseCommission = data.BaseCommission;
                _expireHours = data.ExpireHours;
                _commissions = data.Commissions;
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot load {GetType()}");
                Debug.LogException(ex);
                ex.RaiseException();
                return;
            }
            
            UpdateCommissions();
        }

        public override void Save(SaveManager saveManager) {
            var newSave = new SaveData() {
                ForAllProducts = _forAllProducts,
                CommissionedProducts = _commissionedProducts,
                NumberOfCommissionsPerItem =  _numberOfCommissionsPerItem,
                AmountModifierForNextProduct = _amountModifierForNextProduct,
                BonusRange = new(){Min = _bonusRange.x, Max = _bonusRange.y},
                MaximumTotalCommissions = _maximumTotalCommissions,
                BaseCommission = _baseCommission,
                ExpireHours = _expireHours,
                Commissions = _commissions,
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
                ex.RaiseException();
            }
        }

        public class SaveData {
            public bool ForAllProducts;
            public List<BoxTypeName> CommissionedProducts;
            public int NumberOfCommissionsPerItem;
            public float AmountModifierForNextProduct;
            public (float Min, float Max) BonusRange;
            public int MaximumTotalCommissions;
            public int BaseCommission;
            public int ExpireHours;
            public List<Commission.Commission> Commissions;
        }
    }
}