using System.Collections.Generic;
using UnityEngine;

namespace Script.Controller.SaveLoad {
    public interface ISavable {
        void Save();
        void Load();
    }
}