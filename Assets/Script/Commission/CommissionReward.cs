using Script.Quest;
using Script.Resources;

namespace Script.Controller.Commission {
    public class CommissionReward : QuestReward {
        private Commission _commission;
        public void SetCommission(Commission commission) => _commission = commission;
        public override void Grant() {
            var controller = GameController.Instance.ResourceController;
            if (controller.TryGetAmount(Resource.Gold, out var amount)) {
                controller.TrySetAmount(Resource.Gold, amount + _commission.Price);
            }
        }
    }
}