using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MyBox;
using Script.Controller.Commission;
using Script.Machine;
using Script.Machine.Products;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Controller {
    [Serializable]
    public class CommissionController : ControllerBase {
        public override void Load() { throw new System.NotImplementedException(); }
        public override void Save() { throw new System.NotImplementedException(); }
        
        [Header("\tCommission Settings")]
        [SerializeField] private bool _forAllProducts;

        [ConditionalField(nameof(_forAllProducts), inverse: true)] 
        [SerializeField] private List<BoxTypeName> _commissionedProducts;
        [FormerlySerializedAs("_numberOfCommissions")] [SerializeField] private int _numberOfCommissionsPerItem;
        [SerializeField] private float _amountModifierForNextProduct;
        [SerializeField] private Vector2 _bonusRange;
        [SerializeField][Min(0)] private int _maximumTotalCommissions;
        [SerializeField][Min(0)] private int _baseCommission;
        [SerializeField][Min(0)] private int _expireHours;

        public ReadOnlyCollection<Commission.Commission> Commissions { get => _commissions.ToList().AsReadOnly(); }
        private HashSet<Commission.Commission> _commissions = new();

        public event Action OnCommissionChanged = delegate { };
        public event Action<Commission.Commission> onCommissionCompleted = delegate { };

        public HashSet<Commission.Commission> CreateCommissions() {
            var bb = _forAllProducts
                ? Enum.GetValues(typeof(BoxTypeName)).Cast<BoxTypeName>().ToList()
                : _commissionedProducts;

            var controller = GameController.Instance.BoxController;
            
            List<Commission.Commission> commissions = new ();
            foreach (var boxType in bb) {
                List<Commission.Commission> boxCommissions = new ();
                var data = BoxTypeManager.Instance.GetBoxData(boxType);
                
                //Find the number of box sold the previous day
                var prevSales = controller.SaleData.Where(s => s.BoxTypeName == boxType && s.DateTime >= DateTime.Now.AddDays(-1)).ToList();
                var prevBoxSold = prevSales.Sum(s => s.Amount);
                var prevBoxSoldPerCommission = prevBoxSold / prevSales.Count;
                //If sold 0 box or fewer than base, use the base number instead
                if (prevBoxSoldPerCommission < _baseCommission) prevBoxSold = _baseCommission;
                //Use the number of box sold as medium, find upper and lower number of box should be sold based on amount modifier
                var upperBound = _numberOfCommissionsPerItem / 2 + (_numberOfCommissionsPerItem % 2 == 0 ? 0 : 1);
                var lowerBound = _numberOfCommissionsPerItem / 2;
                List<int> newCommissionsCount = new();
                for (int i = 0; i < upperBound; i++) {
                    var modifierNumber = (int)Math.Floor(prevBoxSoldPerCommission * _amountModifierForNextProduct) - prevBoxSoldPerCommission;
                    newCommissionsCount.Add(prevBoxSoldPerCommission + modifierNumber * i);
                }
                for (int i = 0; i < lowerBound; i++) {
                    var modifierNumber = (int)Math.Floor(prevBoxSoldPerCommission * _amountModifierForNextProduct) - prevBoxSoldPerCommission;
                    newCommissionsCount.Add(prevBoxSoldPerCommission - modifierNumber * i);
                }
                //Calculate bonus 
                List<(int CommissionsCount, long Base, long Bonus)> newCommissionPayout = new();
                foreach (var count in newCommissionsCount) {
                    var basePrice = count * data.value;
                    var bonus = (long)(basePrice * new Unity.Mathematics.Random().NextDouble(_bonusRange.x, _bonusRange.y));
                    newCommissionPayout.Add((count, basePrice, bonus));
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
                }
                
                commissions.AddRange(boxCommissions);
            }
            return commissions.ToHashSet();
        }

        public bool TryAddCommission(Commission.Commission commission) {
            UpdateCommissions();
            
            if (_maximumTotalCommissions == Commissions.Count) return false;
            
            var ret =  _commissions.Add(commission);
            if (ret) OnCommissionChanged?.Invoke();
            return ret;
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


            if (Commissions.Count == 0) {
                //Notify player that there's no commission
            }
        } 
    }
}