using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.Controller;
using Script.Machine.ResourceManager;
using Script.Resources;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Script.Machine.Products {
    [Serializable]
    public class PlaceMachineProduct : SingleProductBase {
        [SerializeField] private MachineBase _parent;
        public MachineBase Building { get => _building; set => _building = value; }
        [SerializeField] MachineBase _building;
        [SerializeField] private bool _hasOffset;
        [ConditionalField(nameof(_hasOffset))][SerializeField] private Vector2Int _offsetFromParent;
        [SerializeField] bool _destroyOnComplete;
        public override void OnProductCreated() {
            if (Building is null) {
                Debug.LogError("No building to build.");
                return;
            }
            #warning Get parent grid position
            //Get parent pos from grid
            
            if (_destroyOnComplete) {
                //Remove parent from grid
            }
            
            //Spawn into parent pos + offset
            
            
            if (_destroyOnComplete) Object.Destroy(_parent.gameObject);
        }
    }
}