using UnityEngine;

public class TutorialStarter : MonoBehaviour
{
    public TutorialManager tutorialManager;

    // Biến trạng thái đơn giản: false = chưa xong, true = đã hoàn thành
    public bool hasCompletedTutorial = false;

    void Start()
    {
        if (!hasCompletedTutorial)
        {
            tutorialManager.StartTutorial();
            hasCompletedTutorial = true; // đánh dấu đã xong
        }
    }
    public bool IsTutorialCompleted()
    {
        return hasCompletedTutorial == true;
    }
}
