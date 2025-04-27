using System;
using Script.Controller;
using UnityEngine;

public class TutorialStarter : MonoBehaviour
{
    public TutorialManager tutorialManager;
    public GameObject tutorialCanvas;  // ✅ Kéo TutorialCanvas vào đây

    // Biến trạng thái đơn giản: false = chưa xong, true = đã hoàn thành
    public bool hasCompletedTutorial => GameController.Instance.CompletedTutorial;

    private void Start() {
        GameController.Instance.onLoad += onLoad;
    }

    void onLoad()
    {
        GameController.Instance.onLoad -= onLoad;
        if (!hasCompletedTutorial)
        {
            tutorialManager.StartTutorial();
            
        }
        else
        {
            if (tutorialCanvas != null)
            {
                tutorialCanvas.SetActive(false);  // ✅ Tắt canvas nếu đã xong
            }
        }
    }

    public void IsTutorialCompleted()
    {
         // hasCompletedTutorial = true;
         GameController.Instance.FinishTutorial();
        tutorialCanvas.SetActive(false);
    }
}
