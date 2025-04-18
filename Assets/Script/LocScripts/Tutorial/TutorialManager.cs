using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public List<TutorialStep> steps;
    public RectTransform overlay;                 // overlay có lỗ tròn
    public DialogueManager dialogueManager;
    //public bool clickOverlay = true;              // nếu true → click toàn màn hình, false → chỉ click quanh vùng lỗ
    public float clickRadius = 90f;               // bán kính click được khi clickOverlay = false
    public RectTransform characterRect;
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
            var step = steps[currentStep];

            if (step.clickOverlay)
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

    public bool IsWaitingForClick()
    {
        return waitingForClick;
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
        characterRect.anchoredPosition = step.characterPosition;
        // Cập nhật vị trí overlay
        overlay.anchoredPosition = step.overlayCenterPosition;
        overlay.gameObject.SetActive(true);
        // ✅ Nếu bật scale overlay → áp dụng
        if (step.enableOverlayScale)
        {
            overlay.localScale = step.overlayScale;
        }
        else
        {
            overlay.localScale = Vector3.one;
        }
        // Thoại + ảnh nhân vật

        characterRect.gameObject.SetActive(true);

        dialogueManager.StartDialogue(step.dialogueText, step.characterSprite);
        // Sự kiện khởi đầu bước
        step.onStepStart?.Invoke();

        waitingForClick = true;
    }

    public void OnClickOverlay()
    {
        if (!waitingForClick) return;

        waitingForClick = false;

        // Gọi sự kiện hoàn thành step nếu có
        steps[currentStep].onStepComplete?.Invoke();

        // Đợi 0.5 giây rồi mới NextStep
        StartCoroutine(WaitAndNextStep(0.5f));
    }

    IEnumerator WaitAndNextStep(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextStep();
    }

    public void EndTutorial()
    {
        overlay.gameObject.SetActive(false);
        Debug.Log("Tutorial complete!");
    }
}
