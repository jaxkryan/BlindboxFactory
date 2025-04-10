using BuildingSystem.Models;
using Script.Controller;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace BuildingSystem
{
    public class BuildingPlacer : PersistentSingleton<BuildingPlacer>
    {
        [SerializeField]
        private Tilemap _backgroundTilemap; 

        [SerializeField]
        private TileBase _requiredTile;

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

        private bool isTouching = false;
        private float touchStartTime;
        private const float maxTouchDuration = 0.2f;

        private void Update()
        {
            if (IsbuildMode)
            {
                bool isEnoughtGold;
                Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                position.z = 0;

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
                        isTouching = true;
                        touchStartTime = Time.time; 
                    }

                    if (Input.GetMouseButtonUp(0) && isTouching)
                    {
                        float touchDuration = Time.time - touchStartTime;
                        if (touchDuration <= maxTouchDuration) // Ensure it's a quick tap
                        {
                            try
                            {
                                _constructionLayer.Stored(position);
                            }
                            catch
                            {
                                // Debug.Log("position clear");
                            }
                        }
                        isTouching = false; // Reset touch state
                    }
                }

                if (ActiveBuildable == null)
                {
                    return;
                }

                var isSpaceEmpty = _constructionLayer.IsEmpty(
                    position,
                    ActiveBuildable.UseCustomCollisionSpace ? ActiveBuildable.CollisionSpace : default
                );

                var itemCost = 0;
                if (ActiveBuildable.Cost == null)
                {
                    itemCost = 0;
                }
                else
                {
                    itemCost = ActiveBuildable.Cost;
                }
                GameController.Instance.ResourceController.TryGetAmount(Script.Resources.Resource.Gold, out long currentMoney);
                if (currentMoney < itemCost)
                {
                    isEnoughtGold = false;
                }
                else
                {
                    isEnoughtGold = true;
                }

                bool isValidTile = IsValidBuildTile(position, ActiveBuildable.CollisionSpace); 

                _previewLayer.ShowPreview(
                    ActiveBuildable,
                    position,
                    isSpaceEmpty && isValidTile, 
                    isEnoughtGold
                );

                if (Input.GetMouseButtonDown(0))
                {
                    isTouching = true;
                    touchStartTime = Time.time;
                }

                if (Input.GetMouseButtonUp(0) && isTouching)
                {
                    float touchDuration = Time.time - touchStartTime;
                    if (touchDuration <= maxTouchDuration 
                        && ActiveBuildable != null 
                        && _constructionLayer != null 
                        && isSpaceEmpty
                        && isValidTile)
                    {
                        Debug.LogWarning(ActiveBuildable.gameObject.name);
                        _constructionLayer.Build(position, ActiveBuildable);
                    }
                    isTouching = false;
                }
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

        private bool IsValidBuildTile(Vector3 worldPosition, RectInt collisionSpace)
        {
            if (_backgroundTilemap == null)
            {
                Debug.LogError("_backgroundTilemap is NULL! Make sure it's assigned in the Inspector.");
                return false;
            }

            if (_requiredTile == null)
            {
                Debug.LogError("_requiredTile is NULL! Make sure you assigned the correct tile.");
                return false;
            }

            Vector3Int cellPosition = _backgroundTilemap.WorldToCell(worldPosition);
            cellPosition.z = 0; 
                for (int x = 0; x < collisionSpace.width; x++)
                {
                    for (int y = 0; y < collisionSpace.height; y++)
                    {
                        Vector3Int offsetPosition = new Vector3Int(cellPosition.x + x, cellPosition.y + y, 0);
                        TileBase tile = _backgroundTilemap.GetTile(offsetPosition);

                        if (tile == null || tile != _requiredTile)
                        {
                            Debug.LogWarning($"Invalid tile at {offsetPosition}: {tile?.name ?? "NULL"} (Required: {_requiredTile.name})");
                            return false;
                        }
                    }
                }
                return true;
        }
    }
}
