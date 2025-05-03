using Script.Quest;
using Script.Resources;

namespace Script.Controller.Commission {
    public class CommissionReward : QuestReward {
        private Commission _commission;
        public void SetCommission(Commission commission) => _commission = commission;
        public override void Grant() {
            var boxController = GameController.Instance.BoxController;
            var resourceController = GameController.Instance.ResourceController;
            foreach (var i in _commission.Items)
            {
                if (boxController.TryGetAmount(i.Key, out var bAmount))
                {
                    if (bAmount < i.Value)
                        return;

                    boxController.TrySetAmount(i.Key, bAmount - i.Value);
                }
                else
                {
                    return;
                }
            }


            if (resourceController.TryGetAmount(Resource.Gold, out var rAmount))
            {
                resourceController.TrySetAmount(Resource.Gold, rAmount + _commission.Price);
            }
        }
    }
}