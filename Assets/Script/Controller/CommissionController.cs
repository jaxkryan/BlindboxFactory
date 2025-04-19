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
using Script.Resources;
using Script.UI.Mission;
using Script.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Script.Controller {
    [Serializable]
    public class CommissionController : ControllerBase {
        [Header("\tCommission Settings")] [SerializeField]
        private bool _forAllProducts;

        [ConditionalField(nameof(_forAllProducts), inverse: true)] [SerializeField]
        private List<BoxTypeName> _commissionedProducts;

        [FormerlySerializedAs("_numberOfCommissions")] [SerializeField] [Min(1)]
        private int _numberOfCommissionsPerItem;

        [FormerlySerializedAs("_amountModifierForNextProduct")] [SerializeField] [Min(1f)]
        private float _baseAmountModifierForNextProduct = 1f;

        [SerializeField] [Range(0f, 1f)] private float _amountModifierRange;

        private float _amountModifierForNextProduct => Random.Range(
            _baseAmountModifierForNextProduct - _amountModifierRange,
            _baseAmountModifierForNextProduct + _amountModifierRange);

        [SerializeField] [Min(0.1f)] private Vector2 _bonusRange;
        [SerializeField] [Min(1)] private int _maximumTotalCommissions;
        [SerializeField] [Min(1)] private int _baseCommission;
        [SerializeField] [Min(1)] private float _expireHours;
        [SerializeField] [Min(0.017f)] public float AvailableCommissionRefreshHours = 1;
        private bool _log => GameController.Instance.Log;

        public ReadOnlyCollection<Commission.Commission> Commissions {
            get => _commissions.AsReadOnly();
        }

        private List<Commission.Commission> _commissions = new();

        public event Action OnCommissionChanged = delegate { };
        public event Action<Commission.Commission> onCommissionCompleted = delegate { };

        public HashSet<Commission.Commission> CreateCommissions() {
            if (_log) Debug.Log("[Commission] Starting commission generation...");
            var bb = _forAllProducts
                ? Enum.GetValues(typeof(BoxTypeName)).Cast<BoxTypeName>().ToList()
                : _commissionedProducts;

            var controller = GameController.Instance.BoxController;

            List<Commission.Commission> commissions = new();
            foreach (var boxType in bb) {
                if (boxType == BoxTypeName.Null) continue;
                if (_log) Debug.Log($"[Commission] Generating for {boxType}...");
                List<Commission.Commission> boxCommissions = new();
                var data = BoxTypeManager.Instance.GetBoxData(boxType);

                //Find the number of box sold the previous day
                var prevSales = controller.SaleData
                    .Where(s => s.BoxTypeName == boxType && s.DateTime >= DateTime.Now.AddDays(-1)).ToList();
                var prevBoxSold = prevSales.Sum(s => s.Amount);
                //If sold 0 box or fewer than base, use the base number instead
                if (prevBoxSold < _baseCommission) prevBoxSold = _baseCommission;
                var prevBoxSoldPerCommission = prevBoxSold / (prevSales.Count == 0 ? 1 : prevSales.Count);
                if (_log)
                    Debug.Log(
                        $"[Commission] {boxType} | Prev Sold: {prevBoxSold}, Sales Count: {prevSales.Count}, Per Commission: {prevBoxSoldPerCommission}");

                //Use the number of box sold as medium, find upper and lower number of box should be sold based on amount modifier
                var lowerBound = Mathf.FloorToInt((float)_numberOfCommissionsPerItem * 1 / 2);
                var upperBound = _numberOfCommissionsPerItem - lowerBound;
                if (_log) Debug.Log($"[Commission] Commission count: {lowerBound} - {upperBound}");

                List<int> newCommissionsCount = new();
                for (int i = 1; i <= upperBound; i++) {
                    var baseNumber = newCommissionsCount.Any() ? newCommissionsCount.Max() : prevBoxSoldPerCommission;
                    var amountMod = _amountModifierForNextProduct - 1;
                    var modifierNumber = Mathf.FloorToInt(baseNumber * amountMod);

                    int count = baseNumber + modifierNumber;
                    newCommissionsCount.Add(count);

                    if (_log)
                        Debug.Log(
                            $"[Commission] Upper-bound #{i}: {count} items (Modifier: {modifierNumber}, Amount: {amountMod}).");
                }

                for (int i = 1; i <= lowerBound; i++) {
                    var baseNumber = newCommissionsCount.Any() ? newCommissionsCount.Min() : prevBoxSoldPerCommission;
                    var amountMod = _amountModifierForNextProduct - 1;
                    var modifierNumber = Mathf.FloorToInt(baseNumber * amountMod);

                    int count = baseNumber - modifierNumber;
                    newCommissionsCount.Add(count);

                    if (_log)
                        Debug.Log(
                            $"[Commission] Lower-bound #{i}: {count} items (Modifier: {modifierNumber}, Amount: {amountMod}).");
                }

                //Calculate bonus 
                List<(int CommissionsCount, long Base, long Bonus)> newCommissionPayout = new();
                foreach (var count in newCommissionsCount) {
                    var basePrice = count * data.value;
                    var bonus = (long)(basePrice
                                       * new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks).NextDouble(
                                           _bonusRange.x, _bonusRange.y));
                    newCommissionPayout.Add((count, basePrice, bonus));
                    if (_log)
                        Debug.Log($"[Commission] Payout | {count}x {boxType} -> Base: {basePrice}, Bonus: {bonus}");
                }

                //Create the commission
                foreach (var payout in newCommissionPayout) {
                    var reward = new CommissionReward();
                    var commission = new Commission.Commission.Builder()
                        .WithItem(boxType, payout.CommissionsCount)
                        .WithBonusPrice(payout.Bonus)
                        .WithValidTime(TimeSpan.FromHours(_expireHours))
                        .WithReward(reward)
                        .Build();
                    reward.SetCommission(commission);
                    boxCommissions.Add(commission);

                    if (_log)
                        Debug.Log(
                            $"[Commission] Created: {boxType} x{payout.CommissionsCount} | Base: {payout.Base}, Bonus: {payout.Bonus}, Expiry: {commission.ExpireDate}");
                }

                boxCommissions = boxCommissions.OrderBy(b => b.Items.Select(i => i.Value).Sum()).ToList();

                commissions.AddRange(boxCommissions);
            }

            if (_log) Debug.Log($"[Commission] Completed. Total generated: {commissions.Count}");
            return commissions.ToHashSet();
        }

        public bool TryAddCommission(Commission.Commission commission) {
            UpdateCommissions();

            if (_maximumTotalCommissions == Commissions.Count) return false;

            commission.Start();
            _commissions.Add(commission);
            OnCommissionChanged?.Invoke();
            return true;
        }

        public bool TryRemoveCommission(Commission.Commission commission) {
            var ret = _commissions.Remove(commission);
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

        private void Subscribe() {
            GameController.Instance.MachineController.Machines.ForEach(AddMachine);

            GameController.Instance.MachineController.onMachineAdded += AddMachine;
            GameController.Instance.MachineController.onMachineRemoved += RemoveMachine;
            GameController.Instance.ResourceController.onResourceAmountChanged += UpdateCommissions;
        }

        private void Unsubscribe() {
            GameController.Instance.MachineController.Machines.ForEach(RemoveMachine);

            GameController.Instance.MachineController.onMachineAdded -= AddMachine;
            GameController.Instance.MachineController.onMachineRemoved -= RemoveMachine;
            GameController.Instance.ResourceController.onResourceAmountChanged -= UpdateCommissions;
        }

        private void AddMachine(MachineBase machine) {
            machine.onCreateProduct += UpdateCommissions;
        }

        private void RemoveMachine(MachineBase machine) {
            machine.onCreateProduct -= UpdateCommissions;
        }

        private void UpdateCommissions(Resource resource, long l1, long l2) => UpdateCommissions();
        private void UpdateCommissions(ProductBase product) => UpdateCommissions();

        private void UpdateCommissions() {
            if (_log) Debug.Log("[Commission] Updating commissions...");
            Commissions.Where(c => c.IsFulfilled(out _)).ToList().ForEach(c => {
                c.Reward.Grant();
                onCommissionCompleted?.Invoke(c);
                TryRemoveCommission(c);
            });

            _commissions.Where(c => c.ExpireDate <= DateTime.Now).ToList().ForEach(TryRemoveCommission);

            var ui = Object.FindFirstObjectByType<MissionHubUI>(FindObjectsInactive.Include);

            if (ui) {
                if (Commissions.Count == 0 && ui.ActivePanelName != nameof(AvailableCommissionPanel)) {
                    new GameAlert.Builder(AlertType.Notification)
                        .WithHeader("Commissions Fulfilled!")
                        .WithMessage("All of your commissions have been fulfilled!")
                        .WithButton2(new AlertUIButtonDetails() {
                            Background = AlertManager.Instance.Green,
                            IsCloseButton = true,
                            Text = "Choose New",
                            TextColor = Color.white,
                            OnClick = () => {
                                Debug.LogWarning("Button clicked");


                                ui.OpenAvailableCommissionsPanel();
                            }
                        })
                        .Build().Raise();
                }
            }
        }

        public override void Load(SaveManager saveManager) {
            try {
                if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                    || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data) return;

                _forAllProducts = data.ForAllProducts;
                _commissionedProducts = data.CommissionedProducts;
                _numberOfCommissionsPerItem = data.NumberOfCommissionsPerItem;
                _baseAmountModifierForNextProduct = data.BaseAmountModifierForNextProduct;
                _amountModifierRange = data.AmountModifierRange;
                _bonusRange = new(data.BonusRange.Min, data.BonusRange.Max);
                _maximumTotalCommissions = data.MaximumTotalCommissions;
                _baseCommission = data.BaseCommission;
                _expireHours = data.ExpireHours;
                var commissions = data.Commissions.Select(c => {
                    var reward = new CommissionReward();
                    var commission = new Commission.Commission.Builder()
                        .WithItems(c.Items.Select(i => (i.Key, i.Value)).ToArray())
                        .WithValidTime(c.ValidTime)
                        .WithStartTime(c.StartTime)
                        .WithBonusPrice(c.BonusPrice)
                        .WithReward(reward)
                        .Build();
                    reward.SetCommission(commission);
                    return commission;
                }).Where(c => c != null).ToList();

                if (_log) Debug.Log($"Loaded {commissions.Count} commissions.");

                _commissions = commissions;
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
                NumberOfCommissionsPerItem = _numberOfCommissionsPerItem,
                BaseAmountModifierForNextProduct = _baseAmountModifierForNextProduct,
                AmountModifierRange = _amountModifierRange,
                BonusRange = new() { Min = _bonusRange.x, Max = _bonusRange.y },
                MaximumTotalCommissions = _maximumTotalCommissions,
                BaseCommission = _baseCommission,
                ExpireHours = _expireHours,
                Commissions = _commissions,
            };


            try {
                var serialized = SaveManager.Serialize(newSave);
                saveManager.AddOrUpdate(this.GetType().Name, serialized);
                // if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
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
            public float BaseAmountModifierForNextProduct;
            public (float Min, float Max) BonusRange;
            public int MaximumTotalCommissions;
            public int BaseCommission;
            public float ExpireHours;
            public List<Commission.Commission> Commissions;
            public float AmountModifierRange;
        }
    }
}