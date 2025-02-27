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
    public class PlaceMachineProduct : ProductBase {
        public override float MaxProgress { get => _maxProgress; }
        [SerializeField] private float _maxProgress = 100f;
        public override List<ResourceUse> ResourceUse { get => _resourceUse.Value; }
        [SerializeReference] private CollectionWrapperList<ResourceUse> _resourceUse;
        [SerializeField] private MachineBase _parent;
        [SerializeField] MachineBase _building;
        [SerializeField] private bool _hasOffset;
        [ConditionalField(nameof(_hasOffset))][SerializeField] private Vector2Int _offsetFromParent;
        [SerializeField] bool _destroyOnComplete;
        public override void OnProductCreated() {
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