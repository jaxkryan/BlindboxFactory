using System.Collections.Generic;
using UnityEngine;

namespace Script.Controller.SaveLoad {
    public interface ISavable {
        void Save(SaveManager saveManager);
        void Load(SaveManager saveManager);
    }
}