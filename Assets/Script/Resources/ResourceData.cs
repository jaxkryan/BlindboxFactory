using System;

namespace Script.Resources {
        
    [Serializable]
    public struct ResourceData {
        public long MaxAmount;

        public bool IsAmountValid(long currentAmount, long newAmount) {
            if (newAmount > MaxAmount) return false;
            if (newAmount < 0) return false;
                
            return true;
        }
    }
}