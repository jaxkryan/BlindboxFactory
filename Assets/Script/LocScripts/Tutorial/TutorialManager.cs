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
    public RectTransform clickCircle;
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
            else if (!step.isAttachButton && !step.isAttachName)
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
        Vector2 localPoint = step.overlayCenterPosition;

        // === ATTACH BUTTON ===
        if (step.isAttachButton && step.targetButton != null)
        {
            if (!step.hasHookedButton)
            {
                // Nếu có targetButton
                if (step.targetButton != null)
                {
                    var slider = step.targetButton.GetComponent<UnityEngine.UI.Slider>();
                    if (slider != null)
                    {
                        slider.onValueChanged.AddListener((value) =>
                        {
                            if (!step.isClickedButton)
                            {
                                step.isClickedButton = true;
                                OnClickOverlay();
                            }
                        });
                    }
                    else
                    {
                        var button = step.targetButton.GetComponent<UnityEngine.UI.Button>();
                        if (button != null)
                        {
                            button.onClick.AddListener(() =>
                            {
                                if (!step.isClickedButton)
                                {
                                    step.isClickedButton = true;
                                    OnClickOverlay();
                                }
                            });
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(step.targetObjectName))
                {
                    // Thử tìm lại nếu targetButton bị null nhưng có tên
                    GameObject found = GameObject.Find(step.targetObjectName);
                    if (found != null)
                    {
                        var slider = found.GetComponent<UnityEngine.UI.Slider>();
                        if (slider != null)
                        {
                            slider.onValueChanged.AddListener((value) =>
                            {
                                if (!step.isClickedButton)
                                {
                                    step.isClickedButton = true;
                                    OnClickOverlay();
                                }
                            });
                        }
                    }
                }

            }

            // Gán vị trí như bình thường
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, step.targetButton.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay.parent as RectTransform, screenPos, null, out localPoint
            );
            localPoint.y -= 20f;
        }


        // === ATTACH BY NAME ===
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
                        overlay.parent as RectTransform, screenPos, null, out localPoint
                    );
                    localPoint.y -= 50f;

                    // Nếu có Button thì gán tương tự
                    var button = target.GetComponent<UnityEngine.UI.Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() =>
                        {
                            step.isClickedButton = true;
                            OnClickOverlay();
                        });


                    }

                    Debug.Log($"[Tutorial] ✅ Found by name: {step.targetObjectName}");
                }
            }
        }

        step.overlayCenterPosition = localPoint;
        overlay.anchoredPosition = localPoint;
        overlay.localScale = step.enableOverlayScale ? step.overlayScale : Vector3.one;
        overlay.gameObject.SetActive(true);

        characterRect.anchoredPosition = step.characterPosition;
        characterRect.gameObject.SetActive(true);
        dialogueManager.StartDialogue(step.dialogueText, step.characterSprite);

        step.isClickedButton = false; // reset mỗi bước
        step.onStepStart?.Invoke();
        waitingForClick = true;
    }


    public void OnClickOverlay()
    {
        if (!waitingForClick) return;

        var step = steps[currentStep];

        if ((step.isAttachButton || step.isAttachName) && !step.isClickedButton)
        {
            Debug.Log("[Tutorial] ⛔ Step requires clicking button first!");
            return;
        }

        waitingForClick = false;
        step.onStepComplete?.Invoke();
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
