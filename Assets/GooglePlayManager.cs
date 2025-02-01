using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using TMPro;
using System;

public class GooglePlayManager : MonoBehaviour
{
    public static GooglePlayManager Instance { get; private set; }
    public bool IsSignedIn { get; private set; } = false;
    public event Action OnSignInComplete; // Event for sign-in completion
    public TMP_Text debugText; // Assign in Inspector
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayGamesPlatform.Activate();
        SignIn();
    }

    public void SignIn()
    {
        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                Log("Google Play Login Successful! ID: " + Social.localUser.id);
                IsSignedIn = true;
                OnSignInComplete?.Invoke(); // Notify ScoreManager
            }
            else
            {
                Log("Google Play Login Failed!");
                IsSignedIn = false;
            }
        });
    }
    void Log(string message)
    {
        Debug.Log(message);
        if (debugText != null)
        {
            debugText.text = message;
        }
    }
}
