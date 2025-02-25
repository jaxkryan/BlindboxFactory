using UnityEngine;

namespace Script.Machine {
    public interface IProduct {
        float MaxProgress { get; }
        void OnProductCreated();
    }
}