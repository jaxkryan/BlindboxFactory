using System.Collections.Generic;
using Script.Resources;

namespace Script.Machine {
    public interface IBuilding {
        int PowerUse { get; }
        List<ResourceManager.ResourceUse> ResourceUse { get; }
    }
}