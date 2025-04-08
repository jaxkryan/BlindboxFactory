using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelMover : MonoBehaviour
{
    public RectTransform panel;
    public Vector2 movedPosition;
    public float moveDuration = 0.5f; // thời gian di chuyển (giây)

    private Vector2 originalPosition;
    private bool isMoved = false;
    private Coroutine moveCoroutine;

    void Start()
    {
        originalPosition = panel.anchoredPosition;
    }

    public void TogglePanel()
    {
        // Nếu đang chạy Coroutine cũ thì dừng lại
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        // Chạy Coroutine mới
        if (isMoved)
        {
            moveCoroutine = StartCoroutine(MovePanel(panel.anchoredPosition, originalPosition));
        }
        else
        {
            moveCoroutine = StartCoroutine(MovePanel(panel.anchoredPosition, movedPosition));
        }

        isMoved = !isMoved;
    }

    IEnumerator MovePanel(Vector2 from, Vector2 to)
    {
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            panel.anchoredPosition = Vector2.Lerp(from, to, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        panel.anchoredPosition = to; // đảm bảo đúng vị trí cuối cùng
    }
}
