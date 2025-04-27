using Script.Controller;
using Script.Controller.SaveLoad;
using Script.Utils;
using UnityEngine;

public class TutorialStarter : ControllerBase
{
    public TutorialManager tutorialManager;
    public GameObject tutorialCanvas;  // ✅ Kéo TutorialCanvas vào đây

    // Biến trạng thái đơn giản: false = chưa xong, true = đã hoàn thành
    public bool hasCompletedTutorial ;

    void Start()
    {
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
         hasCompletedTutorial = true;
        tutorialCanvas.SetActive(false);
    }

    [System.Serializable]
    public class SaveData
    {
        public bool HasCompletedTutorial;
    }

    public override void Save(SaveManager saveManager)
    {
        var newSave = new SaveData()
        {
            HasCompletedTutorial = hasCompletedTutorial
        };

        try
        {
            var serialized = SaveManager.Serialize(newSave);
            saveManager.AddOrUpdate(this.GetType().Name, serialized);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Cannot save {GetType()}");
            Debug.LogException(ex);
            ex.RaiseException();
        }
    }

    public override void Load(SaveManager saveManager)
    {
        try
        {
            if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data) return;

            hasCompletedTutorial = data.HasCompletedTutorial;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Cannot load {GetType()}");
            Debug.LogException(ex);
            ex.RaiseException();
        }
    }
}
