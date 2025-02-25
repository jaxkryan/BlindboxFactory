using System;

namespace Script.Resources {
        
    [Serializable]
    public struct ResourceData {
        public int MaxAmount;

        public bool IsAmountValid(int currentAmount, int newAmount) {
            if (newAmount > MaxAmount) return false;
            if (newAmount < 0) return false;
                
            return true;
        }
    }
}