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
    private bool isTyping;

    public void StartDialogue(string text, Sprite characterSprite = null)
    {
        dialogueBox.SetActive(true);
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        if (characterSprite != null)
            characterImage.sprite = characterSprite;

        currentLine = text;
        typingCoroutine = StartCoroutine(TypeLine(currentLine));
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        isTyping = true;

        characterAnimator.SetBool("isTalking", true);

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        characterAnimator.SetBool("isTalking", false);
    }

    public void SkipOrContinue()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentLine;
            characterAnimator.SetBool("isTalking", false);
            isTyping = false;
        }
        else
        {
            // You can move to next dialogue here
            dialogueBox.SetActive(false);
        }
    }
}
