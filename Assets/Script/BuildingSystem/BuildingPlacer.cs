using BuildingSystem.Models;
using GameInput;
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
        private CrossPlatformInputUser _input;
        [SerializeField]
        public bool _IsBuildingFromInventory { get; private set; } = false;

        private void Update()
        {
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

            if (_input.IsInputButtonPressed(InputButton.Secondary))
            {
                try
                {
                    _constructionLayer.Destroy(_input.PointerWorldPosition);
                }
                catch
                {
                    //Debug.Log("position clear");
                }
            }

            if (ActiveBuildable == null)
            {
                return;
            };

            var isSpaceEmpty = _constructionLayer.IsEmpty(_input.PointerWorldPosition,
                ActiveBuildable.UseCustomCollisionSpace ? ActiveBuildable.CollisionSpace : default);

            _previewLayer.ShowPreview(
                ActiveBuildable,
                _input.PointerWorldPosition,
                isSpaceEmpty
                );
            if (_input.IsInputButtonPressed(InputButton.Primary) &&
                ActiveBuildable != null &&
                _constructionLayer != null &&
                isSpaceEmpty
                )
            {
                _constructionLayer.Build(_input.PointerWorldPosition, ActiveBuildable);
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

        public bool IsBuildingFromInventory()
        {
            return _IsBuildingFromInventory;
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
