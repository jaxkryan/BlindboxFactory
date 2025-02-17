using System;

namespace Script.Gacha.Base {
    public interface ILoot {
        Grade Grade { get; }
    }

    public enum Grade {
        Common, Rare, Special, Epic, Legendary
    }
}