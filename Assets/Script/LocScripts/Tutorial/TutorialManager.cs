using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public List<TutorialStep> steps;
    public RectTransform overlay;                 // overlay có lỗ tròn
    public DialogueManager dialogueManager;
    public bool clickOverlay = true;              // nếu true → click toàn màn hình, false → chỉ click quanh vùng lỗ
    public float clickRadius = 90f;               // bán kính click được khi clickOverlay = false

    private int currentStep = -1;
    private bool waitingForClick = false;

    void Start()
    {
        StartTutorial();
    }

    void Update()
    {
        if (!waitingForClick) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (clickOverlay)
            {
                OnClickOverlay();
            }
            else
            {
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    overlay.parent as RectTransform,
                    Input.mousePosition,
                    null,
                    out localMousePos
                );

                float distance = Vector2.Distance(localMousePos, overlay.anchoredPosition);
                if (distance <= clickRadius)
                {
                    OnClickOverlay();
                }
            }
        }
    }

    public void StartTutorial()
    {
        currentStep = -1;
        NextStep();
    }

    public void NextStep()
    {
        currentStep++;

        if (currentStep >= steps.Count)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = steps[currentStep];

        // Cập nhật vị trí overlay
        overlay.anchoredPosition = step.overlayCenterPosition;
        overlay.gameObject.SetActive(true);

        // Thoại + ảnh nhân vật
        dialogueManager.StartDialogue(step.dialogueText, step.characterSprite);

        // Sự kiện khởi đầu bước
        step.onStepStart?.Invoke();

        waitingForClick = true;
    }

    public void OnClickOverlay()
    {
        if (!waitingForClick) return;

        waitingForClick = false;

        steps[currentStep].onStepComplete?.Invoke();
        NextStep();
    }

    public void EndTutorial()
    {
        overlay.gameObject.SetActive(false);
        Debug.Log("Tutorial complete!");
    }
}
