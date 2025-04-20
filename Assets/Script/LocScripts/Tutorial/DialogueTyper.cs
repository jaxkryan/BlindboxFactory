using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public Image characterImage;
    public Animator characterAnimator;

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;

    private string currentLine;
    private Coroutine typingCoroutine;
    private Coroutine blinkingCoroutine;
    private bool isTyping;

    public void StartDialogue(string text, Sprite characterSprite = null)
    {
        dialogueBox.SetActive(true);

        // Ngắt animation cũ nếu có
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        if (blinkingCoroutine != null)
            StopCoroutine(blinkingCoroutine);

        // Cập nhật ảnh nhân vật
        if (characterSprite != null)
            characterImage.sprite = characterSprite;

        currentLine = text;

        // ✅ Bắt đầu nói + chớp mắt
        characterAnimator.SetBool("isTalking", true);


        blinkingCoroutine = StartCoroutine(BlinkRoutine());

        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        characterAnimator.SetBool("isTalking", false); // ✅ Stop talking

        if (blinkingCoroutine != null)
            StopCoroutine(blinkingCoroutine);
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            characterAnimator.SetTrigger("blink");
            yield return new WaitForSeconds(Random.Range(2f, 4f));
        }
    }

    public void SkipOrContinue()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentLine;
            characterAnimator.SetBool("isTalking", false);
            isTyping = false;

            if (blinkingCoroutine != null)
                StopCoroutine(blinkingCoroutine);
        }
        else
        {
            dialogueBox.SetActive(false);
        }
    }
}