using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using DG.Tweening;
using Script.Utils;

[System.Serializable]
public class SettingPreferences
{
    public float musicVolume = 0.75f;
    public float sfxVolume = 0.75f;
    public int language = 0; // 0 = English, 1 = Vietnamese
}

public class SettingUI : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;
    public GameObject settingsCanvas;

    private string savePath;
    private SettingPreferences settings;
    private Vector3 originalPosition; // To store the panel's original position

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "SettingPreference.json");
        LoadSettings();

        // Store the original position and hide the canvas
        originalPosition = settingsCanvas.transform.position;
        settingsCanvas.transform.localScale = Vector3.zero; // Start scaled down
        settingsCanvas.SetActive(false);
    }

    private void Start()
    {
        // Apply settings
        musicSlider.value = settings.musicVolume;
        sfxSlider.value = settings.sfxVolume;
        SetMusicVolume();
        SetSFXVolume();
        SetLocale(settings.language);
    }

    public void SetMusicVolume()
    {
        settings.musicVolume = musicSlider.value;
        audioMixer.SetFloat("Music", Mathf.Log10(settings.musicVolume) * 20);
        SaveSettings();
    }

    public void SetSFXVolume()
    {
        settings.sfxVolume = sfxSlider.value;
        audioMixer.SetFloat("SFX", Mathf.Log10(settings.sfxVolume) * 20);
        SaveSettings();
    }

    public void OpenSettingMenu()
    {
        settingsCanvas.SetActive(true);

        // Start off-screen (e.g., from the left side of the screen)
        Vector3 startPosition = originalPosition + Vector3.left * 500f; // Adjust offset as needed
        settingsCanvas.transform.position = startPosition;
        settingsCanvas.transform.localScale = Vector3.zero; // Start small

        // Create a sequence: slide in, then scale up
        Sequence openSequence = DOTween.Sequence();
        openSequence
            .Append(settingsCanvas.transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutQuad)) // Slide in
            .Append(settingsCanvas.transform.DOScale(Vector3.one * 1.05f, 0.2f).SetEase(Ease.OutBack)) // Slightly overscale
            .Append(settingsCanvas.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InOutQuad)); // Settle to normal size
    }

    public void CloseSettingMenu()
    {
        // Create a sequence: scale down, then slide out
        Sequence closeSequence = DOTween.Sequence();
        closeSequence
            .Append(settingsCanvas.transform.DOScale(Vector3.one * 1.05f, 0.1f).SetEase(Ease.InOutQuad)) // Slight overscale
            .Append(settingsCanvas.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)) // Scale down
            .Append(settingsCanvas.transform.DOMove(originalPosition + Vector3.left * 500f, 0.3f).SetEase(Ease.InQuad)) // Slide out
            .OnComplete(() => settingsCanvas.SetActive(false)); // Deactivate when done
    }

    public void SetEnglish()
    {
        settings.language = 0; 
        LocalizationExtension.SetLocale(settings.language);

        SaveSettings();
    }

    public void SetVietnamese()
    {
        settings.language = 1;
        LocalizationExtension.SetLocale(settings.language);
        SaveSettings();
    }

    private void SetLocale(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }

    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(savePath, json);
    }

    private void LoadSettings()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            settings = JsonUtility.FromJson<SettingPreferences>(json);
        }
        else
        {
            settings = new SettingPreferences();
            SaveSettings();
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
    }
}