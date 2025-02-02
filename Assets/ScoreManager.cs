using UnityEngine;
using TMPro;
using System;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine.SocialPlatforms;
using GooglePlayGames.BasicApi;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text debugText; // Assign in Inspector
    private int score = 0;
    public float rotationSpeed = 100f;
    public GameObject rotatingObject;

    void Start()
    {
        if (GooglePlayManager.Instance != null && GooglePlayManager.Instance.IsSignedIn)
        {
            LoadFromCloud();
        }
        else
        {
            GooglePlayManager.Instance.OnSignInComplete += LoadFromCloud;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsClickOnUI())
            {
                IncreaseScore();
            }
            else if (IsClickOnUI())
            {
                Log("Click on save button");
                SaveGame();
            }
        }

        if (rotatingObject != null)
        {
            rotatingObject.transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
    }

    bool IsClickOnUI()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) // For 3D UI
        {
            if (hit.collider.CompareTag("UI"))
            {
                return true;
            }
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results) // Check all UI elements
        {
            if (result.gameObject.CompareTag("UI"))
            {
                return true;
            }
        }

        return false;
    }
    void IncreaseScore()
    {
        score++;
        scoreText.text = score.ToString();
    }

    public void SaveGame()
    {
        Log("Saving game...");
        SaveToCloud();
    }

    void SaveToCloud()
    {
        if (!Social.localUser.authenticated)
        {
            Log("Error: Not authenticated with Google Play when saving!");
            return;
        }

        string saveData = score.ToString();
        byte[] data = Encoding.UTF8.GetBytes(saveData);

        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            "savefile",
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("Updated score: " + score)
                        .Build();

                    PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, update, data, (commitStatus, meta) =>
                    {
                        if (commitStatus == SavedGameRequestStatus.Success)
                        {
                            Log("Game Saved! Score: " + score);
                        }
                        else
                        {
                            Log("Error: Failed to save game.");
                        }
                    });
                }
                else
                {
                    Log("Error: Failed to open save file.");
                }
            }
        );
    }

    void LoadFromCloud()
    {
        if (!Social.localUser.authenticated)
        {
            Log("Error: Not authenticated with Google Play when loading!");
            return;
        }
        else
        {
            Log("Ready to load from Google Play");
        }
        PlayGamesPlatform.Instance.SavedGame.OpenWithAutomaticConflictResolution(
            "savefile",
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, (readStatus, data) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success)
                        {
                            string saveData = Encoding.UTF8.GetString(data);
                            if (int.TryParse(saveData, out int savedScore))
                            {
                                score = savedScore;
                                scoreText.text = score.ToString();
                                Log("Game Loaded! Score: " + score);
                            }
                        }
                        else
                        {
                            Log("Error: Failed to read save data.");
                        }
                    });
                }
                else
                {
                    Log("Error: Failed to open save file.");
                }
            }
        );
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
