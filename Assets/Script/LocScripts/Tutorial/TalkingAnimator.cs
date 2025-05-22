using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TalkingAnimator : MonoBehaviour
{
    public Image characterImage;
    public Sprite[] talkingFrames;
    public float frameRate = 0.1f;

    private Coroutine talkingRoutine;

    public void PlayTalking()
    {
        if (talkingRoutine != null)
            StopCoroutine(talkingRoutine);

        talkingRoutine = StartCoroutine(TalkingLoop());
    }

    public void StopTalking()
    {
        if (talkingRoutine != null)
            StopCoroutine(talkingRoutine);
        talkingRoutine = null;

        if (talkingFrames.Length > 0)
            characterImage.sprite = talkingFrames[0];
    }

    IEnumerator TalkingLoop()
    {
        int index = 0;
        while (true)
        {
            characterImage.sprite = talkingFrames[index];
            index = (index + 1) % talkingFrames.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }
}
