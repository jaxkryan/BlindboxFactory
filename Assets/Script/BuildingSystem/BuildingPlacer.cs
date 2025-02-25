using BuildingSystem.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace BuildingSystem
{
    public class BuildingPlacer : MonoBehaviour
    {
        [field:SerializeField]
        public BuildableItem ActiveBuildable {  get; private set; }
        [SerializeField]
        private float _maxBuildingDistance = 3f;

        [SerializeField]
        private ConstructionLayer _constructionLayer;

        [SerializeField]
        private PreviewLayer _previewLayer;

        [SerializeField]
        public bool _IsBuildingFromInventory { get; private set; } = false;

        [SerializeField]
        private bool _storeMode = false;

        public bool IsbuildMode = false;

        private void Update()
        {
            Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            position.z = 0;
            
            //if (!IsMouseWithinBuildableRange())
            //{
            //    _previewLayer.ClearPreview();
            //  return;
            //};
            if (IsPointerOverUI()) return;

            if (_constructionLayer == null)
            {
                _previewLayer.ClearPreview();
                return;
            }

            if (_storeMode)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    try
                    {
                        _constructionLayer.Stored(position);
                    }
                    catch
                    {
                        //Debug.Log("position clear");
                    }
                }
            }

            if (ActiveBuildable == null)
            {
                return;
            };

            var isSpaceEmpty = _constructionLayer.IsEmpty(position,
                ActiveBuildable.UseCustomCollisionSpace ? ActiveBuildable.CollisionSpace : default);

            _previewLayer.ShowPreview(
                ActiveBuildable,
                position,
                isSpaceEmpty
                );
            if (Input.GetMouseButtonUp(0) &&
                ActiveBuildable != null &&
                _constructionLayer != null &&
                isSpaceEmpty
                )
            {
                _constructionLayer.Build(position, ActiveBuildable);
            }


        }


        //private bool IsMouseWithinBuildableRange()
        //{
        //    return Vector3.Distance(_input.PointerWorldPosition, transform.position)
        //        <= _maxBuildingDistance;
        //}

        public void ClearPreview()
        {
            _previewLayer.ClearPreview();
        }
        public void SetActiveBuildable(BuildableItem item)
        {
            ActiveBuildable = item;
            _IsBuildingFromInventory = false;
        }

        public void SetBuildableFromInventory(BuildableItem item)
        {
            ActiveBuildable = item;
            _IsBuildingFromInventory = true; 
        }

        public void SetStoreMode(bool isEnabled)
        {
            _storeMode = isEnabled;
        }

        public bool GetStoreMode()
        {
            return _storeMode;
        }

        public bool IsBuildingFromInventory()
        {
            return _IsBuildingFromInventory;
        }

        public bool IsActiveBuildable()
        {
            return ActiveBuildable != null;
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            if (EventSystem.current.IsPointerOverGameObject()) return true;

            if (Application.isMobilePlatform && Input.touchCount > 0)
            {
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }

            return false;
        }
    }
}
