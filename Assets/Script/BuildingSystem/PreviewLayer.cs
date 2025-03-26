
using BuildingSystem.Models;
using UnityEngine;
namespace BuildingSystem
{
    public class PreviewLayer : TilemapLayer
    {
        [SerializeField] 
        private SpriteRenderer _previewRenderer;

        public void ShowPreview(BuildableItem item, Vector3 worldCoords, bool isValid, bool isEnoughtGold)
        {
            var coords = _tilemap.WorldToCell(worldCoords);

            _previewRenderer.enabled = true;

            Vector3 position = _tilemap.GetCellCenterLocal(coords);

            position.z = -8;

            _previewRenderer.sortingOrder = 10000;

            _previewRenderer.transform.position = position;
            if (item.gameObject != null)
            {
                Sprite targetSprite = item.gameObject?.GetComponent<SpriteRenderer>()?.sprite ?? item.PreviewSprite;
                _previewRenderer.sprite = targetSprite;

                _previewRenderer.transform.localScale = item.gameObject?.transform.localScale ?? Vector3.one;

                if (targetSprite != null)
                {
                    Vector3 itemScale = item.gameObject?.transform.localScale ?? Vector3.one;
                    _previewRenderer.transform.localScale = new Vector3(itemScale.x, itemScale.y, 1);
                }
            }
            else
            {
                _previewRenderer.sprite = item.PreviewSprite;
                _previewRenderer.transform.localScale = Vector3.one;
            }
            if (isEnoughtGold)
            {
                if (isValid)
                {
                    _previewRenderer.color = new Color(0, 1, 0, 0.5f);
                }
                else
                {
                    _previewRenderer.color = new Color(1, 0, 0, 0.5f);
                }
            }
            else
            {
                _previewRenderer.color = new Color(1, 1, 0, 0.5f);
            }
        }



        public void ClearPreview ()
        {
            _previewRenderer.enabled = false;
        }
    }
}