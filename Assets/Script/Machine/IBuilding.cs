using System.Collections.Generic;
using Script.Resources;

namespace Script.Machine {
    public interface IBuilding {
        int PowerUse { get; }
        Dictionary<Resource, int> ResourceUse { get; }
    }
}