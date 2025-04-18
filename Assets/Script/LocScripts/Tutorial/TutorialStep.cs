
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string dialogueText;                   // Thoại sẽ hiển thị
    public Sprite characterSprite;                // Hình nhân vật (nếu có)
    public Vector2 overlayCenterPosition;         // Vị trí vùng lỗ trên màn hình
    public Vector2 characterPosition;
    public bool clickOverlay = true;
    public bool enableOverlayScale = false;   // bật/tắt scale
    public Vector2 overlayScale = Vector2.one; // scale cụ thể
    public UnityEvent onStepStart;                // Gọi khi bước bắt đầu
    public UnityEvent onStepComplete;             // Gọi khi người chơi click
}
