
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string dialogueText;                   // Thoại sẽ hiển thị
    public Sprite characterSprite;                // Hình nhân vật (nếu có)
    public Vector2 overlayCenterPosition;         // Vị trí vùng lỗ trên màn hình
    public UnityEvent onStepStart;                // Gọi khi bước bắt đầu
    public UnityEvent onStepComplete;             // Gọi khi người chơi click
}
