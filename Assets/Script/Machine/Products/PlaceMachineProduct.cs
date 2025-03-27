using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using BuildingSystem;
using BuildingSystem.Models;
using MyBox;
using Script.Controller;
using Script.Machine.ResourceManager;
using Script.Resources;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Script.Machine.Products {
    [Serializable]
    public class PlaceMachineProduct : SingleProductBase {
        [SerializeField] BuildableItem _building;
        [SerializeField] private bool _hasOffset;
        [ConditionalField(nameof(_hasOffset))][SerializeField] private Vector2Int _offsetFromParent;
        [SerializeField] bool _destroyOnComplete;
        public override void OnProductCreated() {
            if (_building is null) {
                Debug.LogError("No building to build.");
                return;
            }
            #warning Get parent grid position
            //Get parent pos from grid
            var pos = _machine.Position;
            
            if (_destroyOnComplete) {
                //Remove parent from grid
                //Currently machines are spawned directly onto the scene
            }
            
            //Spawn into parent pos + offset
            var tilemap = GameController.Instance.ConstructionLayer;
            if (tilemap.TryGetComponent<ConstructionLayer>(out var ctl)) {
                var worldPos = tilemap.CellToWorld((pos + _offsetFromParent).ToVector3Int());
                ctl.Build(worldPos, _building);
            }
            
            if (_destroyOnComplete) Object.Destroy(_machine.gameObject);
        }

        public override IProduct.SaveData Save() {
            if (base.Save() is not PlaceMachineSaveData data) return base.Save();
            
            data.BuildableIndex = GameController.Instance.MachineController.Buildables.IndexOf(_building);
            data.OffsetFromParent = _hasOffset ? (0,0) : (_offsetFromParent.x, _offsetFromParent.y);
            data.DestroyOnComplete = _destroyOnComplete;

            return data;
        }

        public override void Load(IProduct.SaveData saveData) {
            BaseLoad(saveData);
            if (saveData is PlaceMachineSaveData data) {
                _building = GameController.Instance.MachineController.Buildables.ElementAtOrDefault(data.BuildableIndex);
                _hasOffset = data.OffsetFromParent == (0, 0);
                if (_hasOffset) _offsetFromParent = new Vector2Int(data.OffsetFromParent.x, data.OffsetFromParent.y);
                _destroyOnComplete = data.DestroyOnComplete;
            }
        }

        public class PlaceMachineSaveData : IProduct.SaveData {
            public int BuildableIndex;
            public (int x, int y) OffsetFromParent = (0, 0);
            public bool DestroyOnComplete;
        }
    }
}