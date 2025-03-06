using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class SettingPreferences
{
    public float musicVolume = 0.75f;
    public float sfxVolume = 0.75f;
    public int language = 0; // 0 = English, 1 = Vietnamese
}

public class SettingMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;
    public GameObject settingsCanvas;

    private string savePath;
    private SettingPreferences settings;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "SettingPreference.json");
        LoadSettings();

        // Ẩn menu và reset scale ban đầu
        settingsCanvas.transform.localScale = Vector3.zero;
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
        settingsCanvas.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }

    public void CloseSettingMenu()
    {
        settingsCanvas.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            settingsCanvas.SetActive(false);
        });
    }

    public void SetEnglish()
    {
        settings.language = 0;
        SetLocale(0);
        SaveSettings();
    }

    public void SetVietnamese()
    {
        settings.language = 1;
        SetLocale(1);
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
