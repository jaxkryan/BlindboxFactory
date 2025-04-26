using System;
using UnityEngine;

public class SetPanelHeightToPercentageOfScreen : MonoBehaviour
{
    private void OnEnable() {
        if (TryGetComponent<RectTransform>(out var rect))
        {
            rect.sizeDelta = new Vector2(rect.rect.width, Screen.height / 2f);
        }
    }
}
