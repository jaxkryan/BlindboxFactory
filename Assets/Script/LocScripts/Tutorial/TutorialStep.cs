using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TutorialStep
{
    public string stepName;
    [TextArea] public string dialogueText;
    public Sprite characterSprite;

    public Vector2 overlayCenterPosition;
    public bool enableOverlayScale = false;
    public Vector3 overlayScale = Vector3.one;

    public Vector2 characterPosition;
    public bool isAttachButton = false;
    public RectTransform targetButton;
    public bool isAttachName = false;
    public string targetObjectName;

    public bool clickOverlay = true;

    

    public UnityEvent onStepStart;
    public UnityEvent onStepComplete;
}
