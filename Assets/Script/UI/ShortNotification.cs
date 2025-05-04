using System.Collections;
using TMPro;
using UnityEngine;

public class ShortNotification : MonoBehaviour
{
    public static ShortNotification Instance;

    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TMP_Text notificationText; // or TMP_Text

    private void Awake()
    {
        Instance = this;
        notificationPanel.SetActive(false);
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(ShowNotificationCoroutine(message, duration));
    }

    private IEnumerator ShowNotificationCoroutine(string message, float duration)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        notificationPanel.SetActive(false);
    }
}
