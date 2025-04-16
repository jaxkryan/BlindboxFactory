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
    public string UserId { get; private set; } 
    public event Action OnSignInComplete;

        // public TMP_Text debugText; // Assign in Inspector

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
                UserId = Social.localUser.id; 
                IsSignedIn = true;
                // Log("Google Play Login Successful! ID: " + UserId);
                OnSignInComplete?.Invoke();
            }
            else
            {
                UserId = null;
                IsSignedIn = false;
                // Log("Google Play Login Failed!");
            }
        });
    }

    //     void Log(string message)
    // {
    //     Debug.Log(message);
    //     if (debugText != null)
    //     {
    //         debugText.text = message;
    //     }
    // }
}