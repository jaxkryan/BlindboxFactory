using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public List<TutorialStep> steps;
    public RectTransform overlay;
    public DialogueManager dialogueManager;
    public float clickRadius = 90f;
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
        Vector2 localPoint = step.overlayCenterPosition; // mặc định nếu không có attach nào

        // ==== ƯU TIÊN 1: ATTACH BUTTON ====
        if (step.isAttachButton && step.targetButton != null)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, step.targetButton.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay.parent as RectTransform,
                screenPos,
                null,
                out localPoint
            );
            localPoint.y -= 50f; // offset nếu muốn
            Debug.Log("[Tutorial] ✅ Attached to targetButton.");
        }

        // ==== ƯU TIÊN 2: ATTACH NAME ====
        if (step.isAttachName && !string.IsNullOrEmpty(step.targetObjectName))
        {
            GameObject found = GameObject.Find(step.targetObjectName);
            if (found != null)
            {
                RectTransform target = found.GetComponent<RectTransform>();
                if (target != null)
                {
                    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, target.position);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        overlay.parent as RectTransform,
                        screenPos,
                        null,
                        out localPoint
                    );
                    localPoint.y -= 50f;
                    Debug.Log($"[Tutorial] ✅ Found by name: {step.targetObjectName}");
                }
                else
                {
                    Debug.LogWarning($"[Tutorial] ⚠️ Object found but no RectTransform: {step.targetObjectName}");
                }
            }
            else
            {
                Debug.LogWarning($"[Tutorial] ❌ Cannot find object by name: {step.targetObjectName}");
            }
            
        }
        else
        {
            Debug.LogWarning($"[Tutorial] ❌ not attatch or null: {step.targetObjectName}");
        }

        // === Gán vị trí overlay sau khi tính ===
        step.overlayCenterPosition = localPoint;
        overlay.anchoredPosition = localPoint;
        overlay.localScale = step.enableOverlayScale ? step.overlayScale : Vector3.one;
        overlay.gameObject.SetActive(true);

        // === Nhân vật & thoại ===
        characterRect.anchoredPosition = step.characterPosition;
        characterRect.gameObject.SetActive(true);
        dialogueManager.StartDialogue(step.dialogueText, step.characterSprite);

        // === Bắt đầu bước ===
        step.onStepStart?.Invoke();
        waitingForClick = true;
    }


    public void OnClickOverlay()
    {
        if (!waitingForClick) return;

        waitingForClick = false;
        steps[currentStep].onStepComplete?.Invoke();
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
