using Script.Quest;
using Script.Resources;

namespace Script.Controller.Commission {
    public class CommissionReward : QuestReward {
        private Commission _commission;
        public void SetCommission(Commission commission) => _commission = commission;
        public override void Grant() {
            var boxController = GameController.Instance.BoxController;
            var resourceController = GameController.Instance.ResourceController;
            _commission.Items.ForEach(i => {
                if (boxController.TryGetAmount(i.Key, out var amount))
                {
                    if (amount < i.Value) return;
                    boxController.TrySetAmount(i.Key, amount - i.Value);
                }
                else return;
            });


            if (resourceController.TryGetAmount(Resource.Gold, out var amount))
            {
                resourceController.TrySetAmount(Resource.Gold, amount + _commission.Price);
            }
        }
    }
}