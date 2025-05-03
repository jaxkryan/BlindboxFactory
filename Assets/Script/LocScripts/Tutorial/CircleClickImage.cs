using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CircleClickImage : Image
{
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        RectTransform rectTransform = transform as RectTransform;

        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);

        float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;

        return local.magnitude <= radius;
    }
}
