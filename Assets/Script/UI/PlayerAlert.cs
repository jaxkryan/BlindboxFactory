using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAlert : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI alertText;
    [SerializeField] private GameObject alertPanel;
    [SerializeField] private Button button;

    [SerializeField] DotweenAnimation dotweenAnimation;
    bool buttonShrink = false;

    private void Start()
    {
        alertPanel.SetActive(false);
        button.onClick.AddListener(togglePanel);
    }

    public void unShrink()
    {
        buttonShrink = !buttonShrink;
    }

    public void togglePanel()
    {
        if(buttonShrink)
        {
            dotweenAnimation.SlideInFromLeft();
            unShrink();
        }
        else
        {
            dotweenAnimation.SlideOutToLeft();
            unShrink();
        }
    }

    public void ShowAlert(string message)
    {
        alertText.text = message;
        alertPanel.SetActive(true);
        button.gameObject.SetActive(true);
    }

    public void HideAlert()
    {
        alertPanel.SetActive(false);
        button.gameObject.SetActive(false);
    }
}
